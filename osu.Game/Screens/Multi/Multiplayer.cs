// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Logging;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Input;
using osu.Game.Online.API;
using osu.Game.Online.Multiplayer;
using osu.Game.Screens.Menu;
using osu.Game.Screens.Multi.Components;
using osu.Game.Screens.Multi.Lounge;
using osu.Game.Screens.Multi.Lounge.Components;
using osu.Game.Screens.Multi.Match;
using osu.Game.Screens.Multi.Match.Components;
using osu.Game.Screens.Play;
using osuTK;

namespace osu.Game.Screens.Multi
{
    [Cached]
    public class Multiplayer : OsuScreen, IOnlineComponent
    {
        public override bool CursorVisible => (screenStack.CurrentScreen as IMultiplayerSubScreen)?.CursorVisible ?? true;

        // this is required due to PlayerLoader eventually being pushed to the main stack
        // while leases may be taken out by a subscreen.
        public override bool DisallowExternalBeatmapRulesetChanges => true;

        private readonly MultiplayerWaveContainer waves;

        private readonly OsuButton createButton;
        private readonly LoungeSubScreen loungeSubScreen;
        private readonly ScreenStack screenStack;

        private readonly IBindable<bool> isIdle = new BindableBool();

        [Cached]
        private readonly Bindable<Room> selectedRoom = new Bindable<Room>();

        [Cached]
        private readonly Bindable<FilterCriteria> currentFilter = new Bindable<FilterCriteria>(new FilterCriteria());

        [Cached(Type = typeof(IRoomManager))]
        private RoomManager roomManager;

        [Resolved]
        private OsuGameBase game { get; set; }

        [Resolved]
        private IAPIProvider api { get; set; }

        [Resolved(CanBeNull = true)]
        private OsuLogo logo { get; set; }

        private readonly Drawable header;
        private readonly Drawable headerBackground;

        public Multiplayer()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            RelativeSizeAxes = Axes.Both;
            Padding = new MarginPadding { Horizontal = -HORIZONTAL_OVERFLOW_PADDING };

            var backgroundColour = Color4Extensions.FromHex(@"3e3a44");

