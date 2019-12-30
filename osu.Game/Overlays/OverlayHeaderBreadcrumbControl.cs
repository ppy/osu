// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Graphics.UserInterface;
using osu.Framework.Graphics;
using osu.Framework.Graphics.UserInterface;

namespace osu.Game.Overlays
{
    public class OverlayHeaderBreadcrumbControl : BreadcrumbControl<string>
    {
        public OverlayHeaderBreadcrumbControl()
        {
            RelativeSizeAxes = Axes.X;
        }

        protected override float ItemChevronSize => 8;

        protected override TabItem<string> CreateTabItem(string value) => new ControlTabItem(value, ItemChevronSize);

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
