// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Markdig.Extensions.Tables;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers.Markdown;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Overlays;

namespace osu.Game.Graphics.Containers.Markdown
{
    public partial class OsuMarkdownTableCell : MarkdownTableCell
    {
        private readonly bool isHeading;

        public OsuMarkdownTableCell(TableCell cell, TableColumnDefinition definition, bool isHeading)
            : base(cell, definition)
        {
            this.isHeading = isHeading;
            Masking = false;
            BorderThickness = 0;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            AddInternal(CreateBorder(isHeading));
        }

        public override MarkdownTextFlowContainer CreateTextFlow() => new TableCellTextFlowContainer
        {
            Weight = isHeading ? FontWeight.Bold : FontWeight.Regular,
            Padding = new MarginPadding(10),
        };

        protected virtual Box CreateBorder(bool isHeading)
        {
            if (isHeading)
                return new TableHeadBorder();

            return new TableBodyBorder();
        }

        private partial class TableHeadBorder : Box
        {
            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider)
            {
                Colour = colourProvider.Background3;
                RelativeSizeAxes = Axes.X;
                Height = 2;
                Anchor = Anchor.BottomLeft;
                Origin = Anchor.BottomLeft;
            }
        }

        private partial class TableBodyBorder : Box
        {
            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider)
            {
                Colour = colourProvider.Background4;
                RelativeSizeAxes = Axes.X;
                Height = 1;
            }
        }

        private partial class TableCellTextFlowContainer : OsuMarkdownTextFlowContainer
        {
            public FontWeight Weight { get; set; }

            protected override SpriteText CreateSpriteText() => base.CreateSpriteText().With(t => t.Font = t.Font.With(weight: Weight));
        }
    }
}
