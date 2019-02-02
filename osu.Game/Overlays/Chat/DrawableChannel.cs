// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osuTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Cursor;
using osu.Game.Online.Chat;

namespace osu.Game.Overlays.Chat
{
    public class DrawableChannel : Container
    {
        public readonly Channel Channel;
        protected readonly ChatLineContainer ChatLineFlow;
        private readonly ScrollContainer scroll;

        public DrawableChannel(Channel channel)
        {
            Channel = channel;

            RelativeSizeAxes = Axes.Both;

            Children = new Drawable[]
            {
                scroll = new OsuScrollContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    // Some chat lines have effects that slightly protrude to the bottom,
                    // which we do not want to mask away, hence the padding.
                    Padding = new MarginPadding { Bottom = 5 },
                    Child = new OsuContextMenuContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Child = ChatLineFlow = new ChatLineContainer
                        {
                            Padding = new MarginPadding { Left = 20, Right = 20 },
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Direction = FillDirection.Vertical,
                        }
                    },
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            newMessagesArrived(Channel.Messages);

            Channel.NewMessagesArrived += newMessagesArrived;
            Channel.MessageRemoved += messageRemoved;
            Channel.PendingMessageResolved += pendingMessageResolved;

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
            var displayMessages = newMessages.Skip(Math.Max(0, newMessages.Count() - Channel.MaxHistory));

            ChatLineFlow.AddRange(displayMessages.Select(CreateChatLine));

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

        private void messageRemoved(Message removed)
        {
            ChatLineFlow.Children.FirstOrDefault(c => c.Message == removed)?.FadeColour(Color4.Red, 400).FadeOut(600).Expire();
        }

        private void scrollToEnd() => ScheduleAfterChildren(() => scroll.ScrollToEnd());

        protected class ChatLineContainer : FillFlowContainer<ChatLine>
        {
            protected override int Compare(Drawable x, Drawable y)
            {
                var xC = (ChatLine)x;
                var yC = (ChatLine)y;

                return xC.Message.CompareTo(yC.Message);
            }
        }
    }
}
