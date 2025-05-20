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
using osu.Framework.Graphics.Sprites;
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
        public ChannelGroup TeamChannelGroup { get; private set; } = null!;
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
                            // cross-reference for icons: https://github.com/ppy/osu-web/blob/3c9e99eaf4bd9e73d2712f60d67f5bc95f9dfe2b/resources/js/chat/conversation-list.tsx#L13-L19
                            AnnounceChannelGroup = new ChannelGroup(ChatStrings.ChannelsListTitleANNOUNCE.ToUpper(), FontAwesome.Solid.Bullhorn, false),
                            PublicChannelGroup = new ChannelGroup(ChatStrings.ChannelsListTitlePUBLIC.ToUpper(), FontAwesome.Solid.Comments, false),
                            selector = new ChannelListItem(ChannelListingChannel),
                            TeamChannelGroup = new ChannelGroup(ChatStrings.ChannelsListTitleTEAM.ToUpper(), FontAwesome.Solid.Users, false),
                            PrivateChannelGroup = new ChannelGroup(ChatStrings.ChannelsListTitlePM.ToUpper(), FontAwesome.Solid.Envelope, true),
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
            updateVisibility();
        }

        public void AddChannel(Channel channel)
        {
            if (channelMap.ContainsKey(channel))
                return;

            ChannelListItem item = new ChannelListItem(channel)
            {
                CanLeave = channel.Type != ChannelType.Team
            };
            item.OnRequestSelect += chan => OnRequestSelect?.Invoke(chan);
            if (item.CanLeave)
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

                case ChannelType.Team:
                    return TeamChannelGroup;

                default:
                    return PublicChannelGroup;
            }
        }

        private void updateVisibility()
        {
            AnnounceChannelGroup.Alpha = AnnounceChannelGroup.ItemFlow.Any() ? 1 : 0;
            TeamChannelGroup.Alpha = TeamChannelGroup.ItemFlow.Any() ? 1 : 0;
        }

        public partial class ChannelGroup : FillFlowContainer
        {
            private readonly bool sortByRecent;
            public readonly ChannelListItemFlow ItemFlow;

            public ChannelGroup(LocalisableString label, IconUsage icon, bool sortByRecent)
            {
                this.sortByRecent = sortByRecent;
                Direction = FillDirection.Vertical;
                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;
                Padding = new MarginPadding { Top = 8 };

                Children = new Drawable[]
                {
                    new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Horizontal,
                        Spacing = new Vector2(5),
                        Children = new Drawable[]
                        {
                            new OsuSpriteText
                            {
                                Text = label,
                                Margin = new MarginPadding { Left = 18, Bottom = 5 },
                                Font = OsuFont.Torus.With(size: 12, weight: FontWeight.SemiBold),
                            },
                            new SpriteIcon
                            {
                                Icon = icon,
                                Size = new Vector2(12),
                            },
                        }
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
