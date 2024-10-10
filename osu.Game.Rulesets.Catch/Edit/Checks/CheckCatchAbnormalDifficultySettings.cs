// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Checks;
using osu.Game.Rulesets.Edit.Checks.Components;

namespace osu.Game.Rulesets.Catch.Edit.Checks
{
    public class CheckCatchAbnormalDifficultySettings : CheckAbnormalDifficultySettings
    {
        public override CheckMetadata Metadata => new CheckMetadata(CheckCategory.Settings, "Checks catch relevant settings");

        public override IEnumerable<Issue> Run(BeatmapVerifierContext context)
        {
            var diff = context.Beatmap.Difficulty;
            Issue? issue;

            if (HasMoreThanOneDecimalPlace("Approach rate", diff.ApproachRate, out issue))
                yield return issue;

            if (OutOfRange("Approach rate", diff.ApproachRate, out issue))
                yield return issue;

            if (HasMoreThanOneDecimalPlace("Circle size", diff.CircleSize, out issue))
                yield return issue;

            if (OutOfRange("Circle size", diff.CircleSize, out issue))
                yield return issue;

            if (HasMoreThanOneDecimalPlace("Drain rate", diff.DrainRate, out issue))
                yield return issue;

            if (OutOfRange("Drain rate", diff.DrainRate, out issue))
                yield return issue;
        }
    }
}
