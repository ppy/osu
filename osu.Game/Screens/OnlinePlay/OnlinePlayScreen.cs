// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Logging;
using osu.Framework.Screens;
using osu.Game.Graphics.Containers;
using osu.Game.Online.API;
using osu.Game.Overlays;
using osu.Game.Screens.Menu;
using osu.Game.Screens.OnlinePlay.Components;
using osu.Game.Screens.OnlinePlay.Lounge;
using osu.Game.Users;

namespace osu.Game.Screens.OnlinePlay
{
    [Cached]
    public abstract partial class OnlinePlayScreen : OsuScreen, IHasSubScreenStack
    {
        [Cached]
        protected readonly OverlayColourProvider ColourProvider = new OverlayColourProvider(OverlayColourScheme.Plum);

        public IScreen CurrentSubScreen => screenStack.CurrentScreen;

        public override bool CursorVisible => (screenStack?.CurrentScreen as IOnlinePlaySubScreen)?.CursorVisible ?? true;

        // this is required due to PlayerLoader eventually being pushed to the main stack
        // while leases may be taken out by a subscreen.
        public override bool DisallowExternalBeatmapRulesetChanges => true;

        protected LoungeSubScreen Lounge { get; private set; }

        private MultiplayerWaveContainer waves;
        private ScreenStack screenStack;

        [Cached(Type = typeof(IRoomManager))]
        protected RoomManager RoomManager { get; private set; }

        [Cached]
        private readonly OngoingOperationTracker ongoingOperationTracker = new OngoingOperationTracker();

        [Resolved]
        protected IAPIProvider API { get; private set; }

        protected OnlinePlayScreen()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            RelativeSizeAxes = Axes.Both;
            Padding = new MarginPadding { Horizontal = -HORIZONTAL_OVERFLOW_PADDING };

            RoomManager = CreateRoomManager();
        }

        private readonly IBindable<APIState> apiState = new Bindable<APIState>();

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = waves = new MultiplayerWaveContainer
            {
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    screenStack = new OnlinePlaySubScreenStack { RelativeSizeAxes = Axes.Both },
                    new Header(ScreenTitle, screenStack),
                    RoomManager,
                    ongoingOperationTracker
                }
            };
        }

        private void onlineStateChanged(ValueChangedEvent<APIState> state) => Schedule(() =>
        {
            if (state.NewValue != APIState.Online)
            {
                Logger.Log("API connection was lost, can't continue with online play", LoggingTarget.Network, LogLevel.Important);
                Schedule(forcefullyExit);
            }
        });

        protected override void LoadComplete()
        {
            base.LoadComplete();

            screenStack.ScreenPushed += screenPushed;
            screenStack.ScreenExited += screenExited;

            screenStack.Push(Lounge = CreateLounge());

            apiState.BindTo(API.State);
            apiState.BindValueChanged(onlineStateChanged, true);
        }

        private void forcefullyExit()
        {
            Logger.Log($"{this} forcefully exiting due to loss of API connection");

            // This is temporary since we don't currently have a way to force screens to be exited
            if (this.IsCurrentScreen())
            {
                while (this.IsCurrentScreen())
                    this.Exit();
            }
            // Also handle the case where a child screen is current (ie. gameplay).
            else if (this.GetChildScreen() != null)
            {
                this.MakeCurrent();
                Schedule(forcefullyExit);
            }
        }

        public override void OnEntering(ScreenTransitionEvent e)
        {
            this.FadeIn();
            waves.Show();

            Mods.SetDefault();

            if (Lounge.IsCurrentScreen())
                Lounge.OnEntering(e);
            else
                Lounge.MakeCurrent();
        }

        public override void OnResuming(ScreenTransitionEvent e)
        {
            this.FadeIn(250);
            this.ScaleTo(1, 250, Easing.OutSine);

            Debug.Assert(screenStack.CurrentScreen != null);

            // if a subscreen was pushed to the nested stack while the stack was not present, this path will proxy `OnResuming()`
            // to the subscreen before `OnEntering()` can even be called for the subscreen, breaking ordering expectations.
            // to work around this, do not proxy resume to screens that haven't loaded yet.
            if (screenStack.CurrentScreen is Drawable { IsLoaded: true })
                screenStack.CurrentScreen.OnResuming(e);

            base.OnResuming(e);
        }

        public override void OnSuspending(ScreenTransitionEvent e)
        {
            this.ScaleTo(1.1f, 250, Easing.InSine);
            this.FadeOut(250);

            Debug.Assert(screenStack.CurrentScreen != null);

            // if a subscreen was pushed to the nested stack while the stack was not present, this path will proxy `OnSuspending()`
            // to the subscreen before `OnEntering()` can even be called for the subscreen, breaking ordering expectations.
            // to work around this, do not proxy suspend to screens that haven't loaded yet.
            if (screenStack.CurrentScreen is Drawable { IsLoaded: true })
                screenStack.CurrentScreen.OnSuspending(e);
        }

        public override bool OnExiting(ScreenExitEvent e)
        {
            while (screenStack.CurrentScreen != null && screenStack.CurrentScreen is not LoungeSubScreen)
            {
                var subScreen = (Screen)screenStack.CurrentScreen;
                if (subScreen.IsLoaded && subScreen.OnExiting(e))
                    return true;

                subScreen.Exit();
            }

            RoomManager.PartRoom();

            waves.Hide();

            this.Delay(WaveContainer.DISAPPEAR_DURATION).FadeOut();

            base.OnExiting(e);
            return false;
        }

        public override bool OnBackButton()
        {
            if (!(screenStack.CurrentScreen is IOnlinePlaySubScreen onlineSubScreen))
                return false;

            if (((Drawable)onlineSubScreen).IsLoaded && onlineSubScreen.AllowBackButton && onlineSubScreen.OnBackButton())
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
            if (lastScreen is IOsuScreen lastOsuScreen)
                Activity.UnbindFrom(lastOsuScreen.Activity);

            if (newScreen is IOsuScreen newOsuScreen)
                ((IBindable<UserActivity>)Activity).BindTo(newOsuScreen.Activity);
        }

        protected abstract string ScreenTitle { get; }

        protected virtual RoomManager CreateRoomManager() => new RoomManager();

        protected abstract LoungeSubScreen CreateLounge();

        private partial class MultiplayerWaveContainer : WaveContainer
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

        ScreenStack IHasSubScreenStack.SubScreenStack => screenStack;
    }
}
