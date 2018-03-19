using System;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Input;
using osu.Framework.Logging;
using osu.Framework.Screens;
using osu.Framework.Threading;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Cursor;
using osu.Game.Online.API;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Backgrounds;
using osu.Game.Screens.Ranking;
using osu.Game.Storyboards.Drawables;
using OpenTK;
using osu.Game.Screens;
using osu.Game.Screens.Play;
using OpenTK.Input;
using Symcol.Rulesets.Core.Multiplayer.Networking;
using Symcol.Rulesets.Core.Multiplayer.Pieces;
using System.Collections.Generic;
using Symcol.Core.Networking;

namespace Symcol.Rulesets.Core.Multiplayer.Screens
{
    public class MultiPlayer : ScreenWithBeatmapBackground, IProvideCursor
    {
        public readonly RulesetNetworkingClientHandler RulesetNetworkingClientHandler;

        protected override BackgroundScreen CreateBackground() => new BackgroundScreenBeatmap(Beatmap);

        protected override float BackgroundParallaxAmount => 0.1f;

        public override bool ShowOverlaysOnEnter => false;

        public Action RestartRequested;

        public override bool AllowBeatmapRulesetChange => false;

        public bool HasFailed { get; private set; }

        public bool AllowPause { get; set; } = false;
        public bool AllowLeadIn { get; set; } = true;
        public bool AllowResults { get; set; } = true;

        public int RestartCount;

        public CursorContainer Cursor => RulesetContainer.Cursor;
        public bool ProvidingUserCursor => RulesetContainer?.Cursor != null && !RulesetContainer.HasReplayLoaded.Value;

        private IAdjustableClock sourceClock;
        private DecoupleableInterpolatingFramedClock adjustableClock;

        private RulesetInfo ruleset;

        private APIAccess api;

        private ScoreProcessor scoreProcessor;
        protected RulesetContainer RulesetContainer;

        #region User Settings

        private Bindable<double> dimLevel;
        private Bindable<double> blurLevel;
        private Bindable<bool> showStoryboard;
        private Bindable<bool> mouseWheelDisabled;
        private Bindable<double> userAudioOffset;

        private SampleChannel sampleRestart;

        #endregion

        private Container storyboardContainer;
        private DrawableStoryboard storyboard;

        private HUDOverlay hudOverlay;

        private MultiplayerScoreboard scoreboard;

        private bool loadedSuccessfully => RulesetContainer?.Objects.Any() == true;

        private readonly List<ClientInfo> playerList;

