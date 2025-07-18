// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Edit.Checks;

namespace osu.Game.Rulesets.Mania.Edit.Checks
{
    public class CheckManiaLowestDiffDrainTime : CheckLowestDiffDrainTime
    {
        protected override IEnumerable<(DifficultyRating rating, double thresholdMs, string name)> GetThresholds()
        {
            // See lowest difficulty requirements in https://osu.ppy.sh/wiki/en/Ranking_criteria/osu%21mania#rules
            yield return (DifficultyRating.Hard, (2 * 60 + 30) * 1000, "Hard"); // 2:30
            yield return (DifficultyRating.Insane, (2 * 60 + 45) * 1000, "Insane"); // 2:45
            yield return (DifficultyRating.Expert, (3 * 60 + 30) * 1000, "Expert"); // 3:30
        }
    }
}
