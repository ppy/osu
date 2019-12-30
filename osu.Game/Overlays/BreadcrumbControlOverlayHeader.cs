// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Graphics;

namespace osu.Game.Overlays
{
    public abstract class BreadcrumbControlOverlayHeader : ControllableOverlayHeader<OverlayHeaderBreadcrumbControl, string>
    {
        protected override OverlayHeaderBreadcrumbControl CreateControl() => new OverlayHeaderBreadcrumbControl(ColourScheme);

        protected BreadcrumbControlOverlayHeader(OverlayColourScheme colourScheme)
            : base(colourScheme)
        {
        }
    }
}
