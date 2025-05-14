// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.EnumExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Metadata;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Overlays.Dashboard.Friends
{
    public partial class FriendsList : CompositeDrawable
    {
        public readonly IBindable<OnlineStatus> OnlineStream = new Bindable<OnlineStatus>();
        public readonly IBindable<UserSortCriteria> SortCriteria = new Bindable<UserSortCriteria>();
        public readonly IBindable<string> SearchText = new Bindable<string>();

        [Resolved]
        private MetadataClient metadataClient { get; set; } = null!;

        private readonly IBindableDictionary<int, UserPresence> friendPresences = new BindableDictionary<int, UserPresence>();
        private readonly OverlayPanelDisplayStyle style;
        private readonly APIUser[] friends;

        private FriendsSearchContainer searchContainer = null!;

        public FriendsList(OverlayPanelDisplayStyle style, APIUser[] friends)
        {
            this.style = style;
            this.friends = friends;

            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = searchContainer = new FriendsSearchContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Spacing = new Vector2(style == OverlayPanelDisplayStyle.Card ? 10 : 2),
                SortCriteria = { BindTarget = SortCriteria },
                ChildrenEnumerable = friends.Select(createUserPanel)
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            friendPresences.BindTo(metadataClient.FriendPresences);
            friendPresences.BindCollectionChanged(onFriendPresencesChanged);

            SearchText.BindValueChanged(onSearchTextChanged, true);
            OnlineStream.BindValueChanged(onFriendsStreamChanged, true);
        }

        private void onFriendPresencesChanged(object? sender, NotifyDictionaryChangedEventArgs<int, UserPresence> e)
        {
            switch (e.Action)
            {
                case NotifyDictionaryChangedAction.Add:
                case NotifyDictionaryChangedAction.Remove:
                    updatePanelVisibilities();
                    break;
            }
        }

        private void onSearchTextChanged(ValueChangedEvent<string> search)
        {
            searchContainer.SearchTerm = search.NewValue;
        }

        private void onFriendsStreamChanged(ValueChangedEvent<OnlineStatus> stream)
        {
            updatePanelVisibilities();
        }

        private void updatePanelVisibilities()
        {
            foreach (var panel in searchContainer)
            {
                switch (OnlineStream.Value)
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

        private FilterableUserPanel createUserPanel(APIUser user)
        {
            UserPanel panel;

            switch (style)
            {
                default:
                case OverlayPanelDisplayStyle.Card:
                    panel = new UserGridPanel(user);
                    panel.Anchor = Anchor.TopCentre;
                    panel.Origin = Anchor.TopCentre;
                    panel.Width = 290;
                    break;

                case OverlayPanelDisplayStyle.List:
                    panel = new UserListPanel(user);
                    break;

                case OverlayPanelDisplayStyle.Brick:
                    panel = new UserBrickPanel(user);
                    break;
            }

            return new FilterableUserPanel(panel);
        }

        private partial class FriendsSearchContainer : SearchContainer<FilterableUserPanel>
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
                            // Todo: Last visit time is not currently updated according to realtime user presence.
                            return panels.OrderByDescending(panel => panel.User.LastVisit);

                        case UserSortCriteria.Rank:
                            // Todo: Statistics are not currently updated according to realtime user statistics, but it's also not currently displayed in the panels.
                            return panels.OrderByDescending(panel => panel.User.Statistics.GlobalRank.HasValue).ThenBy(panel => panel.User.Statistics.GlobalRank ?? 0);

                        case UserSortCriteria.Username:
                            return panels.OrderBy(panel => panel.User.Username);
                    }
                }
            }
        }

        public partial class FilterableUserPanel : CompositeDrawable, IConditionalFilterable
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

                if (!AutoSizeAxes.HasFlagFast(Axes.X))
                    Width = panel.Width;

                if (!AutoSizeAxes.HasFlagFast(Axes.Y))
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
