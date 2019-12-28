// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Graphics.UserInterface;
using osu.Framework.Graphics;
using osu.Framework.Graphics.UserInterface;

namespace osu.Game.Overlays
{
    public class OverlayHeaderBreadcrumbControl : BreadcrumbControl<string>
    {
        private const float item_chevron_size = 8;

        public OverlayHeaderBreadcrumbControl()
            : base(item_chevron_size)
        {
            RelativeSizeAxes = Axes.X;
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
