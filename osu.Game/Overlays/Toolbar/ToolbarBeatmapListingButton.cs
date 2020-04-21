// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Game.Graphics;

namespace osu.Game.Overlays.Toolbar
{
    public class ToolbarBeatmapListingButton : ToolbarOverlayToggleButtonRightSide
    {
        public ToolbarBeatmapListingButton()
        {
            SetIcon(OsuIcon.ChevronDownCircle);
            TooltipMain = "谱面列表";
            TooltipSub = "在这里下图";
        }

        [BackgroundDependencyLoader(true)]
        private void load(BeatmapListingOverlay beatmapListing)
        {
            StateContainer = beatmapListing;
        }
    }
}
