// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Text;
using osu.Framework.Graphics.Sprites;
using osu.Game.Input.Bindings;
using osu.Game.Overlays.Volume;
using osu.Game.Graphics.UserInterface;
using osu.Framework.Allocation;

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
