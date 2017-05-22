// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Track;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Framework.Logging;
using osu.Framework.Screens;
using osu.Framework.Timing;
using osu.Game.Configuration;
using osu.Game.Database;
using osu.Game.Rulesets;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Backgrounds;
using System;
using System.Linq;
using osu.Framework.Threading;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Ranking;

namespace osu.Game.Screens.Play
{
    public class Player : OsuScreen
    {
        protected override BackgroundScreen CreateBackground() => new BackgroundScreenBeatmap(Beatmap);

        internal override bool ShowOverlays => false;

        internal override bool HasLocalCursorDisplayed => !pauseContainer.IsPaused && !HasFailed && HitRenderer.ProvidingUserCursor;

        public BeatmapInfo BeatmapInfo;

        public Action RestartRequested;

        internal override bool AllowRulesetChange => false;

        public bool HasFailed { get; private set; }

        public int RestartCount;

        private IAdjustableClock adjustableSourceClock;
        private FramedOffsetClock offsetClock;
        private DecoupleableInterpolatingFramedClock decoupledClock;

        private PauseContainer pauseContainer;

        private RulesetInfo ruleset;

        private ScoreProcessor scoreProcessor;
        protected HitRenderer HitRenderer;

        #region User Settings

        private Bindable<double> dimLevel;
        private Bindable<bool> mouseWheelDisabled;
        private Bindable<double> userAudioOffset;

        #endregion

        private HUDOverlay hudOverlay;
        private FailOverlay failOverlay;

        [BackgroundDependencyLoader(permitNulls: true)]
        private void load(AudioManager audio, BeatmapDatabase beatmaps, OsuConfigManager config, OsuGame osu)
        {
            dimLevel = config.GetBindable<double>(OsuSetting.DimLevel);
            mouseWheelDisabled = config.GetBindable<bool>(OsuSetting.MouseDisableWheel);

            Ruleset rulesetInstance;

            try
            {
                if (Beatmap == null)
                    Beatmap = beatmaps.GetWorkingBeatmap(BeatmapInfo, withStoryboard: true);

                if (Beatmap?.Beatmap == null)
                    throw new InvalidOperationException("Beatmap was not loaded");

                ruleset = osu?.Ruleset.Value ?? Beatmap.BeatmapInfo.Ruleset;
                rulesetInstance = ruleset.CreateInstance();

                try
                {
                    HitRenderer = rulesetInstance.CreateHitRendererWith(Beatmap, ruleset.ID == Beatmap.BeatmapInfo.Ruleset.ID);
                }
                catch (BeatmapInvalidForRulesetException)
                {
                    // we may fail to create a HitRenderer if the beatmap cannot be loaded with the user's preferred ruleset
                    // let's try again forcing the beatmap's ruleset.
                    ruleset = Beatmap.BeatmapInfo.Ruleset;
                    rulesetInstance = ruleset.CreateInstance();
                    HitRenderer = rulesetInstance.CreateHitRendererWith(Beatmap, true);
                }

                if (!HitRenderer.Objects.Any())
                    throw new InvalidOperationException("Beatmap contains no hit objects!");
            }
            catch (Exception e)
            {
                Logger.Log($"Could not load this beatmap sucessfully ({e})!", LoggingTarget.Runtime, LogLevel.Error);

                //couldn't load, hard abort!
                Exit();
                return;
            }

            Track track = Beatmap.Track;

            if (track != null)
            {
                audio.Track.SetExclusive(track);
                adjustableSourceClock = track;
            }

            adjustableSourceClock = (IAdjustableClock)track ?? new StopwatchClock();

            decoupledClock = new DecoupleableInterpolatingFramedClock { IsCoupled = false };

            var firstObjectTime = HitRenderer.Objects.First().StartTime;
            decoupledClock.Seek(Math.Min(0, firstObjectTime - Math.Max(Beatmap.Beatmap.TimingInfo.BeatLengthAt(firstObjectTime) * 4, Beatmap.BeatmapInfo.AudioLeadIn)));
            decoupledClock.ProcessFrame();

            offsetClock = new FramedOffsetClock(decoupledClock);

            userAudioOffset = config.GetBindable<double>(OsuSetting.AudioOffset);
            userAudioOffset.ValueChanged += v => offsetClock.Offset = v;
            userAudioOffset.TriggerChange();

            Schedule(() =>
            {
                adjustableSourceClock.Reset();

                foreach (var mod in Beatmap.Mods.Value.OfType<IApplicableToClock>())
                    mod.ApplyToClock(adjustableSourceClock);

                decoupledClock.ChangeSource(adjustableSourceClock);
            });

            Children = new Drawable[]
            {
                pauseContainer = new PauseContainer
                {
                    AudioClock = decoupledClock,
                    FramedClock = offsetClock,
                    OnRetry = Restart,
                    OnQuit = Exit,
                    CheckCanPause = () => ValidForResume && !HasFailed,
                    Retries = RestartCount,
                    OnPause = () => {
                        hudOverlay.KeyCounter.IsCounting = pauseContainer.IsPaused;
                    },
                    OnResume = () => {
                        hudOverlay.KeyCounter.IsCounting = true;
                    },
                    Children = new Drawable[]
                    {
                        new SkipButton(firstObjectTime) { AudioClock = decoupledClock },
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Clock = offsetClock,
                            Children = new Drawable[]
                            {
                                HitRenderer,
                            }
                        },
                        hudOverlay = new StandardHUDOverlay
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre
                        },
                    }
                },
                failOverlay = new FailOverlay
                {
                    OnRetry = Restart,
                    OnQuit = Exit,
                },
                new HotkeyRetryOverlay
                {
                    Action = () => {
                        //we want to hide the hitrenderer immediately (looks better).
                        //we may be able to remove this once the mouse cursor trail is improved.
                        HitRenderer?.Hide();
                        Restart();
                    },
                }
            };

