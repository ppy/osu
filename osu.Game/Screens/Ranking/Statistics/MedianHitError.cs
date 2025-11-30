// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Screens.Ranking.Statistics
{
    public partial class MedianHitError : SimpleStatisticItem<double?>
    {
        public MedianHitError(IEnumerable<HitEvent> hitEvents)
            : base("Median Hit Error")
        {
            Value = hitEvents.CalculateMedianHitError();
        }

        protected override string DisplayValue(double? value) => value == null ? "(not available)" : $"{Math.Abs(value.Value):N2} ms {(value.Value < 0 ? "early" : "late")}";
    }
}
