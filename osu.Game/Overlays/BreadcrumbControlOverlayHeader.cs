// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays
{
    public abstract class BreadcrumbControlOverlayHeader : OverlayHeader
    {
        protected OverlayHeaderBreadcrumbControl BreadcrumbControl;

        protected override TabControl<string> CreateTabControl() => BreadcrumbControl = new OverlayHeaderBreadcrumbControl();

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            BreadcrumbControl.AccentColour = colourProvider.Highlight1;
        }

        public class OverlayHeaderBreadcrumbControl : BreadcrumbControl<string>
        {
            public OverlayHeaderBreadcrumbControl()
            {
                RelativeSizeAxes = Axes.X;
            }

            protected override TabItem<string> CreateTabItem(string value) => new ControlTabItem(value);

            private class ControlTabItem : BreadcrumbTabItem
            {
                protected override float ChevronSize => 8;

                public ControlTabItem(string value)
                    : base(value)
                {
                    Text.Font = Text.Font.With(size: 14);
                    Chevron.Y = 3;
                    Bar.Height = 0;
                }
            }
        }
    }
}
