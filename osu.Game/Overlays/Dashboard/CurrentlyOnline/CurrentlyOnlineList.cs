// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Database;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Metadata;
using osu.Game.Overlays.Dashboard.Friends;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Overlays.Dashboard.CurrentlyOnline
{
    internal partial class CurrentlyOnlineList : CompositeDrawable
    {
        public readonly IBindable<UserSortCriteria> SortCriteria = new Bindable<UserSortCriteria>();
        public readonly IBindable<string> SearchText = new Bindable<string>();

        private readonly IBindableDictionary<int, UserPresence> onlineUserPresences = new BindableDictionary<int, UserPresence>();
        private readonly Dictionary<int, OnlineUserGridPanel> userPanels = new Dictionary<int, OnlineUserGridPanel>();
        private readonly OverlayPanelDisplayStyle style;

        private OnlineUserSearchContainer searchContainer = null!;

        [Resolved]
        private MetadataClient metadataClient { get; set; } = null!;

        [Resolved]
        private UserLookupCache users { get; set; } = null!;

        public CurrentlyOnlineList(OverlayPanelDisplayStyle style)
        {
            this.style = style;

            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = searchContainer = new OnlineUserSearchContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Spacing = new Vector2(style == OverlayPanelDisplayStyle.Card ? 10 : 2),
                SortCriteria = { BindTarget = SortCriteria },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            onlineUserPresences.BindTo(metadataClient.UserPresences);
            onlineUserPresences.BindCollectionChanged(onUserPresenceUpdated, true);

            SearchText.BindValueChanged(onSearchTextChanged, true);
        }

        private void onSearchTextChanged(ValueChangedEvent<string> search)
        {
            searchContainer.SearchTerm = search.NewValue;
        }

        private void onUserPresenceUpdated(object? sender, NotifyDictionaryChangedEventArgs<int, UserPresence> e) => Schedule(() =>
        {
            switch (e.Action)
            {
                case NotifyDictionaryChangedAction.Add:
                    Debug.Assert(e.NewItems != null);

                    foreach (var kvp in e.NewItems)
                    {
                        int userId = kvp.Key;

                        users.GetUserAsync(userId).ContinueWith(task =>
                        {
                            if (task.GetResultSafely() is APIUser user)
                                Schedule(() => searchContainer.Add(userPanels[userId] = createUserPanel(user)));
                        });
                    }

                    break;

                case NotifyDictionaryChangedAction.Remove:
                    Debug.Assert(e.OldItems != null);

                    foreach (var kvp in e.OldItems)
                    {
                        int userId = kvp.Key;
                        if (userPanels.Remove(userId, out var userPanel))
                            userPanel.Expire();
                    }

                    break;
            }
        });

        private OnlineUserGridPanel createUserPanel(APIUser user) =>
            new OnlineUserGridPanel(user).With(panel =>
            {
                panel.Anchor = Anchor.TopCentre;
                panel.Origin = Anchor.TopCentre;
            });

        private partial class OnlineUserSearchContainer : SearchContainer<OnlineUserGridPanel>
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
                    IEnumerable<OnlineUserGridPanel> panels = base.FlowingChildren.OfType<OnlineUserGridPanel>();

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
    }
}
