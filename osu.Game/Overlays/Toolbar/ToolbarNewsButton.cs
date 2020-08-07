// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;

namespace osu.Game.Overlays.Toolbar
{
    public class ToolbarNewsButton : ToolbarOverlayToggleButton
    {
        public ToolbarNewsButton()
        {
            Icon = FontAwesome.Solid.Newspaper;
            TooltipMain = "新闻";
            TooltipSub = "看看社区上都发生了什么";
        }

        [BackgroundDependencyLoader(true)]
        private void load(NewsOverlay news)
        {
            StateContainer = news;
        }
    }
}
