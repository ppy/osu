// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Containers;
using osu.Game.Online.Chat;

namespace osu.Game.Overlays.Chat
{
    public class DrawableChannel : Container
    {
        public readonly Channel Channel;
        private readonly FillFlowContainer<ChatLine> flow;
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
                    Children = new Drawable[]
                    {
                        flow = new FillFlowContainer<ChatLine>
                        {
                            Direction = FillDirection.Vertical,
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Padding = new MarginPadding { Left = 20, Right = 20 }
                        }
                    }
                }
            };

            channel.NewMessagesArrived += newMessagesArrived;
            channel.LocalEchoMessagesRemoved += localEchoMessagesRemoved;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            newMessagesArrived(Channel.Messages);
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
            Channel.LocalEchoMessagesRemoved -= localEchoMessagesRemoved;
        }

        private void newMessagesArrived(IEnumerable<Message> newMessages)
        {
            // Add up to last Channel.MAX_HISTORY messages
            var displayMessages = newMessages.Skip(Math.Max(0, newMessages.Count() - Channel.MAX_HISTORY));

            foreach (Message receivedMessage in displayMessages)
            {
                // Get the received message's sent message tuple if there is one
                Tuple<LocalEchoMessage, Message> sentMessageTuple = Channel.SentMessages.Find(tuple => receivedMessage.Equals(tuple.Item2));

                // sentMessageTuple is  unequal to null if the received message is a handled sent message. In this case replace the old chat line's message.
                if (sentMessageTuple != null)
                    flow.Children.Single(chatLine => chatLine.Message == sentMessageTuple.Item1).Message = sentMessageTuple.Item2;
                else
                    flow.Add(new ChatLine(receivedMessage));
            }

            if (!IsLoaded) return;

            if (scroll.IsScrolledToEnd(10) || !flow.Children.Any())
                scrollToEnd();

            var staleMessages = flow.Children.Where(c => c.LifetimeEnd == double.MaxValue).ToArray();
            int count = staleMessages.Length - Channel.MAX_HISTORY;

            for (int i = 0; i < count; i++)
            {
                var d = staleMessages[i];
                if (!scroll.IsScrolledToEnd(10))
                    scroll.OffsetScrollPosition(-d.DrawHeight);
                d.Expire();
            }

            ensureOrder();
        }

        private void localEchoMessagesRemoved(IEnumerable<Message> RemovedLocalEchoMessages)
        {
            foreach (ChatLine chatLine in flow.Children.Where(child => RemovedLocalEchoMessages.Contains(child.Message)))
                chatLine.FadeColour(Color4.Red, 400).FadeOut(600).Expire();
        }

        private void ensureOrder()
        {
            ChatLine[] childrenArray = flow.Children.ToArray();

            foreach (ChatLine chatLine in childrenArray)
                flow.ChangeChildDepth(chatLine, -Channel.Messages.IndexOf(chatLine.Message));
        }

        private void scrollToEnd() => ScheduleAfterChildren(() => scroll.ScrollToEnd());
    }
}
