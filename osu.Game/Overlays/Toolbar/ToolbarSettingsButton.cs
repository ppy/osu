﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
