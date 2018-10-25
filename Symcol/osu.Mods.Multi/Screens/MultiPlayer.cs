using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using osu.Core;
using osu.Core.Config;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Input.EventArgs;
using osu.Framework.Input.States;
using osu.Framework.Logging;
using osu.Framework.Screens;
using osu.Framework.Threading;
using osu.Framework.Timing;
using osu.Game;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Cursor;
using osu.Game.Online.API;
using osu.Game.Overlays;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Play;
using osu.Game.Skinning;
using osu.Game.Storyboards.Drawables;
using osu.Mods.Multi.Networking;
using osu.Mods.Multi.Networking.Packets.Player;
using osu.Mods.Multi.Rulesets;
using osu.Mods.Multi.Screens.Pieces;
using OpenTK;
using OpenTK.Input;
using Symcol.Base.Graphics.Containers;
using Symcol.Networking.Packets;

namespace osu.Mods.Multi.Screens
{
    public class MultiPlayer : ScreenWithBeatmapBackground, IProvideCursor
    {
        protected override float BackgroundParallaxAmount => 0.1f;

        protected override bool HideOverlaysOnEnter => true;

        protected override OverlayActivation InitialOverlayActivationMode => OverlayActivation.UserTriggered;

        public bool AllowLeadIn { get; set; } = true;

        private Bindable<bool> mouseWheelDisabled;
        private Bindable<double> userAudioOffset;

        public CursorContainer Cursor => RulesetContainer.Cursor;
        public bool ProvidingUserCursor => RulesetContainer?.Cursor != null && !RulesetContainer.HasReplayLoaded.Value;

        private IAdjustableClock sourceClock;

        /// <summary>
        /// The decoupled clock used for gameplay. Should be used for seeks and clock control.
        /// </summary>
        private DecoupleableInterpolatingFramedClock adjustableClock;

        private RulesetInfo ruleset;

        private APIAccess api;

        private OsuGame osu;

        protected ScoreProcessor ScoreProcessor;
        protected RulesetContainer RulesetContainer;

        private HUDOverlay hudOverlay;

        private DrawableStoryboard storyboard;
        private Container storyboardContainer;

        public bool LoadedBeatmapSuccessfully => RulesetContainer?.Objects.Any() == true;

        public readonly OsuNetworkingHandler OsuNetworkingHandler;

        protected readonly List<OsuUserInfo> Users;

        protected DeadContainer<MultiCursorContainer> CursorContainer;

        public MultiPlayer(OsuNetworkingHandler osuNetworkingHandler, List<OsuUserInfo> users)
        {
            Name = "MultiPlayer";
            OsuNetworkingHandler = osuNetworkingHandler;
            Users = users;
            OsuNetworkingHandler.OnPacketReceive += handlePackets;
        }

        private void handlePackets(PacketInfo info)
        {
            switch (info.Packet)
            {
                case MatchStartingPacket start:
                    adjustableClock.Start();
                    break;
                case MatchExitPacket exit:
                    Exit();
                    break;
                case CursorPositionPacket position:
                    foreach (MultiCursorContainer c in CursorContainer)
                        if (position.ID.ToString() == c.Name)
                            c.ActiveCursor.MoveTo(new Vector2(position.X + osu.DrawSize.X / 2, position.Y + osu.DrawSize.Y / 2), 1000f / 30f);
                    break;
            }
        }

