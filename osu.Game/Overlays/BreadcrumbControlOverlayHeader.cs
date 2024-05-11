// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Localisation;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays
{
    public abstract partial class BreadcrumbControlOverlayHeader : TabControlOverlayHeader<LocalisableString?>
    {
        protected override OsuTabControl<LocalisableString?> CreateTabControl() => new OverlayHeaderBreadcrumbControl();

        public partial class OverlayHeaderBreadcrumbControl : BreadcrumbControl<LocalisableString?>
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

            protected override TabItem<LocalisableString?> CreateTabItem(LocalisableString? value) => new ControlTabItem(value)
            {
                AccentColour = AccentColour,
            };

            private partial class ControlTabItem : BreadcrumbTabItem
            {
                protected override float ChevronSize => 8;

                public ControlTabItem(LocalisableString? value)
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
