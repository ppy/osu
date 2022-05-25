// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using System;
using System.Linq;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Chat;
using osu.Game.Overlays.Chat.Listing;

namespace osu.Game.Overlays.Chat.ChannelList
{
    public class ChannelList : Container
    {
        public Action<Channel>? OnRequestSelect;
        public Action<Channel>? OnRequestLeave;

        public IEnumerable<Channel> Channels => publicChannelFlow.Channels.Concat(privateChannelFlow.Channels);

        public readonly ChannelListing.ChannelListingChannel ChannelListingChannel = new ChannelListing.ChannelListingChannel();

        private readonly Dictionary<Channel, ChannelListItem> channelMap = new Dictionary<Channel, ChannelListItem>();

        private OsuScrollContainer scroll = null!;
        private ChannelListItemFlow publicChannelFlow = null!;
        private ChannelListItemFlow privateChannelFlow = null!;
        private ChannelListItem selector = null!;

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colourProvider.Background6,
                },
                scroll = new OsuScrollContainer
                {
                    Padding = new MarginPadding { Vertical = 7 },
                    RelativeSizeAxes = Axes.Both,
                    ScrollbarAnchor = Anchor.TopRight,
                    ScrollDistance = 35f,
                    Child = new FillFlowContainer
                    {
                        Direction = FillDirection.Vertical,
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Children = new Drawable[]
                        {
                            new ChannelListLabel("CHANNELS"),
                            publicChannelFlow = new ChannelListItemFlow(),
                            selector = new ChannelListItem(ChannelListingChannel)
                            {
                                Margin = new MarginPadding { Bottom = 10 },
                            },
                            new ChannelListLabel("DIRECT MESSAGES"),
                            privateChannelFlow = new ChannelListItemFlow(),
                        },
                    },
                },
            };

            selector.OnRequestSelect += chan => OnRequestSelect?.Invoke(chan);
        }

        public void AddChannel(Channel channel)
        {
            if (channelMap.ContainsKey(channel))
                return;

            ChannelListItem item = new ChannelListItem(channel);
            item.OnRequestSelect += chan => OnRequestSelect?.Invoke(chan);
            item.OnRequestLeave += chan => OnRequestLeave?.Invoke(chan);

            ChannelListItemFlow flow = getFlowForChannel(channel);
            channelMap.Add(channel, item);
            flow.Add(item);
        }

        public void RemoveChannel(Channel channel)
        {
            if (!channelMap.ContainsKey(channel))
                return;

            ChannelListItem item = channelMap[channel];
            ChannelListItemFlow flow = getFlowForChannel(channel);

            channelMap.Remove(channel);
            flow.Remove(item);
        }

        public ChannelListItem GetItem(Channel channel)
        {
            if (!channelMap.ContainsKey(channel))
                throw new ArgumentOutOfRangeException();

            return channelMap[channel];
        }

        public void ScrollChannelIntoView(Channel channel) => scroll.ScrollIntoView(GetItem(channel));

        private ChannelListItemFlow getFlowForChannel(Channel channel)
        {
            switch (channel.Type)
            {
                case ChannelType.Public:
                    return publicChannelFlow;

                case ChannelType.PM:
                    return privateChannelFlow;

                default:
                    return publicChannelFlow;
            }
        }

        private class ChannelListLabel : OsuSpriteText
        {
            public ChannelListLabel(string label)
            {
                Text = label;
                Margin = new MarginPadding { Left = 18, Bottom = 5 };
                Font = OsuFont.Torus.With(size: 12, weight: FontWeight.SemiBold);
            }
        }

        private class ChannelListItemFlow : FillFlowContainer<ChannelListItem>
        {
            public IEnumerable<Channel> Channels => Children.Select(c => c.Channel);

            public ChannelListItemFlow()
            {
                Direction = FillDirection.Vertical;
                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;
            }
        }
    }
}
