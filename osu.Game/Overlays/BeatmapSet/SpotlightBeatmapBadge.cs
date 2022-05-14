// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Resources.Localisation.Web;

#nullable enable

namespace osu.Game.Overlays.BeatmapSet
{
    public class SpotlightBeatmapBadge : BeatmapBadge
    {
        public override LocalisableString BadgeText => BeatmapsetsStrings.SpotlightBadgeLabel;
        public override Colour4 BadgeColour => Colours.Pink1;
    }
}
