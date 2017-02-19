// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics.Sprites;
using OpenTK;

namespace osu.Game.Online.Chat.Drawables
{
    public class DrawableChannel : Container
    {
        private readonly Channel channel;
        private FlowContainer flow;

        public DrawableChannel(Channel channel)
        {
            this.channel = channel;
            newMessagesArrived(channel.Messages);
            channel.NewMessagesArrived += newMessagesArrived;

            RelativeSizeAxes = Axes.Both;

            Children = new Drawable[]
            {
                new OsuSpriteText
                {
                    Text = channel.Name,
                    TextSize = 50,
                    Alpha = 0.3f,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre
                },
                new ScrollContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        flow = new FlowContainer
                        {
                            Direction = FlowDirections.Vertical,
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Spacing = new Vector2(1, 1)
                        }
                    }
                }
            };
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            channel.NewMessagesArrived -= newMessagesArrived;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            newMessagesArrived(channel.Messages);
        }

        private void newMessagesArrived(IEnumerable<Message> newMessages)
        {
            if (!IsLoaded) return;

            var displayMessages = newMessages.Skip(Math.Max(0, newMessages.Count() - Channel.MAX_HISTORY));

            //up to last Channel.MAX_HISTORY messages
            foreach (Message m in displayMessages)
                flow.Add(new ChatLine(m));

            while (flow.Children.Count() > Channel.MAX_HISTORY)
                flow.Remove(flow.Children.First());
        }
    }
}