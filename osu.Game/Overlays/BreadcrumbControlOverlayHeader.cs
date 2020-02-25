// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays
{
    public abstract class BreadcrumbControlOverlayHeader : TabControlOverlayHeader<string>
    {
        protected override OsuTabControl<string> CreateTabControl() => new OverlayHeaderBreadcrumbControl();

        public class OverlayHeaderBreadcrumbControl : BreadcrumbControl<string>
        {
            public OverlayHeaderBreadcrumbControl()
            {
                RelativeSizeAxes = Axes.X;
                Height = 47;
            }

            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider)
            {
                AccentColour = colourProvider.Light2;
            }

            protected override TabItem<string> CreateTabItem(string value) => new ControlTabItem(value);

            private class ControlTabItem : BreadcrumbTabItem
            {
                protected override float ChevronSize => 10;

                public ControlTabItem(string value)
                    : base(value)
                {
                    RelativeSizeAxes = Axes.Y;
                    Text.Font = Text.Font.With(size: 14);
                    Text.Margin = new MarginPadding { Vertical = 16.5f }; // 15px padding + 1.5px line-height difference compensation
                    Bar.Height = 0;
                }
            }
        }
    }
}
