// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Rulesets.Osu.Scoring;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Ranking.Statistics;
using osuTK;

namespace osu.Game.Rulesets.Osu.Statistics
{
    /// <summary>
    /// Displays the aim error statistic for a given play.
    /// </summary>
    public partial class AverageAimError : SimpleStatisticItem<double?>
    {
        /// <summary>
        /// Creates and computes an <see cref="AverageHitError"/> statistic.
        /// </summary>
        /// <param name="hitEvents">Sequence of <see cref="HitEvent"/>s to calculate the aim error based on.</param>
        public AverageAimError(IEnumerable<HitEvent> hitEvents)
            : base("Average Aim Error")
        {
            Vector2? offsetVector = hitEvents.CalculateAverageAimError();

            Value = offsetVector?.Length;
        }

        protected override string DisplayValue(double? value) => value == null ? "(not available)" : $"{Math.Abs(value.Value):N2} osu! pixels from center";
    }
}
