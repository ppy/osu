// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Localisation;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Chat;
using osu.Game.Online.Metadata;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;
using osu.Game.Users;
using osuTK.Graphics;

namespace osu.Game.Online
{
    public partial class FriendPresenceNotifier : Component
    {
        [Resolved]
        private INotificationOverlay notifications { get; set; } = null!;

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        [Resolved]
        private MetadataClient metadataClient { get; set; } = null!;

        [Resolved]
        private OsuConfigManager config { get; set; } = null!;

        private readonly Bindable<bool> notifyOnFriendPresenceChange = new BindableBool();

        private readonly IBindableList<APIRelation> friends = new BindableList<APIRelation>();
        private readonly IBindableDictionary<int, UserPresence> friendPresences = new BindableDictionary<int, UserPresence>();

        private readonly HashSet<APIUser> onlineAlertQueue = new HashSet<APIUser>();
        private readonly HashSet<APIUser> offlineAlertQueue = new HashSet<APIUser>();

        private double? nextOnlineAlertTime;
        private double? nextOfflineAlertTime;

        private const double debounce_time_before_notification = 1000;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            config.BindWith(OsuSetting.NotifyOnFriendPresenceChange, notifyOnFriendPresenceChange);
            notifyOnFriendPresenceChange.BindValueChanged(_ =>
            {
                onlineAlertQueue.Clear();
                offlineAlertQueue.Clear();

                nextOfflineAlertTime = null;
                nextOnlineAlertTime = null;
            });

            friends.BindTo(api.LocalUserState.Friends);
            friends.BindCollectionChanged(onFriendsChanged, true);

            friendPresences.BindTo(metadataClient.FriendPresences);
            friendPresences.BindCollectionChanged(onFriendPresenceChanged, true);
        }

        protected override void Update()
        {
            base.Update();

            if (notifyOnFriendPresenceChange.Value)
            {
                alertOnlineUsers();
                alertOfflineUsers();
            }
        }

        private void onFriendsChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (APIRelation friend in e.NewItems!.Cast<APIRelation>())
                    {
                        if (friend.TargetUser is not APIUser user)
                            continue;

                        if (friendPresences.TryGetValue(friend.TargetID, out _))
                            markUserOnline(user);
                    }

                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (APIRelation friend in e.OldItems!.Cast<APIRelation>())
                    {
                        if (friend.TargetUser is not APIUser user)
                            continue;

                        onlineAlertQueue.Remove(user);
                        offlineAlertQueue.Remove(user);
                    }

                    break;
            }
        }

        private void onFriendPresenceChanged(object? sender, NotifyDictionaryChangedEventArgs<int, UserPresence> e)
        {
            switch (e.Action)
            {
                case NotifyDictionaryChangedAction.Add:
                    foreach ((int friendId, _) in e.NewItems!)
                    {
                        APIRelation? friend = friends.FirstOrDefault(f => f.TargetID == friendId);

                        if (friend?.TargetUser is APIUser user)
                            markUserOnline(user);
                    }

                    break;

                case NotifyDictionaryChangedAction.Remove:
                    foreach ((int friendId, _) in e.OldItems!)
                    {
                        APIRelation? friend = friends.FirstOrDefault(f => f.TargetID == friendId);

                        if (friend?.TargetUser is APIUser user)
                            markUserOffline(user);
                    }

                    break;
            }
        }

        private void markUserOnline(APIUser user)
        {
            if (!offlineAlertQueue.Remove(user))
            {
                onlineAlertQueue.Add(user);
                nextOnlineAlertTime ??= Time.Current + debounce_time_before_notification;
            }
        }

        private void markUserOffline(APIUser user)
        {
            if (!onlineAlertQueue.Remove(user))
            {
                offlineAlertQueue.Add(user);
                nextOfflineAlertTime ??= Time.Current + debounce_time_before_notification;
            }
        }

        private void alertOnlineUsers()
        {
            if (nextOnlineAlertTime == null || Time.Current < nextOnlineAlertTime)
                return;

            // If a user quickly switches online-offline, we might reach here without actually having a notification
            // to fire. Importantly, we should still reset the next alert time in such a scenario.

            if (onlineAlertQueue.Count == 1)
                notifications.Post(new SingleFriendOnlineNotification(onlineAlertQueue.Single()));
            else if (onlineAlertQueue.Count > 1)
                notifications.Post(new MultipleFriendsOnlineNotification(onlineAlertQueue.ToArray()));

            onlineAlertQueue.Clear();
            nextOnlineAlertTime = null;
        }

        private void alertOfflineUsers()
        {
            if (nextOfflineAlertTime == null || Time.Current < nextOfflineAlertTime)
                return;

            // If a user quickly switches offline-online, we might reach here without actually having a notification
            // to fire. Importantly, we should still reset the next alert time in such a scenario.

            if (offlineAlertQueue.Count == 1)
                notifications.Post(new SingleFriendOfflineNotification(offlineAlertQueue.Single()));
            else if (offlineAlertQueue.Count > 1)
                notifications.Post(new MultipleFriendsOfflineNotification(offlineAlertQueue.ToArray()));

            offlineAlertQueue.Clear();
            nextOfflineAlertTime = null;
        }

        private partial class SingleFriendOnlineNotification : UserAvatarNotification
        {
            public SingleFriendOnlineNotification(APIUser user)
                : base(user)
            {
                Transient = true;
                IsImportant = false;
                Text = NotificationsStrings.FriendOnline(User.Username);
            }

            [BackgroundDependencyLoader]
            private void load(ChannelManager channelManager, ChatOverlay chatOverlay)
            {
                Activated = () =>
                {
                    channelManager.OpenPrivateChannel(User);
                    chatOverlay.Show();

                    return true;
                };
            }

            public override string PopInSampleName => "UI/notification-friend-online";
        }

        private partial class MultipleFriendsOnlineNotification : SimpleNotification
        {
            public MultipleFriendsOnlineNotification(ICollection<APIUser> users)
            {
                Text = NotificationsStrings.FriendOnline(string.Join(@", ", users.Select(u => u.Username)));
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                Icon = FontAwesome.Solid.User;
                IconColour = colours.Green;
            }

            public override string PopInSampleName => "UI/notification-friend-online";
        }

        private partial class SingleFriendOfflineNotification : UserAvatarNotification
        {
            public SingleFriendOfflineNotification(APIUser user)
                : base(user)
            {
                Transient = true;
                IsImportant = false;
                Text = NotificationsStrings.FriendOffline(User.Username);
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                Icon = FontAwesome.Solid.UserSlash;
                Avatar.Colour = Color4.White.Opacity(0.25f);
            }

            public override string PopInSampleName => "UI/notification-friend-offline";
        }

        private partial class MultipleFriendsOfflineNotification : SimpleNotification
        {
            public MultipleFriendsOfflineNotification(ICollection<APIUser> users)
            {
                Text = NotificationsStrings.FriendOffline(string.Join(@", ", users.Select(u => u.Username)));
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                Icon = FontAwesome.Solid.UserSlash;
                IconColour = colours.Red;
            }

            public override string PopInSampleName => "UI/notification-friend-offline";
        }
    }
}
