﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Game.Graphics;

namespace osu.Game.Overlays.Toolbar
{
    internal class ToolbarSettingsButton : ToolbarOverlayToggleButton
    {
        public ToolbarSettingsButton()
        {
            Icon = FontAwesome.fa_gear;
            TooltipMain = "Settings";
            TooltipSub = "Change your settings";
        }

        [BackgroundDependencyLoader]
        private void load(OptionsOverlay options)
        {
            StateContainer = options;
        }
    }
}