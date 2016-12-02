//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Game.Graphics;
using osu.Game.Screens.Menu;

namespace osu.Game.Overlays.Toolbar
{
    class ToolbarHomeButton : ToolbarButton
    {
        public ToolbarHomeButton()
        {
            Icon = FontAwesome.fa_home;
            TooltipMain = "Home";
            TooltipSub = "Return to the main menu";
        }
    }
}