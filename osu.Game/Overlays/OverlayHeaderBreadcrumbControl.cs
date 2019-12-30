// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Graphics.UserInterface;
using osu.Framework.Graphics;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Allocation;
using osu.Game.Graphics;

namespace osu.Game.Overlays
{
    public class OverlayHeaderBreadcrumbControl : BreadcrumbControl<string>
    {
        private const float item_chevron_size = 8;

        private readonly OverlayColourScheme colourScheme;

        public OverlayHeaderBreadcrumbControl(OverlayColourScheme colourScheme)
            : base(item_chevron_size)
        {
            this.colourScheme = colourScheme;

            RelativeSizeAxes = Axes.X;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            AccentColour = colours.ForOverlayElement(colourScheme, 1, 0.75f);
        }

        protected override TabItem<string> CreateTabItem(string value) => new ControlTabItem(value, item_chevron_size);

        private class ControlTabItem : BreadcrumbTabItem
        {
            public ControlTabItem(string value, float itemChevronSize)
                : base(value, itemChevronSize)
            {
                Text.Font = Text.Font.With(size: 14);
                Chevron.Y = 3;
            }
        }
    }
}
