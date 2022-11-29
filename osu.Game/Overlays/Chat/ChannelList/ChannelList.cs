// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Chat;
using osu.Game.Overlays.Chat.Listing;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Overlays.Chat.ChannelList
{
    public partial class ChannelList : Container
    {
        public Action<Channel>? OnRequestSelect;
        public Action<Channel>? OnRequestLeave;

        public IEnumerable<Channel> Channels => groupFlow.Children
                                                         .OfType<ChannelGroup>()
                                                         .SelectMany(channelGroup => channelGroup.ItemFlow)
                                                         .Select(item => item.Channel);

        public readonly ChannelListing.ChannelListingChannel ChannelListingChannel = new ChannelListing.ChannelListingChannel();

        private readonly Dictionary<Channel, ChannelListItem> channelMap = new Dictionary<Channel, ChannelListItem>();

        private OsuScrollContainer scroll = null!;
        private FillFlowContainer groupFlow = null!;
        private ChannelGroup announceChannelGroup = null!;
        private ChannelGroup publicChannelGroup = null!;
        private ChannelGroup privateChannelGroup = null!;
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
                    RelativeSizeAxes = Axes.Both,
                    ScrollbarAnchor = Anchor.TopRight,
                    ScrollDistance = 35f,
                    Child = groupFlow = new FillFlowContainer
                    {
                        Direction = FillDirection.Vertical,
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Children = new Drawable[]
                        {
                            announceChannelGroup = new ChannelGroup(ChatStrings.ChannelsListTitleANNOUNCE.ToUpper()),
                            publicChannelGroup = new ChannelGroup(ChatStrings.ChannelsListTitlePUBLIC.ToUpper()),
                            selector = new ChannelListItem(ChannelListingChannel),
                            privateChannelGroup = new ChannelGroup(ChatStrings.ChannelsListTitlePM.ToUpper()),
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

            FillFlowContainer<ChannelListItem> flow = getFlowForChannel(channel);
            channelMap.Add(channel, item);
            flow.Add(item);

            updateVisibility();
        }

        public void RemoveChannel(Channel channel)
        {
            if (!channelMap.ContainsKey(channel))
                return;

            ChannelListItem item = channelMap[channel];
            FillFlowContainer<ChannelListItem> flow = getFlowForChannel(channel);

            channelMap.Remove(channel);
            flow.Remove(item, true);

            updateVisibility();
        }

        public ChannelListItem GetItem(Channel channel)
        {
            if (!channelMap.ContainsKey(channel))
                throw new ArgumentOutOfRangeException();

            return channelMap[channel];
        }

        public void ScrollChannelIntoView(Channel channel) => scroll.ScrollIntoView(GetItem(channel));

        private FillFlowContainer<ChannelListItem> getFlowForChannel(Channel channel)
        {
            switch (channel.Type)
            {
                case ChannelType.Public:
                    return publicChannelGroup.ItemFlow;

                case ChannelType.PM:
                    return privateChannelGroup.ItemFlow;

                case ChannelType.Announce:
                    return announceChannelGroup.ItemFlow;

                default:
                    return publicChannelGroup.ItemFlow;
            }
        }

        private void updateVisibility()
        {
            if (announceChannelGroup.ItemFlow.Children.Count == 0)
                announceChannelGroup.Hide();
            else
                announceChannelGroup.Show();
        }

        private partial class ChannelGroup : FillFlowContainer
        {
            public readonly FillFlowContainer<ChannelListItem> ItemFlow;

            public ChannelGroup(LocalisableString label)
            {
                Direction = FillDirection.Vertical;
                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;
                Padding = new MarginPadding { Top = 8 };

                Children = new Drawable[]
                {
                    new OsuSpriteText
                    {
                        Text = label,
                        Margin = new MarginPadding { Left = 18, Bottom = 5 },
                        Font = OsuFont.Torus.With(size: 12, weight: FontWeight.SemiBold),
                    },
                    ItemFlow = new FillFlowContainer<ChannelListItem>
                    {
                        Direction = FillDirection.Vertical,
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                    },
                };
            }
        }
    }
}
