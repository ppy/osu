// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Sprites;

namespace osu.Game.Overlays.Toolbar
{
    public class ToolbarHomeButton : ToolbarButton
    {
        public ToolbarHomeButton()
        {
            Icon = FontAwesome.Solid.Home;
            TooltipMain = "Home";
            TooltipSub = "Return to the main menu";
        }
    }
}
