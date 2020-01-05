// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;

namespace osu.Game.Overlays.Toolbar
{
    public class ToolbarChangelogButton : ToolbarOverlayToggleButton
    {
        public ToolbarChangelogButton()
        {
            SetIcon(FontAwesome.Solid.Bullhorn);
            TooltipMain = "变更日志";
            TooltipSub = "在这里查看变更日志";
        }

        [BackgroundDependencyLoader(true)]
        private void load(ChangelogOverlay changelog)
        {
            StateContainer = changelog;
        }
    }
}
