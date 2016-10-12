//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Transformations;
using osu.Game.Online.Chat.Display.osu.Online.Social;
using OpenTK;

namespace osu.Game.Online.Chat.Display
{
    public class ChannelDisplay : FlowContainer
    {
        private readonly Channel channel;
        private FlowContainer flow;

        public ChannelDisplay(Channel channel)
        {
            this.channel = channel;
            newMessages(channel.Messages);
            channel.NewMessagesArrived += newMessages;

            RelativeSizeAxes = Axes.Both;
            Direction = FlowDirection.VerticalOnly;

            Children = new Drawable[]
            {
                new SpriteText
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
                            Direction = FlowDirection.VerticalOnly,
                            RelativeSizeAxes = Axes.X,
                            LayoutEasing = EasingTypes.Out,
                            Spacing = new Vector2(1, 1)
                        }
                    }
                }
            };
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            channel.NewMessagesArrived -= newMessages;
        }

        public override void Load()
        {
            base.Load();
            newMessages(channel.Messages);
        }

        private void newMessages(IEnumerable<Message> newMessages)
        {
            if (!IsLoaded) return;

            var displayMessages = newMessages.Skip(Math.Max(0, newMessages.Count() - 20));

            //up to last 20 messages
            foreach (Message m in displayMessages)
                flow.Add(new ChatLine(m));

            while (flow.Children.Count() > 20)
                flow.Remove(flow.Children.First());
        }
    }
}