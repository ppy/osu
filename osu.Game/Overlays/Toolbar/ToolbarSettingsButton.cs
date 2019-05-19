// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Commands;

namespace osu.Game.Overlays.Toolbar
{
    public class ToolbarSettingsButton : ToolbarOverlayToggleButton
    {
        public ToolbarSettingsButton()
        {
            Icon = FontAwesome.Solid.Cog;
            TooltipMain = "Settings";
            TooltipSub = "Change your settings";
        }

        [BackgroundDependencyLoader(true)]
        private void load(SettingsOverlay settings, ToggleOverlayCommand<SettingsPanel> toggleOverlayCommand)
        {
            StateContainer = settings;
            Command = toggleOverlayCommand;
        }
    }
}
