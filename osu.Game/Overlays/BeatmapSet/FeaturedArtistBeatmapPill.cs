// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Resources.Localisation.Web;

#nullable enable

namespace osu.Game.Overlays.BeatmapSet
{
    public class FeaturedArtistBeatmapPill : BeatmapBadgePill
    {
        public override LocalisableString BadgeText => BeatmapsetsStrings.FeaturedArtistBadgeLabel;
        public override Colour4 BadgeColour => Colours.Blue1;
    }
}
