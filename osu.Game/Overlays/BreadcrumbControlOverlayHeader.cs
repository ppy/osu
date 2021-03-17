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
                protected override float ChevronSize => 8;

                public ControlTabItem(string value)
                    : base(value)
                {
                    RelativeSizeAxes = Axes.Y;
                    Text.Font = Text.Font.With(size: 14);
                    Text.Anchor = Anchor.CentreLeft;
                    Text.Origin = Anchor.CentreLeft;
                    Chevron.Y = 1;
                    Bar.Height = 0;
                }

                // base OsuTabItem makes font bold on activation, we don't want that here
                protected override void OnActivated() => FadeHovered();

                protected override void OnDeactivated() => FadeUnhovered();
            }
        }
    }
}
