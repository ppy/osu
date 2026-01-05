// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Edit.Checks;

namespace osu.Game.Rulesets.Taiko.Edit.Checks
{
    public class CheckTaikoLowestDiffDrainTime : CheckLowestDiffDrainTime
    {
        protected override IEnumerable<(DifficultyRating rating, double thresholdMs, string name)> GetThresholds()
        {
            // See lowest difficulty requirements in https://osu.ppy.sh/wiki/en/Ranking_criteria/osu%21taiko#general
            yield return (DifficultyRating.Hard, new TimeSpan(0, 2, 30).TotalMilliseconds, "Muzukashii");
            yield return (DifficultyRating.Insane, new TimeSpan(0, 3, 15).TotalMilliseconds, "Oni");
            yield return (DifficultyRating.Expert, new TimeSpan(0, 4, 0).TotalMilliseconds, "Inner Oni");
        }
    }
}
