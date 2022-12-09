// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Cursor;
using osu.Game.Online.Chat;
using osuTK.Graphics;

namespace osu.Game.Overlays.Chat
{
    public partial class DrawableChannel : Container
    {
        public readonly Channel Channel;
        protected FillFlowContainer ChatLineFlow;
        private ChannelScrollContainer scroll;

        private bool scrollbarVisible = true;

        public bool ScrollbarVisible
        {
            set
            {
                if (scrollbarVisible == value) return;

                scrollbarVisible = value;
                if (scroll != null)
                    scroll.ScrollbarVisible = value;
            }
        }

        public DrawableChannel(Channel channel)
        {
            Channel = channel;
            RelativeSizeAxes = Axes.Both;
        }

        private Bindable<Message> highlightedMessage;

        [BackgroundDependencyLoader]
        private void load()
        {
            Child = new OsuContextMenuContainer
            {
                RelativeSizeAxes = Axes.Both,
                Masking = true,
                Child = scroll = new ChannelScrollContainer
                {
                    ScrollbarVisible = scrollbarVisible,
                    RelativeSizeAxes = Axes.Both,
                    // Some chat lines have effects that slightly protrude to the bottom,
                    // which we do not want to mask away, hence the padding.
                    Padding = new MarginPadding { Bottom = 5 },
                    Child = ChatLineFlow = new FillFlowContainer
                    {
                        Padding = new MarginPadding { Horizontal = 10 },
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

            highlightedMessage = Channel.HighlightedMessage.GetBoundCopy();
            highlightedMessage.BindValueChanged(_ => processMessageHighlighting(), true);
        }

        /// <summary>
        /// Processes any pending message in <see cref="highlightedMessage"/>.
        /// </summary>
        // ScheduleAfterChildren is for ensuring the scroll flow has updated with any new chat lines.
        private void processMessageHighlighting() => SchedulerAfterChildren.AddOnce(() =>
        {
            if (highlightedMessage.Value == null)
                return;

            var chatLine = chatLines.FirstOrDefault(c => c.Message.Equals(highlightedMessage.Value));
            if (chatLine == null)
                return;

            float center = scroll.GetChildPosInContent(chatLine, chatLine.DrawSize / 2) - scroll.DisplayableContent / 2;
            scroll.ScrollTo(Math.Clamp(center, 0, scroll.ScrollableExtent));
            chatLine.Highlight();

            highlightedMessage.Value = null;
        });

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            Channel.NewMessagesArrived -= newMessagesArrived;
            Channel.MessageRemoved -= messageRemoved;
            Channel.PendingMessageResolved -= pendingMessageResolved;
        }

        protected virtual ChatLine CreateChatLine(Message m) => new ChatLine(m);

        protected virtual DaySeparator CreateDaySeparator(DateTimeOffset time) => new DaySeparator(time);

        private void newMessagesArrived(IEnumerable<Message> newMessages) => Schedule(() =>
        {
            if (newMessages.Min(m => m.Id) < chatLines.Max(c => c.Message.Id))
            {
                // there is a case (on initial population) that we may receive past messages and need to reorder.
                // easiest way is to just combine messages and recreate drawables (less worrying about day separators etc.)
                newMessages = newMessages.Concat(chatLines.Select(c => c.Message)).OrderBy(m => m.Id).ToList();
                ChatLineFlow.Clear();
            }

            // Add up to last Channel.MAX_HISTORY messages
            var displayMessages = newMessages.Skip(Math.Max(0, newMessages.Count() - Channel.MAX_HISTORY));

            Message lastMessage = chatLines.LastOrDefault()?.Message;

            foreach (var message in displayMessages)
            {
                addDaySeparatorIfRequired(lastMessage, message);

                ChatLineFlow.Add(CreateChatLine(message));
                lastMessage = message;
            }

            var staleMessages = chatLines.Where(c => c.LifetimeEnd == double.MaxValue).ToArray();

            int count = staleMessages.Length - Channel.MAX_HISTORY;

            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                    expireAndAdjustScroll(staleMessages[i]);

                removeAdjacentDaySeparators();
            }

            // due to the scroll adjusts from old messages removal above, a scroll-to-end must be enforced,
            // to avoid making the container think the user has scrolled back up and unwantedly disable auto-scrolling.
            if (newMessages.Any(m => m is LocalMessage))
                scroll.ScrollToEnd();

            processMessageHighlighting();
        });

        private void pendingMessageResolved(Message existing, Message updated) => Schedule(() =>
        {
            var found = chatLines.LastOrDefault(c => c.Message == existing);

            if (found != null)
            {
                Trace.Assert(updated.Id.HasValue, "An updated message was returned with no ID.");

                ChatLineFlow.Remove(found, false);
                found.Message = updated;

                addDaySeparatorIfRequired(chatLines.LastOrDefault()?.Message, updated);
                ChatLineFlow.Add(found);
            }
        });

        private void addDaySeparatorIfRequired(Message lastMessage, Message message)
        {
            if (lastMessage == null || lastMessage.Timestamp.ToLocalTime().Date != message.Timestamp.ToLocalTime().Date)
            {
                // A day separator is displayed even if no messages are in the channel.
                // If there are no messages after it, the simplest way to ensure it is fresh is to remove it
                // and add a new one instead.
                if (ChatLineFlow.LastOrDefault() is DaySeparator ds)
                    ChatLineFlow.Remove(ds, true);

                ChatLineFlow.Add(CreateDaySeparator(message.Timestamp));

                removeAdjacentDaySeparators();
            }
        }

        private void removeAdjacentDaySeparators()
        {
            // remove all adjacent day separators after stale message removal
            for (int i = 0; i < ChatLineFlow.Count - 1; i++)
            {
                if (!(ChatLineFlow[i] is DaySeparator)) break;
                if (!(ChatLineFlow[i + 1] is DaySeparator)) break;

                expireAndAdjustScroll(ChatLineFlow[i]);
            }
        }

        private void expireAndAdjustScroll(Drawable d)
        {
            scroll.OffsetScrollPosition(-d.DrawHeight);
            d.Expire();
        }

        private void messageRemoved(Message removed) => Schedule(() =>
        {
            chatLines.FirstOrDefault(c => c.Message == removed)?.FadeColour(Color4.Red, 400).FadeOut(600).Expire();
        });

        private IEnumerable<ChatLine> chatLines => ChatLineFlow.Children.OfType<ChatLine>();
    }
}
