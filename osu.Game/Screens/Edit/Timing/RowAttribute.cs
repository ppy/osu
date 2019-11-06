// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Screens.Edit.Timing
{
    public class RowAttribute : CompositeDrawable, IHasTooltip
    {
        private readonly string header;
        private readonly Func<string> content;

        public RowAttribute(string header, Func<string> content)
        {
            this.header = header;
            this.content = content;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            AutoSizeAxes = Axes.X;

            Height = 20;

            Anchor = Anchor.CentreLeft;
            Origin = Anchor.CentreLeft;

            Masking = true;
            CornerRadius = 5;

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    Colour = colours.Yellow,
                    RelativeSizeAxes = Axes.Both,
                },
                new OsuSpriteText
                {
                    Padding = new MarginPadding(2),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Font = OsuFont.Default.With(weight: FontWeight.SemiBold, size: 12),
                    Text = header,
                    Colour = colours.Gray3
                },
            };
        }

        public string TooltipText => content();
    }
}
