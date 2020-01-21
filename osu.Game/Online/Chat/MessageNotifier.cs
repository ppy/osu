// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Humanizer;
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
        private Bindable<bool> notifyOnChat;
        private Bindable<User> localUser;

        /// <summary>
        /// Determines if the user is able to see incoming messages.
        /// </summary>
        public bool IsActive => chatOverlay?.IsPresent == true;

        private readonly List<PrivateMessageNotification> privateMessageNotifications = new List<PrivateMessageNotification>();

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config, IAPIProvider api)
        {
            notifyOnMention = config.GetBindable<bool>(OsuSetting.ChatHighlightName);
            notifyOnChat = config.GetBindable<bool>(OsuSetting.ChatMessageNotification);
            localUser = api.LocalUser;

            // Listen for new messages
            channelManager.JoinedChannels.ItemsAdded += joinedChannels =>
            {
                foreach (var channel in joinedChannels)
                    channel.NewMessagesArrived += channel_NewMessagesArrived;
            };

            channelManager.JoinedChannels.ItemsRemoved += leftChannels =>
            {
                foreach (var channel in leftChannels)
                    channel.NewMessagesArrived -= channel_NewMessagesArrived;
            };
        }

        private void channel_NewMessagesArrived(IEnumerable<Message> messages)
        {
            if (messages == null || !messages.Any())
                return;

            HandleMessages(messages.First().ChannelId, messages);
        }

        /// <remarks>
        /// Resolves the channel id
        /// </remarks>
        public void HandleMessages(long channelId, IEnumerable<Message> messages)
        {
            var channel = channelManager.JoinedChannels.FirstOrDefault(c => c.Id == channelId);

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
            if (IsActive && channelManager.CurrentChannel.Value == channel)
                return;

            foreach (var message in messages.OrderByDescending(m => m.Id))
            {
                // ignore messages that already have been read
                if (message.Id < channel.LastReadId)
                    return;

                if (message.Sender.Id == localUser.Value.Id)
                    continue;

                if (checkForPMs(channel, message))
                    continue;

                // change output to bool again if another "message processor" is added.
                checkForMentions(channel, message, localUser.Value.Username);
            }
        }

        private bool checkForPMs(Channel channel, Message message)
        {
            if (!notifyOnChat.Value || channel.Type != ChannelType.PM)
                return false;

            var existingNotification = privateMessageNotifications.FirstOrDefault(n => n.Username == message.Sender.Username);

            if (existingNotification == null)
            {
                var notification = new PrivateMessageNotification(message.Sender.Username, channel);
                notificationOverlay?.Post(notification);
                privateMessageNotifications.Add(notification);
            }
            else
            {
                existingNotification.MessageCount++;
            }

            return true;
        }

        private void checkForMentions(Channel channel, Message message, string username)
        {
            if (!notifyOnMention.Value || !anyCaseInsensitive(getWords(message.Content), username))
                return;

            var notification = new MentionNotification(message.Sender.Username, channel);
            notificationOverlay?.Post(notification);
        }

        private static IEnumerable<string> getWords(string input) => Regex.Matches(input, @"\w+").Select(c => c.Value);

        /// <summary>
        /// Finds the first matching string/word in both <paramref name="x"/> and <paramref name="y"/> (case-insensitive)
        /// </summary>
        private static string hasCaseInsensitive(IEnumerable<string> x, IEnumerable<string> y) => x.FirstOrDefault(x2 => anyCaseInsensitive(y, x2));

        private static bool anyCaseInsensitive(IEnumerable<string> x, string y) => x.Any(x2 => x2.Equals(y, StringComparison.OrdinalIgnoreCase));

        public class PrivateMessageNotification : SimpleNotification
        {
            public PrivateMessageNotification(string username, Channel channel)
            {
                Icon = FontAwesome.Solid.Envelope;
                Username = username;
                MessageCount = 1;
                Channel = channel;
            }

            private int messageCount;

            public int MessageCount
            {
                get => messageCount;
                set
                {
                    messageCount = value;
                    Text = $"You received {"private message".ToQuantity(messageCount)} from '{Username}'. Click to read it!";
                }
            }

            public string Username { get; set; }

            public Channel Channel { get; set; }

            public override bool IsImportant => false;

            [BackgroundDependencyLoader]
            private void load(OsuColour colours, ChatOverlay chatOverlay, NotificationOverlay notificationOverlay, ChannelManager channelManager, MessageNotifier notifier)
            {
                IconBackgound.Colour = colours.PurpleDark;
                Activated = delegate
                {
                    notificationOverlay.Hide();
                    chatOverlay.Show();
                    channelManager.CurrentChannel.Value = Channel;

                    if (notifier.privateMessageNotifications.Contains(this))
                        notifier.privateMessageNotifications.Remove(this);

                    return true;
                };
            }
        }

        public class MentionNotification : SimpleNotification
        {
            public MentionNotification(string username, Channel channel)
            {
                Icon = FontAwesome.Solid.At;
                Text = $"Your name was mentioned in chat by '{username}'. Click to find out why!";
                Channel = channel;
            }

            public Channel Channel { get; set; }

            public override bool IsImportant => false;

            [BackgroundDependencyLoader]
            private void load(OsuColour colours, ChatOverlay chatOverlay, NotificationOverlay notificationOverlay, ChannelManager channelManager)
            {
                IconBackgound.Colour = colours.PurpleDark;
                Activated = delegate
                {
                    notificationOverlay.Hide();
                    chatOverlay.Show();
                    channelManager.CurrentChannel.Value = Channel;

                    return true;
                };
            }
        }
    }
}
