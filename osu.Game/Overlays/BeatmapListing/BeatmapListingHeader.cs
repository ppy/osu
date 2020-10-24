// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Overlays.BeatmapListing
{
    public class BeatmapListingHeader : OverlayHeader
    {
        protected override OverlayTitle CreateTitle() => new BeatmapListingTitle();

        private class BeatmapListingTitle : OverlayTitle
        {
            public BeatmapListingTitle()
            {
                Title = "beatmap listing";
                Description = "browse for new beatmaps";
                IconTexture = "Icons/Hexacons/beatmap";
            }
        }
    }
}