        public MultiPlayer(RulesetNetworkingClientHandler rulesetNetworkingClientHandler, List<ClientInfo> playerList)//, WorkingBeatmap beatmap = null)
        {
            RulesetNetworkingClientHandler = rulesetNetworkingClientHandler;
            RulesetNetworkingClientHandler.OnAbort = () => Exit();
            RulesetNetworkingClientHandler.StartGame = () => start();
            this.playerList = playerList;
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio, APIAccess api, OsuConfigManager config)
        {
            this.api = api;

            dimLevel = config.GetBindable<double>(OsuSetting.DimLevel);
            blurLevel = config.GetBindable<double>(OsuSetting.BlurLevel);
            showStoryboard = config.GetBindable<bool>(OsuSetting.ShowStoryboard);

            mouseWheelDisabled = config.GetBindable<bool>(OsuSetting.MouseDisableWheel);

            sampleRestart = audio.Sample.Get(@"Gameplay/restart");
            userAudioOffset = config.GetBindable<double>(OsuSetting.AudioOffset);

            WorkingBeatmap working = Beatmap.Value;
            Beatmap beatmap;

            try
            {
                beatmap = working.Beatmap;

                if (beatmap == null)
                    throw new InvalidOperationException("Beatmap was not loaded");

                ruleset = Ruleset.Value ?? beatmap.BeatmapInfo.Ruleset;
                var rulesetInstance = ruleset.CreateInstance();

                try
                {
                    RulesetContainer = rulesetInstance.CreateRulesetContainerWith(working, ruleset.ID == beatmap.BeatmapInfo.Ruleset.ID);
                }
                catch (BeatmapInvalidForRulesetException)
                {
                    // we may fail to create a RulesetContainer if the beatmap cannot be loaded with the user's preferred ruleset
                    // let's try again forcing the beatmap's ruleset.
                    ruleset = beatmap.BeatmapInfo.Ruleset;
                    rulesetInstance = ruleset.CreateInstance();
                    RulesetContainer = rulesetInstance.CreateRulesetContainerWith(Beatmap, true);
                }

                if (!RulesetContainer.Objects.Any())
                    throw new InvalidOperationException("Beatmap contains no hit objects!");
            }
            catch (Exception e)
            {
                Logger.Error(e, "Could not load beatmap sucessfully!");

                //couldn't load, hard abort!
                Exit();
                return;
            }

            sourceClock = (IAdjustableClock)working.Track ?? new StopwatchClock();
            adjustableClock = new DecoupleableInterpolatingFramedClock { IsCoupled = false };

            var firstObjectTime = RulesetContainer.Objects.First().StartTime;
            adjustableClock.Seek(AllowLeadIn
                ? Math.Min(0, firstObjectTime - Math.Max(beatmap.ControlPointInfo.TimingPointAt(firstObjectTime).BeatLength * 4, beatmap.BeatmapInfo.AudioLeadIn))
                : firstObjectTime);

            adjustableClock.ProcessFrame();

            // the final usable gameplay clock with user-set offsets applied.
            var offsetClock = new FramedOffsetClock(adjustableClock);

            userAudioOffset.ValueChanged += v => offsetClock.Offset = v;
            userAudioOffset.TriggerChange();

            scoreProcessor = RulesetContainer.CreateScoreProcessor();

            Children = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Clock = offsetClock,
                    Children = new Drawable[]
                    {
                        storyboardContainer = new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Alpha = 0,
                        },
                        RulesetContainer,
                        hudOverlay = new HUDOverlay(scoreProcessor, RulesetContainer, working, offsetClock, adjustableClock)
                        {
                            Clock = Clock, // hud overlay doesn't want to use the audio clock directly
                            ProcessCustomClock = false,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre
                        },
                        new BreakOverlay(beatmap.BeatmapInfo.LetterboxInBreaks, scoreProcessor)
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            ProcessCustomClock = false,
                            Breaks = beatmap.Breaks
                        },
                        scoreboard = new MultiplayerScoreboard(RulesetNetworkingClientHandler, playerList, scoreProcessor)
                    }
                }
            };

            if (showStoryboard)
                initializeStoryboard(false);

            // Bind ScoreProcessor to ourselves
            scoreProcessor.AllJudged += onCompletion;
            scoreProcessor.Failed += onFail;

            foreach (var mod in Beatmap.Value.Mods.Value.OfType<IApplicableToScoreProcessor>())
                mod.ApplyToScoreProcessor(scoreProcessor);
        }

        private void applyRateFromMods()
        {
            if (sourceClock == null) return;

            sourceClock.Rate = 1;
            foreach (var mod in Beatmap.Value.Mods.Value.OfType<IApplicableToClock>())
                mod.ApplyToClock(sourceClock);
        }

        private void initializeStoryboard(bool asyncLoad)
        {
            if (storyboardContainer == null)
                return;

            var beatmap = Beatmap.Value;

            storyboard = beatmap.Storyboard.CreateDrawable();
            storyboard.Masking = true;

            if (asyncLoad)
                LoadComponentAsync(storyboard, storyboardContainer.Add);
            else
                storyboardContainer.Add(storyboard);
        }

        private ScheduledDelegate onCompletionEvent;

        private void onCompletion()
        {
            // Only show the completion screen if the player hasn't failed
            if (scoreProcessor.HasFailed || onCompletionEvent != null)
                return;

            ValidForResume = false;

            if (!AllowResults) return;

            using (BeginDelayedSequence(1000))
            {
                onCompletionEvent = Schedule(delegate
                {
                    if (!IsCurrentScreen) return;

                    var score = new Score
                    {
                        Beatmap = Beatmap.Value.BeatmapInfo,
                        Ruleset = ruleset
                    };
                    scoreProcessor.PopulateScore(score);
                    score.User = RulesetContainer.Replay?.User ?? api.LocalUser.Value;
                    Push(new Results(score));
                });
            }
        }

        private bool onFail()
        {
            if (Beatmap.Value.Mods.Value.OfType<IApplicableFailOverride>().Any(m => !m.AllowFail))
                return false;

            HasFailed = true;
            return true;
        }

        protected override void OnEntering(Screen last)
        {
            base.OnEntering(last);

            Add(RulesetNetworkingClientHandler);

            if (!loadedSuccessfully)
                return;

            Content.Alpha = 0;
            Content
                .ScaleTo(0.7f)
                .ScaleTo(1, 750, Easing.OutQuint)
                .Delay(250)
                .FadeIn(250);

            Task.Run(() =>
            {
                sourceClock.Reset();

                Schedule(() =>
                {
                    adjustableClock.ChangeSource(sourceClock);
                    applyRateFromMods();

                    this.Delay(750).Schedule(() =>
                    {
                        Logger.Log("Client finnished loading", LoggingTarget.Network, LogLevel.Verbose);
                        RulesetNetworkingClientHandler.GameLoaded();
                    });
                });
            });
        }

        private void start()
        {
            adjustableClock.Start();
        }

        protected override void OnSuspending(Screen next)
        {
            fadeOut();
            base.OnSuspending(next);
        }

        protected override bool OnExiting(Screen next)
        {
            Remove(RulesetNetworkingClientHandler);
            fadeOut();
            return base.OnExiting(next);
        }

        protected override void UpdateBackgroundElements()
        {
            if (!IsCurrentScreen) return;

            base.UpdateBackgroundElements();

            if (ShowStoryboard && storyboard == null)
                initializeStoryboard(true);

            var beatmap = Beatmap.Value;
            var storyboardVisible = ShowStoryboard && beatmap.Storyboard.HasDrawable;

            storyboardContainer?
                .FadeColour(OsuColour.Gray(BackgroundOpacity), BACKGROUND_FADE_DURATION, Easing.OutQuint)
                .FadeTo(storyboardVisible && BackgroundOpacity > 0 ? 1 : 0, BACKGROUND_FADE_DURATION, Easing.OutQuint);

            if (storyboardVisible && beatmap.Storyboard.ReplacesBackground)
                Background?.FadeTo(0, BACKGROUND_FADE_DURATION, Easing.OutQuint);
        }

        private void fadeOut()
        {
            const float fade_out_duration = 250;

            RulesetContainer?.FadeOut(fade_out_duration);
            Content.FadeOut(fade_out_duration);

            hudOverlay?.ScaleTo(0.7f, fade_out_duration * 3, Easing.In);

            Background?.FadeTo(1f, fade_out_duration);
        }

        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            if (args.Key == Key.Escape)
                BackOut();

            return base.OnKeyDown(state, args);
        }

        public void BackOut()
        {
            RulesetNetworkingClientHandler.AbortGame();
            RulesetNetworkingClientHandler.OnAbort();
        }

        protected override bool OnWheel(InputState state) => mouseWheelDisabled.Value;
    }
}
