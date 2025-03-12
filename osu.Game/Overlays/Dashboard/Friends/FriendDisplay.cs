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
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Overlays.Dashboard.Friends
{
    public partial class FriendDisplay : CompositeDrawable
    {
        private readonly IBindableList<APIRelation> apiFriends = new BindableList<APIRelation>();

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        private FriendOnlineStreamControl streamControl = null!;
        private Box background = null!;
        private Box controlBackground = null!;
        private UserListToolbar userListToolbar = null!;
        private Container<FriendsList> listContainer = null!;
        private LoadingLayer loading = null!;
        private BasicSearchTextBox searchTextBox = null!;

        private CancellationTokenSource? listLoadCancellation;

        public FriendDisplay()
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
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Children = new Drawable[]
                        {
                            controlBackground = new Box
                            {
                                RelativeSizeAxes = Axes.Both
                            },
                            new Container
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Padding = new MarginPadding
                                {
                                    Top = 20,
                                    Horizontal = WaveOverlayContainer.HORIZONTAL_PADDING - FriendsOnlineStatusItem.PADDING
                                },
                                Child = streamControl = new FriendOnlineStreamControl(),
                            }
                        }
                    },
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
                                                userListToolbar = new UserListToolbar
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
                                            listContainer = new Container<FriendsList>
                                            {
                                                RelativeSizeAxes = Axes.X,
                                                AutoSizeAxes = Axes.Y,
                                                Padding = new MarginPadding { Horizontal = WaveOverlayContainer.HORIZONTAL_PADDING }
                                            },
                                            loading = new LoadingLayer(true)
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            background.Colour = colourProvider.Background4;
            controlBackground.Colour = colourProvider.Background5;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            apiFriends.BindTo(api.Friends);
            apiFriends.BindCollectionChanged((_, _) => reloadList());

            userListToolbar.DisplayStyle.BindValueChanged(_ => reloadList(), true);
        }

        private void reloadList()
        {
            listLoadCancellation?.Cancel();
            var cancellationSource = listLoadCancellation = new CancellationTokenSource();

            FriendsList? currentList = listContainer.SingleOrDefault();
            FriendsList newList = new FriendsList(userListToolbar.DisplayStyle.Value, apiFriends.Select(f => f.TargetUser!).ToArray())
            {
                OnlineStream = { BindTarget = streamControl.Current },
                SortCriteria = { BindTarget = userListToolbar.SortCriteria },
                SearchText = { BindTarget = searchTextBox.Current }
            };

            loading.Show();
            LoadComponentAsync(newList, finishLoad, cancellationSource.Token);

            void finishLoad(FriendsList list)
            {
                loading.Hide();

                if (currentList != null)
                {
                    currentList.FadeOut(100, Easing.OutQuint).Expire();
                    currentList.Delay(25).Schedule(() => currentList.BypassAutoSizeAxes = Axes.Y);
                }

                listContainer.Add(newList);
                newList.FadeIn(200, Easing.OutQuint);
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            listLoadCancellation?.Cancel();
            listLoadCancellation?.Dispose();
        }
    }
}
