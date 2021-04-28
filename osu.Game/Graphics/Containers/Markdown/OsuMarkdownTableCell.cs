// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Markdig.Extensions.Tables;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers.Markdown;
using osu.Framework.Graphics.Shapes;
using osu.Game.Overlays;

namespace osu.Game.Graphics.Containers.Markdown
{
    public class OsuMarkdownTableCell : MarkdownTableCell
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
        private void load(OverlayColourProvider colourProvider)
        {
            var border = new Box
            {
                RelativeSizeAxes = Axes.X,
            };

            // TODO : Change font weight to 700 for heading
            if (isHeading)
            {
                border.Colour = colourProvider.Background3;
                border.Height = 2;
                border.Anchor = Anchor.BottomLeft;
                border.Origin = Anchor.BottomLeft;
            }
            else
            {
                border.Colour = colourProvider.Background4;
                border.Height = 1;
                border.Anchor = Anchor.TopLeft;
                border.Origin = Anchor.TopLeft;
            }

            AddInternal(border);
        }
    }
}
