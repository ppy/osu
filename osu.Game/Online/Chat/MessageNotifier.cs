// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Logging;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Online.API;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;
using osu.Game.Users;

namespace osu.Game.Online.Chat
{
    /// <summary>
    /// Component that handles creating and posting notifications for incoming messages.
    /// </summary>
    public class MessageNotifier : Component
    {
        [Resolved(CanBeNull = true)]
        private NotificationOverlay notificationOverlay { get; set; }

        [Resolved(CanBeNull = true)]
        private ChatOverlay chatOverlay { get; set; }

        [Resolved(CanBeNull = true)]
        private ChannelManager channelManager { get; set; }

        private Bindable<bool> notifyOnMention;
        private Bindable<bool> notifyOnPM;
        private IBindable<User> localUser = new Bindable<User>();
        private readonly BindableList<Channel> joinedChannels = new BindableList<Channel>();

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config, IAPIProvider api)
        {
            notifyOnMention = config.GetBindable<bool>(OsuSetting.ChatHighlightName);
            notifyOnPM = config.GetBindable<bool>(OsuSetting.ChatMessageNotification);
            api.LocalUser.BindTo(localUser);

            // Listen for new messages
            joinedChannels.CollectionChanged += channelsChanged;

            channelManager.JoinedChannels.BindTo(joinedChannels);
        }

        private void channelsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var channel in e.NewItems.Cast<Channel>())
                        channel.NewMessagesArrived += newMessagesArrived;

                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (var channel in e.OldItems.Cast<Channel>())
                        channel.NewMessagesArrived -= newMessagesArrived;

                    break;
            }
        }

        private void newMessagesArrived(IEnumerable<Message> messages)
        {
            if (messages == null || !messages.Any())
                return;

            HandleMessages(messages.First().ChannelId, messages);
        }

        public void HandleMessages(long channelId, IEnumerable<Message> messages)
        {
            var channel = channelManager.JoinedChannels.SingleOrDefault(c => c.Id == channelId);

            if (channel == null)
            {
                Logger.Log($"Couldn't resolve channel id {channelId}", LoggingTarget.Information);
                return;
            }

            HandleMessages(channel, messages);
        }

        public void HandleMessages(Channel channel, IEnumerable<Message> messages)
        {
            // Only send notifications, if ChatOverlay and the target channel aren't visible.
            if (chatOverlay?.IsPresent == true && channelManager.CurrentChannel.Value == channel)
                return;

            foreach (var message in messages.OrderByDescending(m => m.Id))
            {
                // ignore messages that already have been read
                if (message.Id <= channel.LastReadId)
                    return;

                if (message.Sender.Id == localUser.Value.Id)
                    continue;

                // check for private messages first,
                // to avoid both posting two notifications about the same message
                if (checkForPMs(channel, message))
                    continue;

                // change output to bool again if another "message processor" is added.
                checkForMentions(channel, message, localUser.Value.Username);
            }
        }

        private bool checkForPMs(Channel channel, Message message)
        {
            if (!notifyOnPM.Value || channel.Type != ChannelType.PM)
                return false;

            var notification = new PrivateMessageNotification(message.Sender.Username, channel);

            notificationOverlay?.Post(notification);

            return true;
        }

        private void checkForMentions(Channel channel, Message message, string username)
        {
            if (!notifyOnMention.Value || !isMentioning(message.Content, username))
                return;

            var notification = new MentionNotification(message.Sender.Username, channel);
            notificationOverlay?.Post(notification);
        }

        /// <summary>
        /// Checks if <paramref name="message"/> contains <paramref name="username"/>, if not, retries making spaces into underscores.
        /// </summary>
        /// <returns>If the <paramref name="message"/> mentions the <paramref name="username"/></returns>
        private static bool isMentioning(string message, string username) => message.Contains(username, StringComparison.OrdinalIgnoreCase) || message.Contains(username.Replace(' ', '_'), StringComparison.OrdinalIgnoreCase);

        public class OpenChannelNotification : SimpleNotification
        {
            public OpenChannelNotification(Channel channel)
            {
                this.channel = channel;
            }

            private readonly Channel channel;

            public override bool IsImportant => false;

            [BackgroundDependencyLoader]
            private void load(OsuColour colours, ChatOverlay chatOverlay, NotificationOverlay notificationOverlay, ChannelManager channelManager)
            {
                IconBackgound.Colour = colours.PurpleDark;

                Activated = delegate
                {
                    notificationOverlay.Hide();
                    chatOverlay.Show();
                    channelManager.CurrentChannel.Value = channel;

                    return true;
                };
            }
        }

        public class PrivateMessageNotification : OpenChannelNotification
        {
            public PrivateMessageNotification(string username, Channel channel) : base(channel)
            {
                Icon = FontAwesome.Solid.Envelope;
                Text = $"You received a private message from '{username}'. Click to read it!";
            }
        }

        public class MentionNotification : OpenChannelNotification
        {
            public MentionNotification(string username, Channel channel) : base(channel)
            {
                Icon = FontAwesome.Solid.At;
                Text = $"Your name was mentioned in chat by '{username}'. Click to find out why!";
            }
        }
    }
}
