// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Cursor;
using osu.Game.Online.Chat;
using osu.Game.Overlays.Notifications;
using osu.Game.Graphics;
using osu.Game.Online.API;
using osu.Game.Configuration;
using osu.Game.Users;
using osu.Game.Graphics.Sprites;
using osu.Framework.Graphics;
using osuTK.Graphics;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Shapes;

namespace osu.Game.Overlays.Chat
{
    public class DrawableChannel : Container
    {
        public readonly Channel Channel;
        protected FillFlowContainer ChatLineFlow;
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

        [Resolved]
        private OsuColour colours { get; set; }

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
                    Child = ChatLineFlow = new FillFlowContainer
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

        protected virtual DaySeparator CreateDaySeparator(DateTimeOffset time) => new DaySeparator(time)
        {
            Margin = new MarginPadding { Vertical = 10 },
            Colour = colours.ChatBlue.Lighten(0.7f),
        };

        private void newMessagesArrived(IEnumerable<Message> newMessages)
        {
            bool shouldScrollToEnd = scroll.IsScrolledToEnd(10) || !chatLines.Any() || newMessages.Any(m => m is LocalMessage);

            // Add up to last Channel.MAX_HISTORY messages
            var ignoredWords = getWords(ignoreList.Value);
            var displayMessages = newMessages.Where(m => hasCaseInsensitive(getWords(m.Content), ignoredWords) == null);
            displayMessages = displayMessages.Skip(Math.Max(0, newMessages.Count() - Channel.MAX_HISTORY));

            Message lastMessage = chatLines.LastOrDefault()?.Message;

            checkForMentions(displayMessages);

            foreach (var message in displayMessages)
            {
                if (lastMessage == null || lastMessage.Timestamp.ToLocalTime().Date != message.Timestamp.ToLocalTime().Date)
                    ChatLineFlow.Add(CreateDaySeparator(message.Timestamp));

                ChatLineFlow.Add(CreateChatLine(message));
                lastMessage = message;
            }

            var staleMessages = chatLines.Where(c => c.LifetimeEnd == double.MaxValue).ToArray();
            int count = staleMessages.Length - Channel.MAX_HISTORY;

            if (count > 0)
            {
                void expireAndAdjustScroll(Drawable d)
                {
                    scroll.OffsetScrollPosition(-d.DrawHeight);
                    d.Expire();
                }

                for (int i = 0; i < count; i++)
                    expireAndAdjustScroll(staleMessages[i]);

                // remove all adjacent day separators after stale message removal
                for (int i = 0; i < ChatLineFlow.Count - 1; i++)
                {
                    if (!(ChatLineFlow[i] is DaySeparator)) break;
                    if (!(ChatLineFlow[i + 1] is DaySeparator)) break;

                    expireAndAdjustScroll(ChatLineFlow[i]);
                }
            }

            if (shouldScrollToEnd)
                scrollToEnd();
        }

        private void checkForMentions(IEnumerable<Message> messages)
        {
            // only send notifications when the chat overlay is **closed** and the channel is not visible.
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
                    var notification = new PrivateMessageNotification(message.Sender.Username, () =>
                    {
                        channelManager.CurrentChannel.Value = Channel;
                        ScrollToAndHighlightMessage(message);
                    });

                    notificationOverlay?.Post(notification);
                    continue;
                }

                if (notifyOnMention.Value && anyCaseInsensitive(words, username))
                {
                    var notification = new MentionNotification(message.Sender.Username, () =>
                    {
                        channelManager.CurrentChannel.Value = Channel;
                        ScrollToAndHighlightMessage(message);
                    });

                    notificationOverlay?.Post(notification);
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(highlightWords.Value))
                {
                    var matchedWord = hasCaseInsensitive(words, getWords(highlightWords.Value));

                    if (matchedWord != null)
                    {
                        var notification = new HighlightNotification(message.Sender.Username, matchedWord, () =>
                        {
                            channelManager.CurrentChannel.Value = Channel;
                            ScrollToAndHighlightMessage(message);
                        });

                        notificationOverlay?.Post(notification);
                    }
                }
            }
        }

        private void pendingMessageResolved(Message existing, Message updated)
        {
            var found = chatLines.LastOrDefault(c => c.Message == existing);

            if (found != null)
            {
                Trace.Assert(updated.Id.HasValue, "An updated message was returned with no ID.");

                ChatLineFlow.Remove(found);
                found.Message = updated;
                ChatLineFlow.Add(found);
            }
        }

        public void ScrollToAndHighlightMessage(Message message)
        {
            var chatLine = findChatLine(message);
            scroll.ScrollTo(chatLine);
            chatLine.FlashColour(HighlightColour, 5000, Easing.InExpo);
        }

        private void messageRemoved(Message removed)
        {
            chatLines.FirstOrDefault(c => c.Message == removed)?.FadeColour(Color4.Red, 400).FadeOut(600).Expire();
        }

        private IEnumerable<ChatLine> chatLines => ChatLineFlow.Children.OfType<ChatLine>();

        private void scrollToEnd() => ScheduleAfterChildren(() => scroll.ScrollToEnd());

        private string[] getWords(string input) => input.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        /// <summary>
        /// Finds the first matching string/word in both <paramref name="x"/> and <paramref name="y"/> (case-insensitive)
        /// </summary>
        private string hasCaseInsensitive(IEnumerable<string> x, IEnumerable<string> y) => x.FirstOrDefault(x2 => anyCaseInsensitive(y, x2));

        private bool anyCaseInsensitive(IEnumerable<string> x, string y) => x.Any(x2 => x2.Equals(y, StringComparison.InvariantCultureIgnoreCase));

        private ChatLine findChatLine(Message message) => chatLines.FirstOrDefault(c => c.Message == message);

        public class DaySeparator : Container
        {
            public float TextSize
            {
                get => text.Font.Size;
                set => text.Font = text.Font.With(size: value);
            }

            private float lineHeight = 2;

            public float LineHeight
            {
                get => lineHeight;
                set => lineHeight = leftBox.Height = rightBox.Height = value;
            }

            private readonly SpriteText text;
            private readonly Box leftBox;
            private readonly Box rightBox;

            public DaySeparator(DateTimeOffset time)
            {
                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;
                Child = new GridContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    ColumnDimensions = new[]
                    {
                        new Dimension(),
                        new Dimension(GridSizeMode.AutoSize),
                        new Dimension(),
                    },
                    RowDimensions = new[] { new Dimension(GridSizeMode.AutoSize), },
                    Content = new[]
                    {
                        new Drawable[]
                        {
                            leftBox = new Box
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                RelativeSizeAxes = Axes.X,
                                Height = lineHeight,
                            },
                            text = new OsuSpriteText
                            {
                                Margin = new MarginPadding { Horizontal = 10 },
                                Text = time.ToLocalTime().ToString("dd MMM yyyy"),
                            },
                            rightBox = new Box
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                RelativeSizeAxes = Axes.X,
                                Height = lineHeight,
                            },
                        }
                    }
                };
            }
        }

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
                Text = $"You received a private message from '{username}'. Click to read it!";
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
