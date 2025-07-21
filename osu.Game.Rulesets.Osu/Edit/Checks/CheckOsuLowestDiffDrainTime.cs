// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Edit.Checks;

namespace osu.Game.Rulesets.Osu.Edit.Checks
{
    public class CheckOsuLowestDiffDrainTime : CheckLowestDiffDrainTime
    {
        protected override IEnumerable<(DifficultyRating rating, double thresholdMs, string name)> GetThresholds()
        {
            // See lowest difficulty requirements in https://osu.ppy.sh/wiki/en/Ranking_criteria/osu%21#general
            yield return (DifficultyRating.Hard, new TimeSpan(0, 3, 30).TotalMilliseconds, "Hard");
            yield return (DifficultyRating.Insane, new TimeSpan(0, 4, 15).TotalMilliseconds, "Insane");
            yield return (DifficultyRating.Expert, new TimeSpan(0, 5, 0).TotalMilliseconds, "Expert");
        }
    }
}
