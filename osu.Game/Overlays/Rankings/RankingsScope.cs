// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Overlays.Rankings
{
    public enum RankingsScope
    {
        [LocalisableDescription(typeof(RankingsStrings), nameof(RankingsStrings.StatPerformance))]
        Performance,

        [LocalisableDescription(typeof(RankingsStrings), nameof(RankingsStrings.StatRankedScore))]
        Score,

        [LocalisableDescription(typeof(RankingsStrings), nameof(RankingsStrings.TypeCountry))]
        Country,

        [LocalisableDescription(typeof(RankingsStrings), nameof(RankingsStrings.TypePlaylists))]
        Playlists,

        [LocalisableDescription(typeof(RankingsStrings), nameof(RankingsStrings.TypeKudosu))]
        Kudosu,
    }
}
