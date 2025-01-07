// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Chat;
using osu.Game.Online.Metadata;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;
using osu.Game.Users;

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
        private ChannelManager channelManager { get; set; } = null!;

        [Resolved]
        private ChatOverlay chatOverlay { get; set; } = null!;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        private readonly IBindableDictionary<int, UserPresence> userStates = new BindableDictionary<int, UserPresence>();
        private readonly HashSet<APIUser> onlineAlertQueue = new HashSet<APIUser>();
        private readonly HashSet<APIUser> offlineAlertQueue = new HashSet<APIUser>();

        private double? lastOnlineAlertTime;
        private double? lastOfflineAlertTime;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            userStates.BindTo(metadataClient.UserStates);
            userStates.BindCollectionChanged((_, args) =>
            {
                switch (args.Action)
                {
                    case NotifyDictionaryChangedAction.Add:
                        foreach ((int userId, var _) in args.NewItems!)
                        {
                            if (api.GetFriend(userId)?.TargetUser is APIUser user)
                            {
                                if (!offlineAlertQueue.Remove(user))
                                {
                                    onlineAlertQueue.Add(user);
                                    lastOnlineAlertTime ??= Time.Current;
                                }
                            }
                        }

                        break;

                    case NotifyDictionaryChangedAction.Remove:
                        foreach ((int userId, var _) in args.OldItems!)
                        {
                            if (api.GetFriend(userId)?.TargetUser is APIUser user)
                            {
                                if (!onlineAlertQueue.Remove(user))
                                {
                                    offlineAlertQueue.Add(user);
                                    lastOfflineAlertTime ??= Time.Current;
                                }
                            }
                        }

                        break;
                }
            });
        }

        protected override void Update()
        {
            base.Update();

            alertOnlineUsers();
            alertOfflineUsers();
        }

        private void alertOnlineUsers()
        {
            if (onlineAlertQueue.Count == 0)
                return;

            if (lastOnlineAlertTime == null || Time.Current - lastOnlineAlertTime < 1000)
                return;

            APIUser? singleUser = onlineAlertQueue.Count == 1 ? onlineAlertQueue.Single() : null;

            notifications.Post(new SimpleNotification
            {
                Icon = FontAwesome.Solid.UserPlus,
                Text = $"Online: {string.Join(@", ", onlineAlertQueue.Select(u => u.Username))}",
                IconColour = colours.Green,
                Activated = () =>
                {
                    if (singleUser != null)
                    {
                        channelManager.OpenPrivateChannel(singleUser);
                        chatOverlay.Show();
                    }

                    return true;
                }
            });

            onlineAlertQueue.Clear();
            lastOnlineAlertTime = null;
        }

        private void alertOfflineUsers()
        {
            if (offlineAlertQueue.Count == 0)
                return;

            if (lastOfflineAlertTime == null || Time.Current - lastOfflineAlertTime < 1000)
                return;

            notifications.Post(new SimpleNotification
            {
                Icon = FontAwesome.Solid.UserMinus,
                Text = $"Offline: {string.Join(@", ", offlineAlertQueue.Select(u => u.Username))}",
                IconColour = colours.Red
            });

            offlineAlertQueue.Clear();
            lastOfflineAlertTime = null;
        }
    }
}
