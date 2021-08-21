// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
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
using osu.Game.Online.API;
using osu.Game.Online.Rooms;
using osu.Game.Overlays;
using osu.Game.Screens.Menu;
using osu.Game.Screens.OnlinePlay.Components;
using osu.Game.Screens.OnlinePlay.Lounge;
using osu.Game.Users;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.OnlinePlay
{
    [Cached]
    public abstract class OnlinePlayScreen : OsuScreen, IHasSubScreenStack
    {
        [Cached]
        protected readonly OverlayColourProvider ColourProvider = new OverlayColourProvider(OverlayColourScheme.Plum);

        public override bool CursorVisible => (screenStack?.CurrentScreen as IOnlinePlaySubScreen)?.CursorVisible ?? true;

        // this is required due to PlayerLoader eventually being pushed to the main stack
        // while leases may be taken out by a subscreen.
        public override bool DisallowExternalBeatmapRulesetChanges => true;

        private MultiplayerWaveContainer waves;
        private LoungeSubScreen loungeSubScreen;
        private ScreenStack screenStack;

        [Cached(Type = typeof(IRoomManager))]
        protected RoomManager RoomManager { get; private set; }

        [Cached]
        private readonly Bindable<Room> selectedRoom = new Bindable<Room>();

        [Cached]
        private readonly OngoingOperationTracker ongoingOperationTracker = new OngoingOperationTracker();

        [Resolved(CanBeNull = true)]
        private MusicController music { get; set; }

        [Resolved]
        private OsuGameBase game { get; set; }

        [Resolved]
        protected IAPIProvider API { get; private set; }

        [Resolved(CanBeNull = true)]
        private OsuLogo logo { get; set; }

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
                        Children = new Drawable[]
                        {
                            new BeatmapBackgroundSprite
                            {
                                RelativeSizeAxes = Axes.Both
                            },
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = ColourInfo.GradientVertical(Color4.Black.Opacity(0.9f), Color4.Black.Opacity(0.6f))
                            },
                            screenStack = new OnlinePlaySubScreenStack
                            {
                                RelativeSizeAxes = Axes.Both
                            }
                        }
                    },
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

            screenStack.Push(loungeSubScreen = CreateLounge());

            apiState.BindTo(API.State);
            apiState.BindValueChanged(onlineStateChanged, true);
        }

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            var dependencies = new CachedModelDependencyContainer<Room>(base.CreateChildDependencies(parent));
            dependencies.Model.BindTo(selectedRoom);
            return dependencies;
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

            if (loungeSubScreen.IsCurrentScreen())
                loungeSubScreen.OnEntering(last);
            else
                loungeSubScreen.MakeCurrent();
        }

        public override void OnResuming(IScreen last)
        {
            this.FadeIn(250);
            this.ScaleTo(1, 250, Easing.OutSine);

            Debug.Assert(screenStack.CurrentScreen != null);
            screenStack.CurrentScreen.OnResuming(last);

            base.OnResuming(last);
        }

        public override void OnSuspending(IScreen next)
        {
            this.ScaleTo(1.1f, 250, Easing.InSine);
            this.FadeOut(250);

            Debug.Assert(screenStack.CurrentScreen != null);
            screenStack.CurrentScreen.OnSuspending(next);
        }

        public override bool OnExiting(IScreen next)
        {
            var subScreen = screenStack.CurrentScreen as Drawable;
            if (subScreen?.IsLoaded == true && screenStack.CurrentScreen.OnExiting(next))
                return true;

            RoomManager.PartRoom();

            waves.Hide();

            this.Delay(WaveContainer.DISAPPEAR_DURATION).FadeOut();

            base.OnExiting(next);
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

        protected IScreen CurrentSubScreen => screenStack.CurrentScreen;

        protected abstract string ScreenTitle { get; }

        protected virtual RoomManager CreateRoomManager() => new RoomManager();

        protected abstract LoungeSubScreen CreateLounge();

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

        private class BeatmapBackgroundSprite : OnlinePlayBackgroundSprite
        {
            protected override UpdateableBeatmapBackgroundSprite CreateBackgroundSprite() => new BlurredBackgroundSprite(BeatmapSetCoverType) { RelativeSizeAxes = Axes.Both };

            public class BlurredBackgroundSprite : UpdateableBeatmapBackgroundSprite
            {
                public BlurredBackgroundSprite(BeatmapSetCoverType type)
                    : base(type)
                {
                }

                protected override double LoadDelay => 200;

                protected override Drawable CreateDrawable(BeatmapInfo model) =>
                    new BufferedLoader(base.CreateDrawable(model));
            }

            // This class is an unfortunate requirement due to `LongRunningLoad` requiring direct async loading.
            // It means that if the web request fetching the beatmap background takes too long, it will suddenly appear.
            internal class BufferedLoader : BufferedContainer
            {
                private readonly Drawable drawable;

                public BufferedLoader(Drawable drawable)
                {
                    this.drawable = drawable;

                    RelativeSizeAxes = Axes.Both;
                    BlurSigma = new Vector2(10);
                    FrameBufferScale = new Vector2(0.5f);
                    CacheDrawnFrameBuffer = true;
                }

                [BackgroundDependencyLoader]
                private void load()
                {
                    LoadComponentAsync(drawable, d =>
                    {
                        Add(d);
                        ForceRedraw();
                    });
                }
            }
        }

        ScreenStack IHasSubScreenStack.SubScreenStack => screenStack;
    }
}
