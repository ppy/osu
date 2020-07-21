// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;

namespace osu.Game.Overlays.Toolbar
{
    public class ToolbarRankingsButton : ToolbarOverlayToggleButton
    {
        public ToolbarRankingsButton()
        {
            SetIcon(FontAwesome.Regular.ChartBar);
        }

        [BackgroundDependencyLoader(true)]
        private void load(RankingsOverlay rankings)
        {
            StateContainer = rankings;
        }
    }
}
