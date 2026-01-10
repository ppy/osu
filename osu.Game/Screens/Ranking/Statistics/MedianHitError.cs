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
    /// Displays the median hit error statistic for a given play.
    /// </summary>
    public partial class MedianHitError : SimpleStatisticItem<double?>
    {
        /// <summary>
        /// Creates and computes a <see cref="MedianHitError"/> statistic.
        /// </summary>
        /// <param name="hitEvents">Sequence of <see cref="HitEvent"/>s to calculate the median hit error based on.</param>
        public MedianHitError(IEnumerable<HitEvent> hitEvents)
            : base(RankingStatisticsStrings.MedianHitErrorTitle)
        {
            Value = hitEvents.CalculateMedianHitError();
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