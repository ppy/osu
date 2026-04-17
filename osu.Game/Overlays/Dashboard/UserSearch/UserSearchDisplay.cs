// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Threading;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Overlays.Dashboard.Friends;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Overlays.Dashboard.UserSearch
{
    public partial class UserSearchDisplay : CompositeDrawable
    {
        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        public IBindable<bool> Loading => loading;
        private readonly BindableBool loading = new BindableBool();

        private Box background = null!;
        private UserListToolbar userListToolbar = null!;
        private Container listContainer = null!;
        private BasicSearchTextBox searchTextBox = null!;

        public UserSearchDisplay()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            InternalChild = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Children = new Drawable[]
                {
                    new Container
                    {
                        Name = "User List",
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Children = new Drawable[]
                        {
                            background = new Box
                            {
                                RelativeSizeAxes = Axes.Both
                            },
                            new FillFlowContainer
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Direction = FillDirection.Vertical,
                                Margin = new MarginPadding { Bottom = 20 },
                                Children = new Drawable[]
                                {
                                    new GridContainer
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Padding = new MarginPadding
                                        {
                                            Horizontal = 40,
                                            Vertical = 20
                                        },
                                        ColumnDimensions = new[]
                                        {
                                            new Dimension(),
                                            new Dimension(GridSizeMode.Absolute, 50),
                                            new Dimension(GridSizeMode.AutoSize),
                                        },
                                        RowDimensions = new[]
                                        {
                                            new Dimension(GridSizeMode.AutoSize),
                                        },
                                        Content = new[]
                                        {
                                            new[]
                                            {
                                                searchTextBox = new BasicSearchTextBox
                                                {
                                                    RelativeSizeAxes = Axes.X,
                                                    Anchor = Anchor.CentreLeft,
                                                    Origin = Anchor.CentreLeft,
                                                    Height = 40,
                                                    ReleaseFocusOnCommit = false,
                                                    HoldFocus = true,
                                                    PlaceholderText = HomeStrings.SearchPlaceholder,
                                                },
                                                Empty(),
                                                userListToolbar = new UserListToolbar(supportsBrickMode: true, supportsSort: false)
                                                {
                                                    Anchor = Anchor.CentreRight,
                                                    Origin = Anchor.CentreRight,
                                                },
                                            },
                                        },
                                    },
                                    new Container
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Children = new Drawable[]
                                        {
                                            listContainer = new Container
                                            {
                                                RelativeSizeAxes = Axes.X,
                                                AutoSizeAxes = Axes.Y,
                                                Padding = new MarginPadding { Horizontal = WaveOverlayContainer.HORIZONTAL_PADDING }
                                            },
                                        }
                                    }
                                }
                            },
                        }
                    }
                }
            };

            background.Colour = colourProvider.Background4;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            searchTextBox.Current.BindValueChanged(_ => queueUpdateSearch());
            userListToolbar.DisplayStyle.BindValueChanged(_ => performSearch());
        }

        private ScheduledDelegate? queryChangedDebounce;

        private void queueUpdateSearch()
        {
            queryChangedDebounce?.Cancel();

            if (!api.IsLoggedIn)
            {
                clearPreviousResults();
                return;
            }

            if (string.IsNullOrEmpty(searchTextBox.Current.Value))
            {
                loading.Value = false;
                return;
            }

            queryChangedDebounce = Scheduler.AddDelayed(performSearch, 500);
        }

        private void performSearch()
        {
            loading.Value = true;
            var getUsersRequest = new SearchUsersRequest(searchTextBox.Current.Value);
            getUsersRequest.Success += showResults;
            api.Queue(getUsersRequest);
        }

        private void showResults(SearchUsersResponse response)
        {
            clearPreviousResults();
            var friendsList = new UserPanelList(userListToolbar.DisplayStyle.Value, response.Users.ToArray());
            listContainer.Add(friendsList);

            friendsList.FadeInFromZero(500, Easing.OutQuint);
        }

        private void clearPreviousResults()
        {
            foreach (var child in listContainer.Children)
                child.FadeOut(200).Expire();
            listContainer.Clear();

            loading.Value = false;
        }
    }
}
