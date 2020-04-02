// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Overlays.Dashboard;
using osu.Game.Overlays.Dashboard.Friends;
using osu.Game.Users;

namespace osu.Game.Overlays
{
    public class DashboardOverlay : FullscreenOverlay
    {
        private readonly Bindable<User> localUser = new Bindable<User>();

        [Resolved]
        private IAPIProvider api { get; set; }

        private CancellationTokenSource cancellationToken;
        private APIRequest lastRequest;

        private readonly Box background;
        private readonly DashboardOverlayHeader header;
        private readonly Container content;
        private readonly LoadingLayer loading;
        private readonly BasicScrollContainer scrollFlow;

        public DashboardOverlay()
            : base(OverlayColourScheme.Purple)
        {
            Children = new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both
                },
                scrollFlow = new BasicScrollContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    ScrollbarVisible = false,
                    Child = new FillFlowContainer
                    {
                        AutoSizeAxes = Axes.Y,
                        RelativeSizeAxes = Axes.X,
                        Direction = FillDirection.Vertical,
                        Children = new Drawable[]
                        {
                            header = new DashboardOverlayHeader
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                Depth = -float.MaxValue
                            },
                            content = new Container
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y
                            }
                        }
                    }
                },
                loading = new LoadingLayer(content),
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            background.Colour = ColourProvider.Background5;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            localUser.BindTo(api.LocalUser);
            localUser.BindValueChanged(_ => onTabChanged());
            header.Current.BindValueChanged(_ => onTabChanged());
        }

        protected override void PopIn()
        {
            base.PopIn();

            if (!content.Any())
                header.Current.TriggerChange();
        }

        protected override void PopOutComplete()
        {
            base.PopOutComplete();
            loadDisplay(null);
        }

        private void onTabChanged()
        {
            lastRequest?.Cancel();
            cancellationToken?.Cancel();

            loading.Show();

            // We may want to use OnlineViewContainer after https://github.com/ppy/osu/pull/8044 merge
            if (!api.IsLoggedIn)
            {
                loadDisplay(null);
                return;
            }

            var request = createScopedRequest();
            lastRequest = request;

            if (request == null)
            {
                loadDisplay(null);
                return;
            }

            request.Success += () => Schedule(() => loadDisplay(createDisplayFromResponse(request)));
            request.Failure += _ => Schedule(() => loadDisplay(null));

            api.Queue(request);
        }

        private void loadDisplay(Drawable display)
        {
            scrollFlow.ScrollToStart();

            if (display == null)
            {
                content.Clear();
                loading.Hide();
                return;
            }

            LoadComponentAsync(display, loaded =>
            {
                loading.Hide();
                content.Child = loaded;
            }, (cancellationToken = new CancellationTokenSource()).Token);
        }

        private Drawable createDisplayFromResponse(APIRequest request)
        {
            switch (request)
            {
                case GetFriendsRequest friendsRequest:
                    return new FriendDisplay(friendsRequest.Result);
            }

            return null;
        }

        private APIRequest createScopedRequest()
        {
            switch (header.Current.Value)
            {
                case HomeOverlayTabs.Friends:
                    return new GetFriendsRequest();
            }

            return null;
        }

        protected override void Dispose(bool isDisposing)
        {
            lastRequest?.Cancel();
            cancellationToken?.Cancel();
            base.Dispose(isDisposing);
        }
    }
}