// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osuTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Cursor;
using osu.Game.Online.Chat;
using osu.Game.Overlays.Notifications;
using osu.Game.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Colour;
using osu.Game.Online.API;
using osu.Game.Configuration;
using osu.Framework.Bindables;
using osu.Game.Users;

namespace osu.Game.Overlays.Chat
{
    public class DrawableChannel : Container
    {
        public readonly Channel Channel;
        protected ChatLineContainer ChatLineFlow;
        private OsuScrollContainer scroll;
        public ColourInfo HighlightColour { get; set; }

        [Resolved(CanBeNull = true)]
        private NotificationOverlay notificationOverlay { get; set; }

        [Resolved(CanBeNull = true)]
        private ChatOverlay chatOverlay { get; set; }

        [Resolved(CanBeNull = true)]
        private ChannelManager channelManager { get; set; }

        private Bindable<bool> notifyOnMention;
        private Bindable<bool> notifyOnChat;
        private Bindable<string> highlightWords;
        private Bindable<string> ignoreList;
        private Bindable<User> localUser;

        public DrawableChannel(Channel channel)
        {
            Channel = channel;
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours, OsuConfigManager config, IAPIProvider api)
        {
            notifyOnMention = config.GetBindable<bool>(OsuSetting.ChatHighlightName);
            notifyOnChat = config.GetBindable<bool>(OsuSetting.ChatMessageNotification);
            highlightWords = config.GetBindable<string>(OsuSetting.HighlightWords);
            ignoreList = config.GetBindable<string>(OsuSetting.IgnoreList);
            localUser = api.LocalUser;
            HighlightColour = colours.Blue;

            Child = new OsuContextMenuContainer
            {
                RelativeSizeAxes = Axes.Both,
                Masking = true,
                Child = scroll = new OsuScrollContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    // Some chat lines have effects that slightly protrude to the bottom,
                    // which we do not want to mask away, hence the padding.
                    Padding = new MarginPadding { Bottom = 5 },
                    Child = ChatLineFlow = new ChatLineContainer
                    {
                        Padding = new MarginPadding { Left = 20, Right = 20 },
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Vertical,
                    }
                },
            };

            newMessagesArrived(Channel.Messages);

            Channel.NewMessagesArrived += newMessagesArrived;
            Channel.MessageRemoved += messageRemoved;
            Channel.PendingMessageResolved += pendingMessageResolved;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            scrollToEnd();
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            Channel.NewMessagesArrived -= newMessagesArrived;
            Channel.MessageRemoved -= messageRemoved;
            Channel.PendingMessageResolved -= pendingMessageResolved;
        }

        protected virtual ChatLine CreateChatLine(Message m) => new ChatLine(m);

        private void newMessagesArrived(IEnumerable<Message> newMessages)
        {
            // Add up to last Channel.MAX_HISTORY messages
            var ignoredWords = getWords(ignoreList.Value);
            var displayMessages = newMessages.Where(m => hasCaseInsensitive(getWords(m.Content), ignoredWords) == null);
            displayMessages = displayMessages.Skip(Math.Max(0, newMessages.Count() - Channel.MaxHistory));

            ChatLineFlow.AddRange(displayMessages.Select(CreateChatLine));

            checkForMentions(displayMessages);

            if (scroll.IsScrolledToEnd(10) || !ChatLineFlow.Children.Any() || newMessages.Any(m => m is LocalMessage))
                scrollToEnd();

            var staleMessages = ChatLineFlow.Children.Where(c => c.LifetimeEnd == double.MaxValue).ToArray();
            int count = staleMessages.Length - Channel.MaxHistory;

            for (int i = 0; i < count; i++)
            {
                var d = staleMessages[i];
                if (!scroll.IsScrolledToEnd(10))
                    scroll.OffsetScrollPosition(-d.DrawHeight);
                d.Expire();
            }
        }

