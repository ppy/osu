// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK;
using osuTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics.Sprites;

namespace osu.Game.Overlays.Settings
{
    public abstract class SettingsSection : Container, IHasFilterableChildren
    {
        protected FillFlowContainer FlowContent;
        protected override Container<Drawable> Content => FlowContent;

        public abstract IconUsage Icon { get; }
        public abstract string Header { get; }

        public IEnumerable<IFilterable> FilterableChildren => Children.OfType<IFilterable>();
        public IEnumerable<string> FilterTerms => new[] { Header };

        private const int header_size = 26;
        private const int header_margin = 25;
        private const int border_size = 2;

        public bool MatchingFilter
        {
            set => this.FadeTo(value ? 1 : 0);
        }

        public bool FilteringActive { get; set; }

        protected SettingsSection()
        {
            Margin = new MarginPadding { Top = 20 };
            AutoSizeAxes = Axes.Y;
            RelativeSizeAxes = Axes.X;

            FlowContent = new FillFlowContainer
            {
                Margin = new MarginPadding
                {
                    Top = header_size + header_margin
                },
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0, 30),
                AutoSizeAxes = Axes.Y,
                RelativeSizeAxes = Axes.X,
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            AddRangeInternal(new Drawable[]
            {
                new Box
                {
                    Colour = new Color4(0, 0, 0, 255),
                    RelativeSizeAxes = Axes.X,
                    Height = border_size,
                },
                new Container
                {
                    Padding = new MarginPadding
                    {
                        Top = 20 + border_size,
                        Bottom = 10,
                    },
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Children = new Drawable[]
                    {
                        new OsuSpriteText
                        {
                            Font = OsuFont.GetFont(size: header_size),
                            Text = Header,
                            Colour = colours.Yellow,
                            Margin = new MarginPadding { Left = SettingsPanel.CONTENT_MARGINS, Right = SettingsPanel.CONTENT_MARGINS }
                        },
                        FlowContent
                    }
                },
            });
        }
    }
}
