// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.Metadata;
using osu.Game.Overlays.Dashboard.Friends;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Overlays.Dashboard.CurrentlyOnline
{
    public partial class CurrentlyOnlineDisplay : CompositeDrawable
    {
        public IBindable<bool> Loading => loading;
        private readonly BindableBool loading = new BindableBool();

        /// <summary>
        /// The current state of the <see cref="DashboardOverlay"/>.
        /// Presence is only updated when this value is <see cref="Visibility.Visible"/>.
        /// </summary>
        public readonly Bindable<Visibility> OverlayState = new Bindable<Visibility>(Visibility.Visible);

        [Resolved]
        private MetadataClient metadataClient { get; set; } = null!;

        private readonly IBindable<bool> isConnected = new Bindable<bool>();

        private Box background = null!;
        private UserListToolbar userListToolbar = null!;
        private Container<RealtimeUserList> listContainer = null!;
        private BasicSearchTextBox searchTextBox = null!;

        private CancellationTokenSource? listLoadCancellation;
        private IDisposable? userPresenceWatchToken;

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
                                    userListToolbar = new UserListToolbar(supportsBrickMode: false)
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

            isConnected.BindTo(metadataClient.IsConnected);
            isConnected.BindValueChanged(_ => updateUserPresenceState());

            OverlayState.BindValueChanged(_ => updateUserPresenceState(), true);

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

            loading.Value = true;
            LoadComponentAsync(newList, finishLoad, cancellationSource.Token);

            void finishLoad(RealtimeUserList list)
            {
                loading.Value = false;

                if (currentList != null)
                {
                    currentList.FadeOut(250, Easing.OutQuint).Expire();
                    currentList.Delay(25).Schedule(() => currentList.BypassAutoSizeAxes = Axes.Y);
                }

                listContainer.Add(newList);
                newList.FadeInFromZero(250, Easing.OutQuint);
            }
        }

        private void updateUserPresenceState()
        {
            if (!isConnected.Value)
                return;

            if (OverlayState.Value == Visibility.Visible)
                userPresenceWatchToken ??= metadataClient.BeginWatchingUserPresence();
            else
            {
                userPresenceWatchToken?.Dispose();
                userPresenceWatchToken = null;
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            listLoadCancellation?.Cancel();
            listLoadCancellation?.Dispose();

            userPresenceWatchToken?.Dispose();
        }
    }
}
