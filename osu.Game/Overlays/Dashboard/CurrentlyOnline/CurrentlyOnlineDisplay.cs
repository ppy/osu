// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Dashboard.Friends;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Overlays.Dashboard.CurrentlyOnline
{
    public partial class CurrentlyOnlineDisplay : CompositeDrawable
    {
        private Box background = null!;
        private UserListToolbar userListToolbar = null!;
        private Container<RealtimeUserList> listContainer = null!;
        private LoadingLayer loading = null!;
        private BasicSearchTextBox searchTextBox = null!;

        private CancellationTokenSource? listLoadCancellation;

        public CurrentlyOnlineDisplay()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            InternalChildren = new Drawable[]
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
                                    userListToolbar = new UserListToolbar(false)
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
                                listContainer = new Container<RealtimeUserList>
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
            };

            background.Colour = colourProvider.Background4;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            userListToolbar.DisplayStyle.BindValueChanged(_ => reloadList(), true);
        }

        private void reloadList()
        {
            listLoadCancellation?.Cancel();
            var cancellationSource = listLoadCancellation = new CancellationTokenSource();

            RealtimeUserList? currentList = listContainer.SingleOrDefault();
            RealtimeUserList newList = new RealtimeUserList(userListToolbar.DisplayStyle.Value)
            {
                SortCriteria = { BindTarget = userListToolbar.SortCriteria },
                SearchText = { BindTarget = searchTextBox.Current }
            };

            loading.Show();
            LoadComponentAsync(newList, finishLoad, cancellationSource.Token);

            void finishLoad(RealtimeUserList list)
            {
                loading.Hide();

                if (currentList != null)
                {
                    currentList.FadeOut(250, Easing.OutQuint).Expire();
                    currentList.Delay(25).Schedule(() => currentList.BypassAutoSizeAxes = Axes.Y);
                }

                listContainer.Add(newList);
                newList.FadeInFromZero(250, Easing.OutQuint);
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
