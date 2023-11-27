// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Game.Graphics;
using osu.Game.Input.Bindings;
using osu.Game.Localisation;

namespace osu.Game.Overlays.Toolbar
{
    public partial class ToolbarHomeButton : ToolbarButton
    {
        public ToolbarHomeButton()
        {
            Width *= 1.4f;
            Hotkey = GlobalAction.Home;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            TooltipMain = ToolbarStrings.HomeHeaderTitle;
            TooltipSub = ToolbarStrings.HomeHeaderDescription;
            SetIcon(HexaconsIcons.Home);
        }
    }
}
