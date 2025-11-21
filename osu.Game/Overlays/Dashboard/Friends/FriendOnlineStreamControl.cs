// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Metadata;
using osu.Game.Users;

namespace osu.Game.Overlays.Dashboard.Friends
{
    public partial class FriendOnlineStreamControl : OverlayStreamControl<OnlineStatus>
    {
        private readonly IBindableDictionary<int, UserPresence> friendPresences = new BindableDictionary<int, UserPresence>();
        private readonly IBindableList<APIRelation> apiFriends = new BindableList<APIRelation>();
        private readonly BindableInt countAll = new BindableInt();
        private readonly BindableInt countOnline = new BindableInt();
        private readonly BindableInt countOffline = new BindableInt();

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        [Resolved]
        private MetadataClient metadataClient { get; set; } = null!;

        public FriendOnlineStreamControl()
        {
            Items =
            [
                OnlineStatus.All,
                OnlineStatus.Online,
                OnlineStatus.Offline
            ];
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            apiFriends.BindTo(api.LocalUserState.Friends);
            apiFriends.BindCollectionChanged((_, _) => updateCounts());

            friendPresences.BindTo(metadataClient.FriendPresences);
            friendPresences.BindCollectionChanged(onFriendPresencesChanged);

            updateCounts();
        }

        private void onFriendPresencesChanged(object? sender, NotifyDictionaryChangedEventArgs<int, UserPresence> e)
        {
            switch (e.Action)
            {
                case NotifyDictionaryChangedAction.Add:
                case NotifyDictionaryChangedAction.Remove:
                    updateCounts();
                    break;
            }
        }

        private void updateCounts()
        {
            countAll.Value = apiFriends.Count;
            countOnline.Value = 0;
            countOffline.Value = 0;

            foreach (var user in apiFriends)
            {
                if (friendPresences.ContainsKey(user.TargetID))
                    countOnline.Value++;
                else
                    countOffline.Value++;
            }
        }

        protected override OverlayStreamItem<OnlineStatus> CreateStreamItem(OnlineStatus value)
        {
            switch (value)
            {
                case OnlineStatus.All:
                    return new FriendsOnlineStatusItem(value) { UserCount = { BindTarget = countAll } };

                case OnlineStatus.Online:
                    return new FriendsOnlineStatusItem(value) { UserCount = { BindTarget = countOnline } };

                case OnlineStatus.Offline:
                    return new FriendsOnlineStatusItem(value) { UserCount = { BindTarget = countOffline } };

                default:
                    throw new ArgumentException(nameof(value));
            }
        }
    }
}
