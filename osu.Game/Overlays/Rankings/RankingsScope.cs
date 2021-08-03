// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Localisation;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Overlays.Rankings
{
    [LocalisableEnum(typeof(RankingsScopeEnumLocalisationMapper))]
    public enum RankingsScope
    {
        Performance,
        Spotlights,
        Score,
        Country
    }

    public class RankingsScopeEnumLocalisationMapper : EnumLocalisationMapper<RankingsScope>
    {
        public override LocalisableString Map(RankingsScope value)
        {
            switch (value)
            {
                case RankingsScope.Performance:
                    return RankingsStrings.TypePerformance;

                case RankingsScope.Spotlights:
                    return RankingsStrings.TypeCharts;

                case RankingsScope.Score:
                    return RankingsStrings.TypeScore;

                case RankingsScope.Country:
                    return RankingsStrings.TypeCountry;

                default:
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }
        }
    }
}
