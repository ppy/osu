// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
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

        [Resolved]
        private OsuColour colours { get; set; }

        private Bindable<bool> notifyOnMention;
        private Bindable<bool> notifyOnChat;
        private Bindable<string> highlightWords;
        private Bindable<User> localUser;

        /// <summary>
        /// Determines if the user is able to see incoming messages.
        /// </summary>
        public bool IsActive => chatOverlay?.IsPresent == true;

        [BackgroundDependencyLoader]
        private void load(OsuColour colours, OsuConfigManager config, IAPIProvider api)
        {
            notifyOnMention = config.GetBindable<bool>(OsuSetting.ChatHighlightName);
            notifyOnChat = config.GetBindable<bool>(OsuSetting.ChatMessageNotification);
            highlightWords = config.GetBindable<string>(OsuSetting.HighlightWords);
            localUser = api.LocalUser;
        }

        public void HandleMessages(Channel channel, IEnumerable<Message> messages)
        {
            // don't show if visible or not visible
            if (IsActive && channelManager.CurrentChannel.Value == channel)
                return;

            var channelDrawable = chatOverlay.GetChannelDrawable(channel);
            if (channelDrawable == null)
                return;

            foreach (var message in messages)
            {
                var words = getWords(message.Content);
                var localUsername = localUser.Value.Username;

                if (message.Sender.Username == localUsername)
                    continue;

                void onClick()
                {
                    if (channelManager != null)
                        channelManager.CurrentChannel.Value = channel;

                    channelDrawable.ScrollToAndHighlightMessage(message);
                }

                if (notifyOnChat.Value && channel.Type == ChannelType.PM)
                {
                    var username = message.Sender.Username;
                    var existingNotification = notificationOverlay.Notifications.OfType<PrivateMessageNotification>().FirstOrDefault(n => n.Username == username);

                    if (existingNotification == null)
                    {
                        var notification = new PrivateMessageNotification(username, onClick);
                        notificationOverlay?.Post(notification);
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
                if (!string.IsNullOrWhiteSpace(highlightWords.Value))
                {
                    var matchedWord = hasCaseInsensitive(words, getWords(highlightWords.Value));

                    if (matchedWord != null)
                    {
                        var notification = new HighlightNotification(message.Sender.Username, matchedWord, onClick);
                        notificationOverlay?.Post(notification);
                    }
                }
            }
        }

        private static string[] getWords(string input) => input.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        /// <summary>
        /// Finds the first matching string/word in both <paramref name="x"/> and <paramref name="y"/> (case-insensitive)
        /// </summary>
        private static string hasCaseInsensitive(IEnumerable<string> x, IEnumerable<string> y) => x.FirstOrDefault(x2 => anyCaseInsensitive(y, x2));

        private static bool anyCaseInsensitive(IEnumerable<string> x, string y) => x.Any(x2 => x2.Equals(y, StringComparison.InvariantCultureIgnoreCase));

        private class HighlightNotification : SimpleNotification
        {
            public HighlightNotification(string highlighter, string word, Action onClick)
            {
                Icon = FontAwesome.Solid.Highlighter;
                Text = $"'{word}' was mentioned in chat by '{highlighter}'. Click to find out why!";
                this.onClick = onClick;
            }

            private readonly Action onClick;

            public override bool IsImportant => false;

            [BackgroundDependencyLoader]
            private void load(OsuColour colours, NotificationOverlay notificationOverlay, ChatOverlay chatOverlay)
            {
                IconBackgound.Colour = colours.PurpleDark;
                Activated = delegate
                {
                    notificationOverlay.Hide();
                    chatOverlay.Show();
                    onClick?.Invoke();

                    return true;
                };
            }
        }

        private class PrivateMessageNotification : SimpleNotification
        {
            public PrivateMessageNotification(string username, Action onClick)
            {
                Icon = FontAwesome.Solid.Envelope;
                Username = username;
                MessageCount = 1;
                this.onClick = onClick;
            }

            private int messageCount = 0;

            public int MessageCount
            {
                get => messageCount;
                set
                {
                    messageCount = value;
                    if (messageCount > 1)
                    {
                        Text = $"You received {messageCount} private messages from '{Username}'. Click to read it!";
                    }
                    else
                    {
                        Text = $"You received a private message from '{Username}'. Click to read it!";
                    }
                }
            }

            public string Username { get; set; }

            private readonly Action onClick;

            public override bool IsImportant => false;

            [BackgroundDependencyLoader]
            private void load(OsuColour colours, NotificationOverlay notificationOverlay, ChatOverlay chatOverlay)
            {
                IconBackgound.Colour = colours.PurpleDark;
                Activated = delegate
                {
                    notificationOverlay.Hide();
                    chatOverlay.Show();
                    onClick?.Invoke();

                    return true;
                };
            }
        }

        private class MentionNotification : SimpleNotification
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
            private void load(OsuColour colours, NotificationOverlay notificationOverlay, ChatOverlay chatOverlay)
            {
                IconBackgound.Colour = colours.PurpleDark;
                Activated = delegate
                {
                    notificationOverlay.Hide();
                    chatOverlay.Show();
                    onClick?.Invoke();

                    return true;
                };
            }
        }
    }
}
