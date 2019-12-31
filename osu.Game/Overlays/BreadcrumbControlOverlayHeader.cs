// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.UserInterface;

namespace osu.Game.Overlays
{
    public abstract class BreadcrumbControlOverlayHeader : ControllableOverlayHeader
    {
        protected OverlayHeaderBreadcrumbControl TabControl;

        protected override TabControl<string> CreateControl() => TabControl = new OverlayHeaderBreadcrumbControl();
    }
}