            InternalChild = waves = new MultiplayerWaveContainer
            {
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = backgroundColour,
                    },
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Padding = new MarginPadding { Top = Header.HEIGHT },
                        Children = new[]
                        {
                            header = new Container
                            {
                                RelativeSizeAxes = Axes.X,
                                Height = 400,
                                Children = new[]
                                {
                                    headerBackground = new Container
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Width = 1.25f,
                                        Masking = true,
                                        Children = new Drawable[]
                                        {
                                            new HeaderBackgroundSprite
                                            {
                                                RelativeSizeAxes = Axes.X,
                                                Height = 400 // Keep a static height so the header doesn't change as it's resized between subscreens
                                            },
                                        }
                                    },
                                    new Container
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Padding = new MarginPadding { Bottom = -1 }, // 1px padding to avoid a 1px gap due to masking
                                        Child = new Box
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            Colour = ColourInfo.GradientVertical(backgroundColour.Opacity(0.7f), backgroundColour)
                                        },
                                    }
                                }
                            },
                            screenStack = new MultiplayerSubScreenStack { RelativeSizeAxes = Axes.Both }
                        }
                    },
                    new Header(screenStack),
                    createButton = new CreateRoomButton
                    {
                        Anchor = Anchor.TopRight,
                        Origin = Anchor.TopRight,
                        Action = createRoom
                    },
                    roomManager = new RoomManager()
                }
            };

            screenStack.Push(loungeSubScreen = new LoungeSubScreen());

            screenStack.ScreenPushed += screenPushed;
            screenStack.ScreenExited += screenExited;
        }

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
            isIdle.BindValueChanged(idle => updatePollingRate(idle.NewValue), true);
        }

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            var dependencies = new CachedModelDependencyContainer<Room>(base.CreateChildDependencies(parent));
            dependencies.Model.BindTo(selectedRoom);
            return dependencies;
        }

        private void updatePollingRate(bool idle)
        {
            if (!this.IsCurrentScreen())
            {
                roomManager.TimeBetweenListingPolls = 0;
                roomManager.TimeBetweenSelectionPolls = 0;
            }
            else
            {
                switch (screenStack.CurrentScreen)
                {
                    case LoungeSubScreen _:
                        roomManager.TimeBetweenListingPolls = idle ? 120000 : 15000;
                        roomManager.TimeBetweenSelectionPolls = idle ? 120000 : 15000;
                        break;

                    case MatchSubScreen _:
                        roomManager.TimeBetweenListingPolls = 0;
                        roomManager.TimeBetweenSelectionPolls = idle ? 30000 : 5000;
                        break;

                    default:
                        roomManager.TimeBetweenListingPolls = 0;
                        roomManager.TimeBetweenSelectionPolls = 0;
                        break;
                }
            }

            Logger.Log($"Polling adjusted (listing: {roomManager.TimeBetweenListingPolls}, selection: {roomManager.TimeBetweenSelectionPolls})");
        }

        /// <summary>
        /// Push a <see cref="Player"/> to the main screen stack to begin gameplay.
        /// Generally called from a <see cref="MatchSubScreen"/> via DI resolution.
        /// </summary>
        public void Start(Func<Player> player)
        {
            if (!this.IsCurrentScreen())
                return;

            this.Push(new PlayerLoader(player));
        }

        public void APIStateChanged(IAPIProvider api, APIState state)
        {
            if (state != APIState.Online)
                Schedule(forcefullyExit);
        }

        private void forcefullyExit()
        {
            // This is temporary since we don't currently have a way to force screens to be exited
            if (this.IsCurrentScreen())
            {
                while (this.IsCurrentScreen())
                    this.Exit();
            }
            else
            {
                this.MakeCurrent();
                Schedule(forcefullyExit);
            }
        }

        public override void OnEntering(IScreen last)
        {
            this.FadeIn();
            waves.Show();

            beginHandlingTrack();
        }

        public override void OnResuming(IScreen last)
        {
            this.FadeIn(250);
            this.ScaleTo(1, 250, Easing.OutSine);

            base.OnResuming(last);

            beginHandlingTrack();

            updatePollingRate(isIdle.Value);
        }

        public override void OnSuspending(IScreen next)
        {
            this.ScaleTo(1.1f, 250, Easing.InSine);
            this.FadeOut(250);

            endHandlingTrack();

            updatePollingRate(isIdle.Value);
        }

        public override bool OnExiting(IScreen next)
        {
            roomManager.PartRoom();

            if (screenStack.CurrentScreen != null && !(screenStack.CurrentScreen is LoungeSubScreen))
            {
                screenStack.Exit();
                return true;
            }

            waves.Hide();

            this.Delay(WaveContainer.DISAPPEAR_DURATION).FadeOut();

            if (screenStack.CurrentScreen != null)
                loungeSubScreen.MakeCurrent();

            endHandlingTrack();

            base.OnExiting(next);
            return false;
        }

        protected override void LogoExiting(OsuLogo logo)
        {
            base.LogoExiting(logo);

            // the wave overlay transition takes longer than expected to run.
            logo.Delay(WaveContainer.DISAPPEAR_DURATION / 2).FadeOut();
        }

        private void createRoom()
        {
            loungeSubScreen.Open(new Room { Name = { Value = $"{api.LocalUser}'s awesome room" } });
        }

        private void beginHandlingTrack()
        {
            Beatmap.BindValueChanged(updateTrack, true);
        }

        private void endHandlingTrack()
        {
            cancelLooping();
            Beatmap.ValueChanged -= updateTrack;
        }

        private void screenPushed(IScreen lastScreen, IScreen newScreen)
        {
            subScreenChanged(newScreen);
        }

        private void screenExited(IScreen lastScreen, IScreen newScreen)
        {
            subScreenChanged(newScreen);

            if (screenStack.CurrentScreen == null && this.IsCurrentScreen())
                this.Exit();
        }

        private void subScreenChanged(IScreen newScreen)
        {
            switch (newScreen)
            {
                case LoungeSubScreen _:
                    header.Delay(MultiplayerSubScreen.RESUME_TRANSITION_DELAY).ResizeHeightTo(400, MultiplayerSubScreen.APPEAR_DURATION, Easing.OutQuint);
                    headerBackground.MoveToX(0, MultiplayerSubScreen.X_MOVE_DURATION, Easing.OutQuint);
                    break;

                case MatchSubScreen _:
                    header.ResizeHeightTo(135, MultiplayerSubScreen.APPEAR_DURATION, Easing.OutQuint);
                    headerBackground.MoveToX(-MultiplayerSubScreen.X_SHIFT, MultiplayerSubScreen.X_MOVE_DURATION, Easing.OutQuint);
                    break;
            }

            updatePollingRate(isIdle.Value);
            createButton.FadeTo(newScreen is LoungeSubScreen ? 1 : 0, 200);

            updateTrack();
        }

        private void updateTrack(ValueChangedEvent<WorkingBeatmap> _ = null)
        {
            if (screenStack.CurrentScreen is MatchSubScreen)
            {
                var track = Beatmap.Value?.Track;

                if (track != null)
                {
                    track.RestartPoint = Beatmap.Value.Metadata.PreviewTime;
                    track.Looping = true;

                    if (!track.IsRunning)
                        track.Restart();
                }
            }
            else
            {
                cancelLooping();
            }
        }

        private void cancelLooping()
        {
            var track = Beatmap?.Value?.Track;

            if (track != null)
            {
                track.Looping = false;
                track.RestartPoint = 0;
            }
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
                FirstWaveColour = Color4Extensions.FromHex(@"654d8c");
                SecondWaveColour = Color4Extensions.FromHex(@"554075");
                ThirdWaveColour = Color4Extensions.FromHex(@"44325e");
                FourthWaveColour = Color4Extensions.FromHex(@"392850");
            }
        }

        private class HeaderBackgroundSprite : MultiplayerBackgroundSprite
        {
            protected override UpdateableBeatmapBackgroundSprite CreateBackgroundSprite() => new BackgroundSprite { RelativeSizeAxes = Axes.Both };

            private class BackgroundSprite : UpdateableBeatmapBackgroundSprite
            {
                protected override double TransformDuration => 200;
            }
        }

        public class CreateRoomButton : PurpleTriangleButton
        {
            public CreateRoomButton()
            {
                Size = new Vector2(150, Header.HEIGHT - 20);
                Margin = new MarginPadding
                {
                    Top = 10,
                    Right = 10 + HORIZONTAL_OVERFLOW_PADDING,
                };
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                Triangles.TriangleScale = 1.5f;

                Text = "Create room";
            }
        }
    }
}
