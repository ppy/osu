// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Game.Graphics;

namespace osu.Game.Overlays.Toolbar
{
    public class ToolbarSettingsButton : ToolbarOverlayToggleButton
    {
        public ToolbarSettingsButton()
        {
            Icon = FontAwesome.fa_gear;
            TooltipMain = "Settings";
            TooltipSub = "Change your settings";
        }

        [BackgroundDependencyLoader(true)]
        private void load(MainSettings settings)
        {
            StateContainer = settings;
        }
    }
}