        [BackgroundDependencyLoader]
        private void load(OsuGame osu, AudioManager audio, APIAccess api, OsuConfigManager config)
        {
            this.api = api;
            this.osu = osu;

            WorkingBeatmap working = Beatmap.Value;
            if (working is DummyWorkingBeatmap)
                return;

            mouseWheelDisabled = config.GetBindable<bool>(OsuSetting.MouseDisableWheel);
            userAudioOffset = config.GetBindable<double>(OsuSetting.AudioOffset);

            IBeatmap beatmap;

            try
            {
                beatmap = working.Beatmap;

                if (beatmap == null)
                    throw new InvalidOperationException("Beatmap was not loaded");

                ruleset = Ruleset.Value ?? beatmap.BeatmapInfo.Ruleset;
                Ruleset rulesetInstance = ruleset.CreateInstance();

                try
                {
                    RulesetContainer = rulesetInstance.CreateRulesetContainerWith(working);
                }
                catch (BeatmapInvalidForRulesetException)
                {
                    // we may fail to create a RulesetContainer if the beatmap cannot be loaded with the user's preferred ruleset
                    // let's try again forcing the beatmap's ruleset.
                    ruleset = beatmap.BeatmapInfo.Ruleset;
                    rulesetInstance = ruleset.CreateInstance();

                    RulesetContainer = rulesetInstance.CreateRulesetContainerWith(Beatmap.Value);
                }

                if (!RulesetContainer.Objects.Any())
                {
                    Logger.Error(new InvalidOperationException("Beatmap contains no hit objects!"), "Beatmap contains no hit objects!");
                    return;
                }
            }
            catch (Exception e)
            {
                Logger.Error(e, "Could not load beatmap sucessfully!");
                //couldn't load, hard abort!
                return;
            }

            sourceClock = (IAdjustableClock)working.Track ?? new StopwatchClock();
            adjustableClock = new DecoupleableInterpolatingFramedClock { IsCoupled = false };

            adjustableClock.Seek(AllowLeadIn
                ? Math.Min(0, RulesetContainer.GameplayStartTime - beatmap.BeatmapInfo.AudioLeadIn)
                : RulesetContainer.GameplayStartTime);

            adjustableClock.ProcessFrame();

            // Lazer's audio timings in general doesn't match stable. This is the result of user testing, albeit limited.
            // This only seems to be required on windows. We need to eventually figure out why, with a bit of luck.
            var platformOffsetClock = new FramedOffsetClock(adjustableClock) { Offset = RuntimeInfo.OS == RuntimeInfo.Platform.Windows ? 22 : 0 };

            // the final usable gameplay clock with user-set offsets applied.
            var offsetClock = new FramedOffsetClock(platformOffsetClock);

            userAudioOffset.ValueChanged += v => offsetClock.Offset = v;
            userAudioOffset.TriggerChange();

            ScoreProcessor = RulesetContainer.CreateScoreProcessor();

            try
            {
                RulesetContainer.Cursor.ActiveCursor.Colour = OsuColour.FromHex(SymcolOsuModSet.SymcolConfigManager.GetBindable<string>(SymcolSetting.PlayerColor));
            }
            catch { }

            Children = new[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Clock = offsetClock,
                    Children = new[]
                    {
                        storyboardContainer = new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Alpha = 0,
                        },
                        new LocalSkinOverrideContainer(working.Skin)
                        {
                            RelativeSizeAxes = Axes.Both,
                            Child = RulesetContainer
                        },
                        new Scoreboard(OsuNetworkingHandler, Users, ScoreProcessor),
                        new BreakOverlay(beatmap.BeatmapInfo.LetterboxInBreaks, ScoreProcessor)
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            ProcessCustomClock = false,
                            Breaks = beatmap.Breaks
                        },
                        RulesetContainer.Cursor?.CreateProxy() ?? new Container(),
                        CursorContainer = new DeadContainer<MultiCursorContainer>
                        {
                            RelativeSizeAxes = Axes.Both
                        },
                        hudOverlay = new HUDOverlay(ScoreProcessor, RulesetContainer, working, offsetClock, adjustableClock)
                        {
                            Clock = Clock, // hud overlay doesn't want to use the audio clock directly
                            ProcessCustomClock = false,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre
                        },
                        //TODO: voting on this
                        /*
                        new SkipOverlay(RulesetContainer.GameplayStartTime)
                        {
                            Clock = Clock, // skip button doesn't want to use the audio clock directly
                            ProcessCustomClock = false,
                            AdjustableClock = adjustableClock,
                            FramedClock = offsetClock,
                        },
                        */
                    }
                }
            };

            foreach (OsuUserInfo user in Users)
                if (user.ID != OsuNetworkingHandler.OsuUserInfo.ID)
                {
                    try
                    {
                        MultiCursorContainer c = new MultiCursorContainer();

                        if (RulesetContainer.Cursor != null && RulesetContainer.Cursor is MultiCursorContainer m && m.CreateMultiCursor() != null)
                            c = m.CreateMultiCursor();

                        c.Colour = OsuColour.FromHex(user.Colour);
                        c.Name = user.ID.ToString();
                        c.Slave = true;
                        c.Alpha = 0.5f;
                        CursorContainer.Add(c);
                    }
                    catch { }
                }

            if (!ScoreProcessor.Mode.Disabled)
                config.BindWith(OsuSetting.ScoreDisplayMode, ScoreProcessor.Mode);

            hudOverlay.HoldToQuit.Action = Exit;
            hudOverlay.KeyCounter.Visible.BindTo(RulesetContainer.HasReplayLoaded);

            if (ShowStoryboard)
                initializeStoryboard(false);

            // Bind ScoreProcessor to ourselves
            ScoreProcessor.AllJudged += onCompletion;
            ScoreProcessor.Failed += onFail;

            foreach (var mod in Beatmap.Value.Mods.Value.OfType<IApplicableToScoreProcessor>())
                mod.ApplyToScoreProcessor(ScoreProcessor);
        }

        private void applyRateFromMods()
        {
            if (sourceClock == null) return;

            sourceClock.Rate = 1;
            foreach (var mod in Beatmap.Value.Mods.Value.OfType<IApplicableToClock>())
                mod.ApplyToClock(sourceClock);
        }

        private ScheduledDelegate onCompletionEvent;

        private void onCompletion()
        {
            // Only show the completion screen if the player hasn't failed
            if (ScoreProcessor.HasFailed || onCompletionEvent != null)
                return;

            ValidForResume = false;

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
                    ScoreProcessor.PopulateScore(score);
                    score.User = RulesetContainer.Replay?.User ?? api.LocalUser.Value;
                    //Push(new Results(score));
                    Exit();
                });
            }
        }

        private bool onFail()
        {
            if (Beatmap.Value.Mods.Value.OfType<IApplicableFailOverride>().Any(m => !m.AllowFail))
                return false;
            return true;
        }

        protected override void OnEntering(Screen last)
        {
            base.OnEntering(last);
            Add(OsuNetworkingHandler);

            if (!LoadedBeatmapSuccessfully)
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
                        Logger.Log("Client finnished loading", LoggingTarget.Network);
                        OsuNetworkingHandler.SendToServer(new PlayerLoadedPacket());
                    });
                });
            });
        }

        protected override void OnSuspending(Screen next)
        {
            fadeOut();
            Remove(OsuNetworkingHandler);
            base.OnSuspending(next);
        }

        protected override void OnResuming(Screen last)
        {
            base.OnResuming(last);
            Add(OsuNetworkingHandler);
        }

        protected override bool OnExiting(Screen next)
        {
            OsuNetworkingHandler.OnPacketReceive -= handlePackets;
            Remove(OsuNetworkingHandler);
            applyRateFromMods();

            fadeOut();
            return base.OnExiting(next);
        }

        private void fadeOut()
        {
            const float fade_out_duration = 250;

            RulesetContainer?.FadeOut(fade_out_duration);
            Content.FadeOut(fade_out_duration);

            hudOverlay?.ScaleTo(0.7f, fade_out_duration * 3, Easing.In);

            Background?.FadeTo(1f, fade_out_duration);
        }

        protected override bool OnScroll(InputState state) => mouseWheelDisabled.Value;

        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            if (args.Key == Key.Escape && !args.Repeat)
            {
                OsuNetworkingHandler.SendToServer(new MatchExitPacket());
                Exit();
                return true;
            }

            return base.OnKeyDown(state, args);
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

        private double boo = double.MinValue;
        protected override void Update()
        {
            base.Update();

            if (boo <= Time.Current)
            {
                //30 packets per second test
                boo = Time.Current + 1000f / 30f;

                if (RulesetContainer.Cursor != null)
                    OsuNetworkingHandler.SendToServer(new CursorPositionPacket
                    {
                        ID = OsuNetworkingHandler.OsuUserInfo.ID,
                        X = RulesetContainer.Cursor.ActiveCursor.Position.X - osu.DrawSize.X / 2,
                        Y = RulesetContainer.Cursor.ActiveCursor.Position.Y - osu.DrawSize.Y / 2,
                    });
            }
        }
    }
}
