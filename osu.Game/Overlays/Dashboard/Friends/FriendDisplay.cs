// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Metadata;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Overlays.Dashboard.Friends
{
    public partial class FriendDisplay : CompositeDrawable
    {
        private readonly IBindableList<APIRelation> apiFriends = new BindableList<APIRelation>();
        private readonly IBindableDictionary<int, UserPresence> friendPresences = new BindableDictionary<int, UserPresence>();

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        [Resolved]
        private MetadataClient metadataClient { get; set; } = null!;

        private FriendOnlineStreamControl streamControl = null!;
        private Box background = null!;
        private Box controlBackground = null!;
        private UserListToolbar userListToolbar = null!;
        private LoadingLayer loading = null!;
        private BasicSearchTextBox searchTextBox = null!;
        private FriendsSearchContainer panelsContainer = null!;

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
                                            panelsContainer = new FriendsSearchContainer
                                            {
                                                RelativeSizeAxes = Axes.X,
                                                AutoSizeAxes = Axes.Y,
                                                Padding = new MarginPadding { Horizontal = WaveOverlayContainer.HORIZONTAL_PADDING },
                                                // Todo: Spacing = new Vector2(style == OverlayPanelDisplayStyle.Card ? 10 : 2),
                                                Spacing = new Vector2(10),
                                                SortCriteria = { BindTarget = userListToolbar.SortCriteria }
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
            apiFriends.BindCollectionChanged(onFriendsChanged, true);

            friendPresences.BindTo(metadataClient.FriendPresences);
            friendPresences.BindCollectionChanged(onFriendPresencesChanged, true);

            searchTextBox.Current.BindValueChanged(onSearchChanged);
            streamControl.Current.BindValueChanged(onFriendsStreamChanged);
        }

        private void onFriendsChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (APIRelation relation in e.NewItems!.OfType<APIRelation>())
                    {
                        panelsContainer.Add(new FilterableUserPanel(new UserGridPanel(relation.TargetUser!).With(panel =>
                        {
                            panel.Anchor = Anchor.TopCentre;
                            panel.Origin = Anchor.TopCentre;
                            panel.Width = 290;
                        })));
                    }

                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (APIRelation relation in e.OldItems!.OfType<APIRelation>())
                        panelsContainer.RemoveAll(panel => panel.User.Equals(relation.TargetUser), true);

                    break;
            }

            updatePanelVisibilities();
            updateStatusCounts();
        }

        private void onFriendPresencesChanged(object? sender, NotifyDictionaryChangedEventArgs<int, UserPresence> e)
        {
            switch (e.Action)
            {
                case NotifyDictionaryChangedAction.Add:
                case NotifyDictionaryChangedAction.Remove:
                    updatePanelVisibilities();
                    updateStatusCounts();
                    break;
            }
        }

        private void onFriendsStreamChanged(ValueChangedEvent<OnlineStatus> stream)
        {
            updatePanelVisibilities();
        }

        private void onSearchChanged(ValueChangedEvent<string> search)
        {
            panelsContainer.SearchTerm = search.NewValue;
        }

        private void updatePanelVisibilities()
        {
            foreach (var panel in panelsContainer)
            {
                switch (streamControl.Current.Value)
                {
                    case OnlineStatus.All:
                        panel.CanBeShown.Value = true;
                        break;

                    case OnlineStatus.Online:
                        panel.CanBeShown.Value = friendPresences.ContainsKey(panel.User.OnlineID);
                        break;

                    case OnlineStatus.Offline:
                        panel.CanBeShown.Value = !friendPresences.ContainsKey(panel.User.OnlineID);
                        break;
                }
            }
        }

        private void updateStatusCounts()
        {
            int countOnline = 0;
            int countOffline = 0;

            foreach (var user in apiFriends)
            {
                if (friendPresences.ContainsKey(user.TargetID))
                    countOnline++;
                else
                    countOffline++;
            }

            streamControl.CountAll.Value = apiFriends.Count;
            streamControl.CountOnline.Value = countOnline;
            streamControl.CountOffline.Value = countOffline;
        }

        private class FriendsSearchContainer : SearchContainer<FilterableUserPanel>
        {
            public readonly IBindable<UserSortCriteria> SortCriteria = new Bindable<UserSortCriteria>();

            protected override void LoadComplete()
            {
                base.LoadComplete();
                SortCriteria.BindValueChanged(_ => InvalidateLayout(), true);
            }

            public override IEnumerable<Drawable> FlowingChildren
            {
                get
                {
                    IEnumerable<FilterableUserPanel> panels = base.FlowingChildren.OfType<FilterableUserPanel>();

                    switch (SortCriteria.Value)
                    {
                        default:
                        case UserSortCriteria.LastVisit:
                            return panels.OrderByDescending(panel => panel.User.LastVisit);

                        case UserSortCriteria.Rank:
                            return panels.OrderByDescending(panel => panel.User.Statistics.GlobalRank.HasValue).ThenBy(panel => panel.User.Statistics.GlobalRank ?? 0);

                        case UserSortCriteria.Username:
                            return panels.OrderBy(panel => panel.User.Username);
                    }
                }
            }
        }

        private class FilterableUserPanel : CompositeDrawable, IConditionalFilterable
        {
            public readonly Bindable<bool> CanBeShown = new Bindable<bool>();

            public APIUser User => panel.User;

            private readonly UserPanel panel;

            public FilterableUserPanel(UserPanel panel)
            {
                this.panel = panel;

                Anchor = panel.Anchor;
                Origin = panel.Origin;
                RelativeSizeAxes = panel.RelativeSizeAxes;
                AutoSizeAxes = panel.AutoSizeAxes;
                Width = panel.Width;
                Height = panel.Height;

                InternalChild = panel;
            }

            IBindable<bool> IConditionalFilterable.CanBeShown => CanBeShown;

            IEnumerable<LocalisableString> IHasFilterTerms.FilterTerms => panel.FilterTerms;

            bool IFilterable.MatchingFilter
            {
                set
                {
                    if (value)
                        Show();
                    else
                        Hide();
                }
            }

            bool IFilterable.FilteringActive { set { } }
        }
    }
}
