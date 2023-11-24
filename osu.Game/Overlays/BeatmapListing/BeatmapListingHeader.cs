// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Localisation;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Overlays.BeatmapListing
{
    public partial class BeatmapListingHeader : OverlayHeader
    {
        public BeatmapListingFilterControl FilterControl { get; private set; }

        protected override OverlayTitle CreateTitle() => new BeatmapListingTitle();

        protected override Drawable CreateContent() => FilterControl = new BeatmapListingFilterControl();

        private partial class BeatmapListingTitle : OverlayTitle
        {
            public BeatmapListingTitle()
            {
                Title = PageTitleStrings.MainBeatmapsetsControllerIndex;
                Description = NamedOverlayComponentStrings.BeatmapListingDescription;
                Icon = HexaconsIcons.Beatmap;
            }
        }
    }
}
