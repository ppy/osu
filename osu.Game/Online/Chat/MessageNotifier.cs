// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
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
        private Bindable<bool> notifyOnChat;
        private Bindable<string> highlightWords;
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
            highlightWords = config.GetBindable<string>(OsuSetting.HighlightWords);
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
            // don't show if the ChatOverlay and the target channel is visible.
            if (IsActive && channelManager.CurrentChannel.Value == channel)
                return;

            foreach (var message in messages)
            {
                // ignore messages that already have been read
                if (message.Id < channel.LastReadId)
                    return;

                var localUsername = localUser.Value.Username;

                if (message.Sender.Username == localUsername)
                    continue;

                var words = getWords(message.Content);

                void onClick()
                {
                    notificationOverlay.Hide();
                    chatOverlay.Show();
                    channelManager.CurrentChannel.Value = channel;
                }

                if (notifyOnChat.Value && channel.Type == ChannelType.PM)
                {
                    var existingNotification = privateMessageNotifications.FirstOrDefault(n => n.Username == message.Sender.Username);

                    if (existingNotification == null)
                    {
                        var notification = new PrivateMessageNotification(message.Sender.Username, onClick);
                        notificationOverlay?.Post(notification);
                        privateMessageNotifications.Add(notification);
                    }
                    else
                    {
                        existingNotification.MessageCount++;
                    }

                    continue;
                }

                if (notifyOnMention.Value && anyCaseInsensitive(words, localUsername))
                {
                    var notification = new MentionNotification(message.Sender.Username, onClick);
                    notificationOverlay?.Post(notification);

                    continue;
                }
            }
        }

        private static string[] getWords(string input) => input.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        /// <summary>
        /// Finds the first matching string/word in both <paramref name="x"/> and <paramref name="y"/> (case-insensitive)
        /// </summary>
        private static string hasCaseInsensitive(IEnumerable<string> x, IEnumerable<string> y) => x.FirstOrDefault(x2 => anyCaseInsensitive(y, x2));

        private static bool anyCaseInsensitive(IEnumerable<string> x, string y) => x.Any(x2 => x2.Equals(y, StringComparison.OrdinalIgnoreCase));



        public class PrivateMessageNotification : SimpleNotification
        {
            public PrivateMessageNotification(string username, Action onClick)
            {
                Icon = FontAwesome.Solid.Envelope;
                Username = username;
                MessageCount = 1;
                this.onClick = onClick;
            }

            private int messageCount;

            public int MessageCount
            {
                get => messageCount;
                set => Text = (messageCount = value) > 1 ? $"You received {messageCount} private messages from '{Username}'. Click to read it!" : $"You received a private message from '{Username}'. Click to read it!";
            }

            public string Username { get; set; }

            private readonly Action onClick;

            public override bool IsImportant => false;

            [BackgroundDependencyLoader]
            private void load(OsuColour colours, MessageNotifier notifier)
            {
                IconBackgound.Colour = colours.PurpleDark;
                Activated = delegate
                {
                    onClick?.Invoke();

                    if (notifier.privateMessageNotifications.Contains(this))
                        notifier.privateMessageNotifications.Remove(this);

                    return true;
                };
            }
        }

        public class MentionNotification : SimpleNotification
        {
            public MentionNotification(string username, Action onClick)
            {
                Icon = FontAwesome.Solid.At;
                Text = $"Your name was mentioned in chat by '{username}'. Click to find out why!";
                this.onClick = onClick;
            }

            private readonly Action onClick;

            public override bool IsImportant => false;

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                IconBackgound.Colour = colours.PurpleDark;
                Activated = delegate
                {
                    onClick?.Invoke();
                    return true;
                };
            }
        }
    }
}
