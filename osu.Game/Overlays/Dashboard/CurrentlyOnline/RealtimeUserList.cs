// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Database;
using osu.Game.Extensions;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Metadata;
using osu.Game.Overlays.Dashboard.Friends;
using osu.Game.Rulesets;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Overlays.Dashboard.CurrentlyOnline
{
    internal partial class RealtimeUserList : CompositeDrawable
    {
        public readonly IBindable<UserSortCriteria> SortCriteria = new Bindable<UserSortCriteria>();
        public readonly IBindable<string> SearchText = new Bindable<string>();

        private readonly IBindableDictionary<int, UserPresence> onlineUserPresences = new BindableDictionary<int, UserPresence>();
        private readonly Dictionary<int, OnlineUserPanel> userPanels = new Dictionary<int, OnlineUserPanel>();
        private readonly OverlayPanelDisplayStyle style;

        private OnlineUserSearchContainer searchContainer = null!;

        [Resolved]
        private MetadataClient metadataClient { get; set; } = null!;

        [Cached(typeof(UserLookupCache))] // not used at the moment.
        private UserWithRankLookupCache userCache { get; set; } = new UserWithRankLookupCache();

        public RealtimeUserList(OverlayPanelDisplayStyle style)
        {
            this.style = style;

            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            AddInternal(userCache);

            InternalChild = searchContainer = new OnlineUserSearchContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Spacing = new Vector2(style == OverlayPanelDisplayStyle.Card ? 10 : 3),
                SortCriteria = { BindTarget = SortCriteria },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            onlineUserPresences.BindTo(metadataClient.UserPresences);
            onlineUserPresences.BindCollectionChanged(onUserPresenceUpdated, true);

            SearchText.BindValueChanged(onSearchTextChanged, true);

            Scheduler.AddDelayed(updateUsers, 2000, true);
        }

        private void onSearchTextChanged(ValueChangedEvent<string> search)
        {
            searchContainer.SearchTerm = search.NewValue;
        }

        private void onUserPresenceUpdated(object? sender, NotifyDictionaryChangedEventArgs<int, UserPresence> e)
        {
            switch (e.Action)
            {
                case NotifyDictionaryChangedAction.Replace:
                    foreach ((int userId, var presence) in e.NewItems!)
                    {
                        if (userPanels.TryGetValue(userId, out var userPanel))
                            updateUserSpectateState(presence, userPanel);
                    }

                    break;

                case NotifyDictionaryChangedAction.Add:
                    pendingUsers.AddRange(e.NewItems!.Select(i => i.Key));

                    break;

                case NotifyDictionaryChangedAction.Remove:
                    foreach ((int userId, _) in e.OldItems!)
                    {
                        if (userPanels.Remove(userId, out var userPanel))
                            userPanel.Expire();

                        pendingUsers.Remove(userId);
                    }

                    break;
            }
        }

        private readonly HashSet<int> pendingUsers = new HashSet<int>();

        protected override void Update()
        {
            base.Update();

            // ReSharper disable once InconsistentlySynchronizedField
            if (pendingUsers.Count > 100)
                updateUsers();
        }

        private void updateUsers()
        {
            if (pendingUsers.Count == 0)
                return;

            // partitioning here is just to break up the requests.
            // without this, the intitial request will take seconds to minutes.
            const int partition_size = 50;

            for (int i = 0; i <= pendingUsers.Count / partition_size; i++)
            {
                int[] partitionedUsers = pendingUsers.Skip(i * partition_size).Take(partition_size).ToArray();

                userCache.GetUsersAsync(partitionedUsers).ContinueWith(task => Schedule(() =>
                {
                    var users = task.GetResultSafely();

                    foreach (APIUser? user in users)
                    {
                        if (user == null)
                            continue;

                        var presence = metadataClient.GetPresence(user.Id);

                        if (presence == null)
                            continue;

                        if (userPanels.TryGetValue(user.Id, out _))
                            return;

                        // This is quite dodgy – it affects the global `UserLookupCache`.
                        //
                        // but it's the best we can do for now.
                        // this should probaly be returned by server-spectator not osu-web.
                        user.LastVisit = DateTimeOffset.Now;

                        var panel = createUserPanel(user);
                        updateUserSpectateState(presence.Value, panel);
                        searchContainer.Add(userPanels[user.Id] = panel);
                    }
                }));
            }

            pendingUsers.Clear();
        }

        private static void updateUserSpectateState(UserPresence presence, OnlineUserPanel userPanel)
        {
            switch (presence.Activity)
            {
                default:
                    userPanel.CanSpectate.Value = false;
                    break;

                case UserActivity.InSoloGame:
                case UserActivity.InMultiplayerGame:
                case UserActivity.InPlaylistGame:
                    userPanel.CanSpectate.Value = true;
                    break;
            }
        }

        private OnlineUserPanel createUserPanel(APIUser user)
        {
            switch (style)
            {
                default:
                case OverlayPanelDisplayStyle.Card:
                    return new OnlineUserGridPanel(user)
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre
                    };

                case OverlayPanelDisplayStyle.List:
                    return new OnlineUserListPanel(user);
            }
        }

        private partial class OnlineUserSearchContainer : SearchContainer<OnlineUserPanel>
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
                    IEnumerable<OnlineUserPanel> panels = base.FlowingChildren.OfType<OnlineUserPanel>();

                    switch (SortCriteria.Value)
                    {
                        default:
                        case UserSortCriteria.LastVisit:
                            // Todo: Last visit time is not currently updated according to realtime user presence.
                            return panels.OrderByDescending(panel => panel.User.LastVisit).ThenBy(panel => panel.User.Id);

                        case UserSortCriteria.Rank:
                            // Todo: Rank is not currently displayed in the panels. Additionally the sort mode kind of breaks if you change ruleset with this overlay open.
                            return panels.OrderByDescending(panel => panel.User.Rank?.Rank != null).ThenBy(panel => panel.User.Rank?.Rank ?? 0);

                        case UserSortCriteria.Username:
                            return panels.OrderBy(panel => panel.User.Username);
                    }
                }
            }
        }

        /// <summary>
        /// This is implemented local to avoid invalidating the full cache on ruleset change at a global `UserLookupCache` level.
        /// We should probably do better than this (server-spectator sending the rank data instead? something else?).
        /// </summary>
        private partial class UserWithRankLookupCache : UserLookupCache
        {
            [Resolved]
            private IBindable<RulesetInfo> ruleset { get; set; } = null!;

            protected override void LoadComplete()
            {
                base.LoadComplete();

                ruleset.BindValueChanged(ruleset =>
                {
                    if (ruleset.OldValue?.OnlineID != ruleset.NewValue?.OnlineID)
                        Clear();
                });
            }

            protected override LookupUsersRequest CreateRequest(IEnumerable<int> ids) => new LookupUsersRequest(ids.ToArray(), ruleset.Value?.OnlineID >= 0 ? ruleset.Value.OnlineID : null);
        }
    }
}
