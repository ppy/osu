// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.Chat;

namespace osu.Game.Overlays.Chat
{
    public class DrawableChannel : Container
    {
        public readonly Channel Channel;
        private readonly FillFlowContainer flow;
        private readonly ScrollContainer scroll;

        public DrawableChannel(Channel channel)
        {
            Channel = channel;

            RelativeSizeAxes = Axes.Both;

            Children = new Drawable[]
            {
                scroll = new ScrollContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        flow = new FillFlowContainer
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
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            newMessagesArrived(Channel.Messages);
            scrollToEnd();
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            Channel.NewMessagesArrived -= newMessagesArrived;
        }

        private void newMessagesArrived(IEnumerable<Message> newMessages)
        {
            if (!IsLoaded) return;

            var displayMessages = newMessages.Skip(Math.Max(0, newMessages.Count() - Channel.MAX_HISTORY));

            if (scroll.IsScrolledToEnd(10) || !flow.Children.Any())
                scrollToEnd();

            //up to last Channel.MAX_HISTORY messages
            foreach (Message m in displayMessages)
            {
                var d = new ChatLine(m);
                flow.Add(d);
            }

            while (flow.Children.Count(c => c.LifetimeEnd == double.MaxValue) > Channel.MAX_HISTORY)
            {
                var d = flow.Children.First(c => c.LifetimeEnd == double.MaxValue);
                if (!scroll.IsScrolledToEnd(10))
                    scroll.OffsetScrollPosition(-d.DrawHeight);
                d.Expire();
            }
        }

        private void scrollToEnd() => Scheduler.AddDelayed(() => scroll.ScrollToEnd(), 50);
    }
}
