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
            TooltipMain = "News";
            TooltipSub = "Get up-to-date on community happenings";
        }

        [BackgroundDependencyLoader(true)]
        private void load(NewsOverlay news)
        {
            StateContainer = news;
        }
    }
}
