// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Edit.Checks;

namespace osu.Game.Rulesets.Catch.Edit.Checks
{
    public class CheckCatchLowestDiffDrainTime : CheckLowestDiffDrainTime
    {
        protected override IEnumerable<(DifficultyRating rating, double thresholdMs, string name)> GetThresholds()
        {
            // See lowest difficulty requirements in https://osu.ppy.sh/wiki/en/Ranking_criteria/osu%21catch#general
            yield return (DifficultyRating.Hard, (2 * 60 + 30) * 1000, "Platter"); // 2:30
            yield return (DifficultyRating.Insane, (3 * 60 + 15) * 1000, "Rain"); // 3:15
            yield return (DifficultyRating.Expert, 4 * 60 * 1000, "Overdose"); // 4:00
        }
    }
}
