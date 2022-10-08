// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Game.Localisation;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Overlays.BeatmapListing
{
    public class BeatmapListingHeader : OverlayHeader
    {
        protected override OverlayTitle CreateTitle() => new BeatmapListingTitle();

        private class BeatmapListingTitle : OverlayTitle
        {
            public BeatmapListingTitle()
            {
                Title = PageTitleStrings.MainBeatmapsetsControllerIndex;
                Description = NamedOverlayComponentStrings.BeatmapListingDescription;
                IconTexture = "Icons/Hexacons/beatmap";
            }
        }
    }
}