        private void checkForMentions(IEnumerable<Message> messages)
        {
            // only send notifications when chat overlay is **closed**
            if (chatOverlay?.IsPresent == true && channelManager?.CurrentChannel.Value == Channel)
                return;

            foreach (var message in messages)
            {
                var words = getWords(message.Content);
                var username = localUser.Value.Username;

                if (message.Sender.Username == username)
                    continue;

                if (notifyOnChat.Value && Channel.Type == ChannelType.PM)
                {
                    var notification = new MentionNotification(Channel, message.Sender.Username, () =>
                    {
                        channelManager.CurrentChannel.Value = Channel;
                        HighlightMessage(message);
                    }, true);

                    notificationOverlay?.Post(notification);
                    continue;
                }

                if (notifyOnMention.Value && anyCaseInsensitive(words, username))
                {
                    var notification = new MentionNotification(Channel, message.Sender.Username, () =>
                    {
                        channelManager.CurrentChannel.Value = Channel;
                        HighlightMessage(message);
                    }, false);

                    notificationOverlay?.Post(notification);
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(highlightWords.Value))
                {
                    var matchedWord = hasCaseInsensitive(words, getWords(highlightWords.Value));

                    if (matchedWord != null)
                    {
                        var notification = new MentionNotification(Channel, message.Sender.Username, matchedWord, () =>
                        {
                            channelManager.CurrentChannel.Value = Channel;
                            HighlightMessage(message);
                        });

                        notificationOverlay?.Post(notification);
                        continue;
                    }
                }
            }
        }

        private void pendingMessageResolved(Message existing, Message updated)
        {
            var found = ChatLineFlow.Children.LastOrDefault(c => c.Message == existing);

            if (found != null)
            {
                Trace.Assert(updated.Id.HasValue, "An updated message was returned with no ID.");

                ChatLineFlow.Remove(found);
                found.Message = updated;
                ChatLineFlow.Add(found);
            }
        }

        public void HighlightMessage(Message message)
        {
            var chatLine = findChatLine(message);
            scroll.ScrollTo(chatLine);
            chatLine.FlashColour(HighlightColour, 5000, Easing.InExpo);
        }

        private void messageRemoved(Message removed)
        {
            findChatLine(removed)?.FadeColour(Color4.Red, 400).FadeOut(600).Expire();
        }

        private ChatLine findChatLine(Message message) => ChatLineFlow.Children.FirstOrDefault(c => c.Message == message);

        private void scrollToEnd() => ScheduleAfterChildren(() => scroll.ScrollToEnd());

        private string[] getWords(string input) => input.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        /// <summary>
        /// Finds the first matching string/word in both <paramref name="x"/> and <paramref name="y"/> (case-insensitive)
        /// </summary>
        private string hasCaseInsensitive(IEnumerable<string> x, IEnumerable<string> y) => x.FirstOrDefault(x2 => anyCaseInsensitive(y, x2));

        private bool anyCaseInsensitive(IEnumerable<string> x, string y) => x.Any(x2 => x2.Equals(y, StringComparison.InvariantCultureIgnoreCase));

        protected class ChatLineContainer : FillFlowContainer<ChatLine>
        {
            protected override int Compare(Drawable x, Drawable y)
            {
                var xC = (ChatLine)x;
                var yC = (ChatLine)y;

                return xC.Message.CompareTo(yC.Message);
            }
        }

        private class MentionNotification : SimpleNotification
        {
            public MentionNotification(Channel channel, string username, Action onClick, bool isPm) : this(channel, onClick)
            {
                if (isPm)
                {
                    Icon = FontAwesome.Solid.Envelope;
                    Text = $"You received a private message from '{username}'. Click to read it!";
                }
                else
                {
                    Icon = FontAwesome.Solid.At;
                    Text = $"Your name was mentioned in chat by '{username}'. Click to find out why!";
                }
            }

            public MentionNotification(Channel channel, string highlighter, string word, Action onClick) : this(channel, onClick)
            {
                Icon = FontAwesome.Solid.Highlighter;
                Text = $"'{word}' was mentioned in chat by '{highlighter}'. Click to find out why!";
            }

            private MentionNotification(Channel channel, Action onClick)
            {
                Channel = channel;
                this.onClick = onClick;
            }

            private readonly Action onClick;

            public Channel Channel { get; }

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
