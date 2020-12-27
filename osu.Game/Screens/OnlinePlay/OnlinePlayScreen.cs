// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Screens;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Input;
using osu.Game.Online.API;
using osu.Game.Online.Rooms;
using osu.Game.Overlays;
using osu.Game.Screens.Menu;
using osu.Game.Screens.OnlinePlay.Components;
using osu.Game.Screens.OnlinePlay.Lounge;
using osu.Game.Screens.OnlinePlay.Lounge.Components;
using osu.Game.Screens.OnlinePlay.Match;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Screens.OnlinePlay
{
    [Cached]
    public abstract class OnlinePlayScreen : OsuScreen
    {
        public override bool CursorVisible => (screenStack.CurrentScreen as IOnlinePlaySubScreen)?.CursorVisible ?? true;

        // this is required due to PlayerLoader eventually being pushed to the main stack
        // while leases may be taken out by a subscreen.
        public override bool DisallowExternalBeatmapRulesetChanges => true;

        private readonly MultiplayerWaveContainer waves;

        private readonly OsuButton createButton;
        private readonly LoungeSubScreen loungeSubScreen;
        private readonly ScreenStack screenStack;

        private readonly IBindable<bool> isIdle = new BindableBool();

        [Cached(Type = typeof(IRoomManager))]
        protected RoomManager RoomManager { get; private set; }

        [Cached]
        private readonly Bindable<Room> selectedRoom = new Bindable<Room>();

        [Cached]
        private readonly Bindable<FilterCriteria> currentFilter = new Bindable<FilterCriteria>(new FilterCriteria());

        [Resolved(CanBeNull = true)]
        private MusicController music { get; set; }

        [Resolved]
        private OsuGameBase game { get; set; }

        [Resolved]
        protected IAPIProvider API { get; private set; }

        [Resolved(CanBeNull = true)]
        private OsuLogo logo { get; set; }

        private readonly Drawable header;
        private readonly Drawable headerBackground;

        protected OnlinePlayScreen()
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
                                            Colour = ColourInfo.GradientVertical(backgroundColour.Opacity(0.5f), backgroundColour)
                                        },
                                    }
                                }
                            },
                            screenStack = new OnlinePlaySubScreenStack { RelativeSizeAxes = Axes.Both }
                        }
                    },
                    new Header(ScreenTitle, screenStack),
                    createButton = CreateNewMultiplayerGameButton().With(button =>
                    {
                        button.Anchor = Anchor.TopRight;
                        button.Origin = Anchor.TopRight;
                        button.Size = new Vector2(150, Header.HEIGHT - 20);
                        button.Margin = new MarginPadding
                        {
                            Top = 10,
                            Right = 10 + HORIZONTAL_OVERFLOW_PADDING,
                        };
                        button.Action = () => OpenNewRoom();
                    }),
                    RoomManager = CreateRoomManager()
                }
            };

            screenStack.ScreenPushed += screenPushed;
            screenStack.ScreenExited += screenExited;

            screenStack.Push(loungeSubScreen = CreateLounge());
        }

        private readonly IBindable<APIState> apiState = new Bindable<APIState>();

        [BackgroundDependencyLoader(true)]
        private void load(IdleTracker idleTracker)
        {
            apiState.BindTo(API.State);
            apiState.BindValueChanged(onlineStateChanged, true);

            if (idleTracker != null)
                isIdle.BindTo(idleTracker.IsIdle);
        }

        private void onlineStateChanged(ValueChangedEvent<APIState> state) => Schedule(() =>
        {
            if (state.NewValue != APIState.Online)
                Schedule(forcefullyExit);
        });

        protected override void LoadComplete()
        {
            base.LoadComplete();
            isIdle.BindValueChanged(idle => UpdatePollingRate(idle.NewValue), true);
        }

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            var dependencies = new CachedModelDependencyContainer<Room>(base.CreateChildDependencies(parent));
            dependencies.Model.BindTo(selectedRoom);
            return dependencies;
        }

        protected abstract void UpdatePollingRate(bool isIdle);

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

            if (loungeSubScreen.IsCurrentScreen())
                loungeSubScreen.OnEntering(last);
            else
                loungeSubScreen.MakeCurrent();
        }

        public override void OnResuming(IScreen last)
        {
            this.FadeIn(250);
            this.ScaleTo(1, 250, Easing.OutSine);

            screenStack.CurrentScreen?.OnResuming(last);
            base.OnResuming(last);

            UpdatePollingRate(isIdle.Value);
        }

        public override void OnSuspending(IScreen next)
        {
            this.ScaleTo(1.1f, 250, Easing.InSine);
            this.FadeOut(250);

            screenStack.CurrentScreen?.OnSuspending(next);

            UpdatePollingRate(isIdle.Value);
        }

        public override bool OnExiting(IScreen next)
        {
            RoomManager.PartRoom();

            waves.Hide();

            this.Delay(WaveContainer.DISAPPEAR_DURATION).FadeOut();

            screenStack.CurrentScreen?.OnExiting(next);
            base.OnExiting(next);
            return false;
        }

        public override bool OnBackButton()
        {
            if ((screenStack.CurrentScreen as IOnlinePlaySubScreen)?.OnBackButton() == true)
                return true;

            if (screenStack.CurrentScreen != null && !(screenStack.CurrentScreen is LoungeSubScreen))
            {
                screenStack.Exit();
                return true;
            }

            return false;
        }

        protected override void LogoExiting(OsuLogo logo)
        {
            base.LogoExiting(logo);

            // the wave overlay transition takes longer than expected to run.
            logo.Delay(WaveContainer.DISAPPEAR_DURATION / 2).FadeOut();
        }

        /// <summary>
        /// Creates and opens the newly-created room.
        /// </summary>
        /// <param name="room">An optional template to use when creating the room.</param>
        public void OpenNewRoom(Room room = null) => loungeSubScreen.Open(room ?? CreateNewRoom());

        /// <summary>
        /// Creates a new room.
        /// </summary>
        /// <returns>The created <see cref="Room"/>.</returns>
        protected abstract Room CreateNewRoom();

        private void screenPushed(IScreen lastScreen, IScreen newScreen)
        {
            subScreenChanged(lastScreen, newScreen);
        }

        private void screenExited(IScreen lastScreen, IScreen newScreen)
        {
            subScreenChanged(lastScreen, newScreen);

            if (screenStack.CurrentScreen == null && this.IsCurrentScreen())
                this.Exit();
        }

        private void subScreenChanged(IScreen lastScreen, IScreen newScreen)
        {
            switch (newScreen)
            {
                case LoungeSubScreen _:
                    header.Delay(OnlinePlaySubScreen.RESUME_TRANSITION_DELAY).ResizeHeightTo(400, OnlinePlaySubScreen.APPEAR_DURATION, Easing.OutQuint);
                    headerBackground.MoveToX(0, OnlinePlaySubScreen.X_MOVE_DURATION, Easing.OutQuint);
                    break;

                case RoomSubScreen _:
                    header.ResizeHeightTo(135, OnlinePlaySubScreen.APPEAR_DURATION, Easing.OutQuint);
                    headerBackground.MoveToX(-OnlinePlaySubScreen.X_SHIFT, OnlinePlaySubScreen.X_MOVE_DURATION, Easing.OutQuint);
                    break;
            }

            if (lastScreen is IOsuScreen lastOsuScreen)
                Activity.UnbindFrom(lastOsuScreen.Activity);

            if (newScreen is IOsuScreen newOsuScreen)
                ((IBindable<UserActivity>)Activity).BindTo(newOsuScreen.Activity);

            UpdatePollingRate(isIdle.Value);
            createButton.FadeTo(newScreen is LoungeSubScreen ? 1 : 0, 200);
        }

        protected IScreen CurrentSubScreen => screenStack.CurrentScreen;

        protected abstract string ScreenTitle { get; }

        protected abstract RoomManager CreateRoomManager();

        protected abstract LoungeSubScreen CreateLounge();

        protected abstract OsuButton CreateNewMultiplayerGameButton();

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

        private class HeaderBackgroundSprite : OnlinePlayBackgroundSprite
        {
            protected override UpdateableBeatmapBackgroundSprite CreateBackgroundSprite() => new BackgroundSprite { RelativeSizeAxes = Axes.Both };

            private class BackgroundSprite : UpdateableBeatmapBackgroundSprite
            {
                protected override double TransformDuration => 200;
            }
        }
    }
}
