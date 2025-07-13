// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
            yield return (DifficultyRating.Hard, (3 * 60 + 30) * 1000, "Hard"); // 3:30
            yield return (DifficultyRating.Insane, (4 * 60 + 15) * 1000, "Insane"); // 4:15
            yield return (DifficultyRating.Expert, 5 * 60 * 1000, "Expert"); // 5:00
        }
    }
}
