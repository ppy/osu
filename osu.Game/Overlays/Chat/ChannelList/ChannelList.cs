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
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Localisation;
using osu.Framework.Testing;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.Chat;
using osu.Game.Overlays.Chat.Listing;
using osu.Game.Resources.Localisation.Web;
using osuTK;

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

        public ChannelGroup AnnounceChannelGroup { get; private set; } = null!;
        public ChannelGroup PublicChannelGroup { get; private set; } = null!;
        public ChannelGroup PrivateChannelGroup { get; private set; } = null!;

        private OsuScrollContainer scroll = null!;
        private SearchContainer groupFlow = null!;

        private ChannelListItem selector = null!;
        private TextBox searchTextBox = null!;

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
                    Child = groupFlow = new SearchContainer
                    {
                        Direction = FillDirection.Vertical,
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Children = new Drawable[]
                        {
                            new Container
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Padding = new MarginPadding { Horizontal = 10, Top = 8 },
                                Child = searchTextBox = new ChannelSearchTextBox
                                {
                                    RelativeSizeAxes = Axes.X,
                                }
                            },
                            AnnounceChannelGroup = new ChannelGroup(ChatStrings.ChannelsListTitleANNOUNCE.ToUpper(), false),
                            PublicChannelGroup = new ChannelGroup(ChatStrings.ChannelsListTitlePUBLIC.ToUpper(), false),
                            selector = new ChannelListItem(ChannelListingChannel),
                            PrivateChannelGroup = new ChannelGroup(ChatStrings.ChannelsListTitlePM.ToUpper(), true),
                        },
                    },
                },
            };

            searchTextBox.Current.BindValueChanged(_ => groupFlow.SearchTerm = searchTextBox.Current.Value, true);
            searchTextBox.OnCommit += (_, _) =>
            {
                if (string.IsNullOrEmpty(searchTextBox.Current.Value))
                    return;

                var firstMatchingItem = this.ChildrenOfType<ChannelListItem>().FirstOrDefault(item => item.MatchingFilter);
                if (firstMatchingItem == null)
                    return;

                OnRequestSelect?.Invoke(firstMatchingItem.Channel);
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

            ChannelGroup group = getGroupFromChannel(channel);
            channelMap.Add(channel, item);
            group.AddChannel(item);

            updateVisibility();
        }

        public void RemoveChannel(Channel channel)
        {
            if (!channelMap.TryGetValue(channel, out var item))
                return;

            ChannelGroup group = getGroupFromChannel(channel);

            channelMap.Remove(channel);
            group.RemoveChannel(item);

            updateVisibility();
        }

        public ChannelListItem GetItem(Channel channel)
        {
            if (!channelMap.TryGetValue(channel, out var item))
                throw new ArgumentOutOfRangeException();

            return item;
        }

        public void ScrollChannelIntoView(Channel channel) => scroll.ScrollIntoView(GetItem(channel));

        private ChannelGroup getGroupFromChannel(Channel channel)
        {
            switch (channel.Type)
            {
                case ChannelType.Public:
                    return PublicChannelGroup;

                case ChannelType.PM:
                    return PrivateChannelGroup;

                case ChannelType.Announce:
                    return AnnounceChannelGroup;

                default:
                    return PublicChannelGroup;
            }
        }

        private void updateVisibility()
        {
            if (AnnounceChannelGroup.ItemFlow.Children.Count == 0)
                AnnounceChannelGroup.Hide();
            else
                AnnounceChannelGroup.Show();
        }

        public partial class ChannelGroup : FillFlowContainer
        {
            private readonly bool sortByRecent;
            public readonly ChannelListItemFlow ItemFlow;

            public ChannelGroup(LocalisableString label, bool sortByRecent)
            {
                this.sortByRecent = sortByRecent;
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
                    ItemFlow = new ChannelListItemFlow(sortByRecent)
                    {
                        Direction = FillDirection.Vertical,
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                    },
                };
            }

            public partial class ChannelListItemFlow : FillFlowContainer<ChannelListItem>
            {
                private readonly bool sortByRecent;

                public ChannelListItemFlow(bool sortByRecent)
                {
                    this.sortByRecent = sortByRecent;
                }

                public void Reflow() => InvalidateLayout();

                public override IEnumerable<Drawable> FlowingChildren => sortByRecent
                    ? base.FlowingChildren.OfType<ChannelListItem>().OrderByDescending(i => i.Channel.LastMessageId ?? long.MinValue)
                    : base.FlowingChildren.OfType<ChannelListItem>().OrderBy(i => i.Channel.Name);
            }

            public void AddChannel(ChannelListItem item)
            {
                ItemFlow.Add(item);

                if (sortByRecent)
                {
                    item.Channel.NewMessagesArrived += newMessagesArrived;
                    item.Channel.PendingMessageResolved += pendingMessageResolved;
                }

                ItemFlow.Reflow();
            }

            public void RemoveChannel(ChannelListItem item)
            {
                if (sortByRecent)
                {
                    item.Channel.NewMessagesArrived -= newMessagesArrived;
                    item.Channel.PendingMessageResolved -= pendingMessageResolved;
                }

                ItemFlow.Remove(item, true);
            }

            private void pendingMessageResolved(LocalEchoMessage _, Message __) => ItemFlow.Reflow();
            private void newMessagesArrived(IEnumerable<Message> _) => ItemFlow.Reflow();

            protected override void Dispose(bool isDisposing)
            {
                base.Dispose(isDisposing);

                foreach (var item in ItemFlow)
                {
                    item.Channel.NewMessagesArrived -= newMessagesArrived;
                    item.Channel.PendingMessageResolved -= pendingMessageResolved;
                }
            }
        }

        private partial class ChannelSearchTextBox : BasicSearchTextBox
        {
            protected override bool AllowCommit => true;

            public ChannelSearchTextBox()
            {
                const float scale_factor = 0.8f;
                Scale = new Vector2(scale_factor);
                Width = 1 / scale_factor;
            }
        }
    }
}
