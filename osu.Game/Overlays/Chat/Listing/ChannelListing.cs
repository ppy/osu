// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.Containers;
using osu.Game.Online.Chat;

namespace osu.Game.Overlays.Chat.Listing
{
    public partial class ChannelListing : VisibilityContainer
    {
        public event Action<Channel>? OnRequestJoin;
        public event Action<Channel>? OnRequestLeave;

        public string SearchTerm
        {
            get => flow.SearchTerm;
            set => flow.SearchTerm = value;
        }

        private SearchContainer<ChannelListingItem> flow = null!;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colourProvider.Background4,
                },
                new OsuScrollContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    ScrollbarAnchor = Anchor.TopRight,
                    Child = flow = new SearchContainer<ChannelListingItem>
                    {
                        Direction = FillDirection.Vertical,
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Padding = new MarginPadding
                        {
                            Vertical = 13,
                            Horizontal = 15,
                        },
                    },
                },
            };
        }

        public void UpdateAvailableChannels(IEnumerable<Channel> newChannels)
        {
            flow.ChildrenEnumerable = newChannels.Where(c => c.Type == ChannelType.Public)
                                                 .Select(c => new ChannelListingItem(c));

            foreach (var item in flow)
            {
                item.OnRequestJoin += channel => OnRequestJoin?.Invoke(channel);
                item.OnRequestLeave += channel => OnRequestLeave?.Invoke(channel);
            }
        }

        protected override void PopIn() => this.FadeIn();

        protected override void PopOut() => this.FadeOut();

        public class ChannelListingChannel : Channel
        {
            public ChannelListingChannel()
            {
                Name = "Add more channels";
                Type = ChannelType.System;
            }
        }
    }
}
