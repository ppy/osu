// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Rulesets.Osu.Scoring;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Ranking.Statistics;

namespace osu.Game.Rulesets.Osu.Statistics
{
    /// <summary>
    /// Displays the aim error statistic for a given play.
    /// </summary>
    public partial class AimError : SimpleStatisticItem<double?>
    {
        /// <summary>
        /// Creates and computes an <see cref="AimError"/> statistic.
        /// </summary>
        /// <param name="hitEvents">Sequence of <see cref="HitEvent"/>s to calculate the aim error based on.</param>
        public AimError(IEnumerable<HitEvent> hitEvents)
            : base("Aim Error")
        {
            Value = hitEvents.CalculateAimError();
        }

        protected override string DisplayValue(double? value) => value == null ? "(not available)" : value.Value.ToString(@"N2");
    }
}
