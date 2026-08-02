// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Localisation;
using osu.Game.Rulesets.Scoring;
using osu.Game.Localisation;

namespace osu.Game.Screens.Ranking.Statistics
{
    /// <summary>
    /// Displays the unstable rate statistic for a given play.
    /// </summary>
    public partial class AverageHitError : SimpleStatisticItem<double?>
    {
        /// <summary>
        /// Creates and computes an <see cref="AverageHitError"/> statistic.
        /// </summary>
        /// <param name="hitEvents">Sequence of <see cref="HitEvent"/>s to calculate the unstable rate based on.</param>
        public AverageHitError(IEnumerable<HitEvent> hitEvents)
            : base(RankingStatisticsStrings.AverageHitErrorTitle)
        {
            Value = hitEvents.CalculateAverageHitError();
        }

        protected override LocalisableString DisplayValue(double? value)
        {
            return value == null ? RankingStatisticsStrings.NotAvailable : getEarlyLateText(value.Value);

            LocalisableString getEarlyLateText(double offset) =>
                offset < 0
                    ? RankingStatisticsStrings.Early(Math.Abs(offset))
                    : RankingStatisticsStrings.Late(Math.Abs(offset));
        }
    }
}
