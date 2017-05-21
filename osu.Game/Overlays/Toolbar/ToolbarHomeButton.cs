// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Graphics;

namespace osu.Game.Overlays.Toolbar
{
    internal class ToolbarHomeButton : ToolbarButton
    {
        public ToolbarHomeButton()
        {
            Icon = FontAwesome.fa_home;
            TooltipMain = "Home";
            TooltipSub = "Return to the main menu";
        }
    }
}