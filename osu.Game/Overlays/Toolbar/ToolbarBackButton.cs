// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Sprites;

namespace osu.Game.Overlays.Toolbar
{
    public class ToolbarBackButton : ToolbarOverlayToggleButton
    {
        public ToolbarBackButton()
        {
            Icon = FontAwesome.Solid.ChevronLeft;
            TooltipMain = "Back";
            TooltipSub = "Go Back a screen";
        }
    }
}
