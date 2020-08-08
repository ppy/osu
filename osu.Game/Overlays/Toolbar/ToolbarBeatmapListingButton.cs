// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Game.Graphics;
using osu.Game.Input.Bindings;

namespace osu.Game.Overlays.Toolbar
{
    public class ToolbarBeatmapListingButton : ToolbarOverlayToggleButton
    {
        public ToolbarBeatmapListingButton()
        {
            SetIcon(OsuIcon.ChevronDownCircle);
            TooltipMain = "Beatmap listing";
            TooltipSub = "Browse for new beatmaps";

            Hotkey = GlobalAction.ToggleDirect;
        }

        [BackgroundDependencyLoader(true)]
        private void load(BeatmapListingOverlay beatmapListing)
        {
            StateContainer = beatmapListing;
        }
    }
}