            scoreProcessor = HitRenderer.CreateScoreProcessor();

            hudOverlay.KeyCounter.Add(rulesetInstance.CreateGameplayKeys());
            hudOverlay.BindProcessor(scoreProcessor);
            hudOverlay.BindHitRenderer(HitRenderer);

            hudOverlay.Progress.Objects = HitRenderer.Objects;
            hudOverlay.Progress.AudioClock = decoupledClock;
            hudOverlay.Progress.AllowSeeking = HitRenderer.HasReplayLoaded;
            hudOverlay.Progress.OnSeek = pos => decoupledClock.Seek(pos);

            hudOverlay.ModDisplay.Current.BindTo(Beatmap.Mods);

            //bind HitRenderer to ScoreProcessor and ourselves (for a pass situation)
            HitRenderer.OnAllJudged += onCompletion;

            //bind ScoreProcessor to ourselves (for a fail situation)
            scoreProcessor.Failed += onFail;
        }

        public void Restart()
        {
            ValidForResume = false;
            RestartRequested?.Invoke();
            Exit();
        }

        private ScheduledDelegate onCompletionEvent;

        private void onCompletion()
        {
            // Only show the completion screen if the player hasn't failed
            if (scoreProcessor.HasFailed || onCompletionEvent != null)
                return;

            ValidForResume = false;

            using (BeginDelayedSequence(1000))
            {
                onCompletionEvent = Schedule(delegate
                {
                    var score = new Score
                    {
                        Beatmap = Beatmap.BeatmapInfo,
                        Ruleset = ruleset
                    };
                    scoreProcessor.PopulateScore(score);
                    score.User = HitRenderer.Replay?.User ?? (Game as OsuGame)?.API?.LocalUser?.Value;
                    Push(new Results(score));
                });
            }
        }

        private void onFail()
        {
            decoupledClock.Stop();

            HasFailed = true;
            failOverlay.Retries = RestartCount;
            failOverlay.Show();
        }

        protected override void OnEntering(Screen last)
        {
            base.OnEntering(last);

            (Background as BackgroundScreenBeatmap)?.BlurTo(Vector2.Zero, 1500, EasingTypes.OutQuint);
            Background?.FadeTo(1 - (float)dimLevel, 1500, EasingTypes.OutQuint);

            Content.Alpha = 0;

            dimLevel.ValueChanged += newDim => Background?.FadeTo(1 - (float)newDim, 800);

            Content.ScaleTo(0.7f);

            using (Content.BeginDelayedSequence(250))
                Content.FadeIn(250);

            Content.ScaleTo(1, 750, EasingTypes.OutQuint);

            using (BeginDelayedSequence(750))
                Schedule(() =>
                {
                    if (!pauseContainer.IsPaused)
                        decoupledClock.Start();

                });

            pauseContainer.Alpha = 0;
            pauseContainer.FadeIn(750, EasingTypes.OutQuint);
        }

        protected override void OnSuspending(Screen next)
        {
            fadeOut();
            base.OnSuspending(next);
        }

        protected override bool OnExiting(Screen next)
        {
            if (HasFailed || !ValidForResume || pauseContainer.AllowExit || HitRenderer.HasReplayLoaded)
            {
                fadeOut();
                return base.OnExiting(next);
            }

            pauseContainer.Pause();
            return true;
        }

        private void fadeOut()
        {
            const float fade_out_duration = 250;

            HitRenderer?.FadeOut(fade_out_duration);
            Content.FadeOut(fade_out_duration);

            hudOverlay.ScaleTo(0.7f, fade_out_duration * 3, EasingTypes.In);

            Background?.FadeTo(1f, fade_out_duration);
        }

        protected override bool OnWheel(InputState state) => mouseWheelDisabled.Value && !pauseContainer.IsPaused;
    }
}
