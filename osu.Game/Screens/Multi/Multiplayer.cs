// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Logging;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Input;
using osu.Game.Online.API;
using osu.Game.Online.Multiplayer;
using osu.Game.Overlays;
using osu.Game.Overlays.BeatmapSet.Buttons;
using osu.Game.Screens.Menu;
using osu.Game.Screens.Multi.Lounge;
using osu.Game.Screens.Multi.Match;
using osuTK;

namespace osu.Game.Screens.Multi
{
    [Cached]
    public class Multiplayer : CompositeDrawable, IOsuScreen, IOnlineComponent
    {
        public bool AllowBeatmapRulesetChange => (screenStack.CurrentScreen as IMultiplayerSubScreen)?.AllowBeatmapRulesetChange ?? true;
        public bool AllowExternalScreenChange => (screenStack.CurrentScreen as IMultiplayerSubScreen)?.AllowExternalScreenChange ?? true;
        public bool CursorVisible => (screenStack.CurrentScreen as IMultiplayerSubScreen)?.AllowExternalScreenChange ?? true;

        public bool HideOverlaysOnEnter => false;
        public OverlayActivation InitialOverlayActivationMode => OverlayActivation.All;

        public float BackgroundParallaxAmount => 1;

        public bool ValidForResume { get; set; } = true;
        public bool ValidForPush { get; set; } = true;

        public override bool RemoveWhenNotAlive => false;

        private readonly MultiplayerWaveContainer waves;

        private readonly OsuButton createButton;
        private readonly LoungeSubScreen loungeSubScreen;
        private readonly ScreenStack screenStack;

        [Cached(Type = typeof(IRoomManager))]
        private RoomManager roomManager;

        [Resolved]
        private IBindableBeatmap beatmap { get; set; }

        [Resolved]
        private OsuGameBase game { get; set; }

        [Resolved]
        private APIAccess api { get; set; }

        [Resolved(CanBeNull = true)]
        private OsuLogo logo { get; set; }

        public Multiplayer()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            RelativeSizeAxes = Axes.Both;

            InternalChild = waves = new MultiplayerWaveContainer
            {
                RelativeSizeAxes = Axes.Both,
            };

            screenStack = new ScreenStack(loungeSubScreen = new LoungeSubScreen(this.Push)) { RelativeSizeAxes = Axes.Both };
            Padding = new MarginPadding { Horizontal = -OsuScreen.HORIZONTAL_OVERFLOW_PADDING };

            waves.AddRange(new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = OsuColour.FromHex(@"3e3a44"),
                        },
                        new Triangles
                        {
                            RelativeSizeAxes = Axes.Both,
                            ColourLight = OsuColour.FromHex(@"3c3842"),
                            ColourDark = OsuColour.FromHex(@"393540"),
                            TriangleScale = 5,
                        },
                    },
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Top = Header.HEIGHT },
                    Child = screenStack
                },
                new Header(screenStack),
                createButton = new HeaderButton
                {
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    RelativeSizeAxes = Axes.None,
                    Size = new Vector2(150, Header.HEIGHT - 20),
                    Margin = new MarginPadding
                    {
                        Top = 10,
                        Right = 10 + OsuScreen.HORIZONTAL_OVERFLOW_PADDING,
                    },
                    Text = "Create room",
                    Action = () => loungeSubScreen.Push(new Room
                    {
                        Name = { Value = $"{api.LocalUser}'s awesome room" }
                    }),
                },
                roomManager = new RoomManager()
            });

            screenStack.ScreenPushed += screenPushed;
            screenStack.ScreenExited += screenExited;
        }

        private readonly IBindable<bool> isIdle = new BindableBool();

        [BackgroundDependencyLoader(true)]
        private void load(IdleTracker idleTracker)
        {
            api.Register(this);

            if (idleTracker != null)
                isIdle.BindTo(idleTracker.IsIdle);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            isIdle.BindValueChanged(updatePollingRate, true);
        }

        private void updatePollingRate(bool idle)
        {
            roomManager.TimeBetweenPolls = !this.IsCurrentScreen() || !(screenStack.CurrentScreen is LoungeSubScreen) ? 0 : (idle ? 120000 : 15000);
            Logger.Log($"Polling adjusted to {roomManager.TimeBetweenPolls}");
        }

        public void APIStateChanged(APIAccess api, APIState state)
        {
            if (state != APIState.Online)
                forcefullyExit();
        }

        private void forcefullyExit()
        {
            // This is temporary since we don't currently have a way to force screens to be exited
            if (this.IsCurrentScreen())
                this.Exit();
            else
            {
                this.MakeCurrent();
                Schedule(forcefullyExit);
            }
        }

        public void OnEntering(IScreen last)
        {
            this.FadeIn();

            waves.Show();
        }

        public bool OnExiting(IScreen next)
        {
            waves.Hide();

            this.Delay(WaveContainer.DISAPPEAR_DURATION).FadeOut();

            cancelLooping();

            if (screenStack.CurrentScreen != null)
                loungeSubScreen.MakeCurrent();

            updatePollingRate(isIdle.Value);

            // the wave overlay transition takes longer than expected to run.
            logo?.AppendAnimatingAction(() => logo.Delay(WaveContainer.DISAPPEAR_DURATION / 2).FadeOut(), false);

            return false;
        }

        public void OnResuming(IScreen last)
        {
            this.FadeIn(250);
            this.ScaleTo(1, 250, Easing.OutSine);

            logo?.AppendAnimatingAction(() => OsuScreen.ApplyLogoArrivingDefaults(logo), true);

            updatePollingRate(isIdle.Value);
        }

        public void OnSuspending(IScreen next)
        {
            this.ScaleTo(1.1f, 250, Easing.InSine);
            this.FadeOut(250);

            cancelLooping();
            roomManager.TimeBetweenPolls = 0;
        }

        private void cancelLooping()
        {
            var track = beatmap.Value.Track;
            if (track != null)
                track.Looping = false;
        }

        protected override void Update()
        {
            base.Update();

            if (!this.IsCurrentScreen()) return;

            if (screenStack.CurrentScreen is MatchSubScreen)
            {
                var track = beatmap.Value.Track;
                if (track != null)
                {
                    track.Looping = true;

                    if (!track.IsRunning)
                    {
                        game.Audio.AddItemToList(track);
                        track.Seek(beatmap.Value.Metadata.PreviewTime);
                        track.Start();
                    }
                }

                createButton.Hide();
            }
            else if (screenStack.CurrentScreen is LoungeSubScreen)
                createButton.Show();
        }

        private void screenPushed(IScreen lastScreen, IScreen newScreen)
            => updatePollingRate(isIdle.Value);

        private void screenExited(IScreen lastScreen, IScreen newScreen)
        {
            if (lastScreen is MatchSubScreen)
                cancelLooping();

            updatePollingRate(isIdle.Value);

            if (screenStack.CurrentScreen == null)
                this.Exit();
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            api?.Unregister(this);
        }

        private class MultiplayerWaveContainer : WaveContainer
        {
            protected override bool StartHidden => true;

            public MultiplayerWaveContainer()
            {
                FirstWaveColour = OsuColour.FromHex(@"654d8c");
                SecondWaveColour = OsuColour.FromHex(@"554075");
                ThirdWaveColour = OsuColour.FromHex(@"44325e");
                FourthWaveColour = OsuColour.FromHex(@"392850");
            }
        }
    }
}
