// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osuTK;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Chat;

namespace osu.Game.Overlays.Chat.Selection
{
    public class ChannelSection : Container, IHasFilterableChildren
    {
        private readonly OsuSpriteText header;

        public readonly FillFlowContainer<ChannelListItem> ChannelFlow;

        public IEnumerable<IFilterable> FilterableChildren => ChannelFlow.Children;
        public IEnumerable<string> FilterTerms => Array.Empty<string>();
        public bool MatchingFilter
        {
            set
            {
                this.FadeTo(value ? 1f : 0f, 100);
            }
        }

        public string Header
        {
            get { return header.Text; }
            set { header.Text = value.ToUpperInvariant(); }
        }

        public IEnumerable<Channel> Channels
        {
            set { ChannelFlow.ChildrenEnumerable = value.Select(c => new ChannelListItem(c)); }
        }

        public ChannelSection()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            Children = new Drawable[]
            {
                header = new OsuSpriteText
                {
                    TextSize = 15,
                    Font = @"Exo2.0-Bold",
                },
                ChannelFlow = new FillFlowContainer<ChannelListItem>
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Margin = new MarginPadding { Top = 25 },
                    Spacing = new Vector2(0f, 5f),
                },
            };
        }
    }
}
