// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;
using osu.Game.Input.Bindings;

namespace osu.Game.Overlays.Toolbar
{
    public class ToolbarSocialButton : ToolbarOverlayToggleButtonRightSide
    {
        public ToolbarSocialButton()
        {
            Icon = FontAwesome.Solid.Users;
            TooltipMain = "看板";
            TooltipSub = "在这里查看各种各样的东西";

            Hotkey = GlobalAction.ToggleSocial;
        }

        [BackgroundDependencyLoader(true)]
        private void load(DashboardOverlay dashboard)
        {
            StateContainer = dashboard;
        }
    }
}
