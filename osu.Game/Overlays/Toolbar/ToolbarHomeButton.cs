// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Game.Input.Bindings;
using osu.Game.Localisation;

namespace osu.Game.Overlays.Toolbar
{
    public class ToolbarHomeButton : ToolbarButton
    {
        public ToolbarHomeButton()
        {
            Width *= 1.4f;
            Hotkey = GlobalAction.Home;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            TooltipMain = HomeStrings.HeaderTitle;
            TooltipSub = HomeStrings.HeaderDescription;
            SetIcon("Icons/Hexacons/home");
        }
    }
}
