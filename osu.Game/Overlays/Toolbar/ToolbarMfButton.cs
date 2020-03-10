// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;

namespace osu.Game.Overlays.Toolbar
{
    public class ToolbarMfButton : ToolbarOverlayToggleButton
    {
        public ToolbarMfButton()
        {
            Icon = FontAwesome.Solid.Gift;
            TooltipMain = "Mf-osu";
            TooltipSub = "这是一个官方lazer的分支,祝游玩愉快(｡･ω･)ﾉﾞ";
        }

        [BackgroundDependencyLoader(true)]
        private void load(CustomMenuOverlay custommenu)
        {
            StateContainer = custommenu;
        }
    }
}
