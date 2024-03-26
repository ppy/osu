// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Checks;
using osu.Game.Rulesets.Edit.Checks.Components;

namespace osu.Game.Rulesets.Taiko.Edit.Checks
{
    public class CheckTaikoAbnormalDifficultySettings : CheckAbnormalDifficultySettings
    {
        public override CheckMetadata Metadata => new CheckMetadata(CheckCategory.Settings, "Checks taiko relevant settings");

        public override IEnumerable<Issue> Run(BeatmapVerifierContext context)
        {
            var diff = context.Beatmap.Difficulty;

            if (HasMoreThanOneDecimalPlace(diff.OverallDifficulty))
                yield return new IssueTemplateMoreThanOneDecimal(this).Create("Overall difficulty", diff.OverallDifficulty);

            if (OutOfRange(diff.OverallDifficulty))
                yield return new IssueTemplateOutOfRange(this).Create("Overall difficulty", diff.OverallDifficulty);

            if (HasMoreThanOneDecimalPlace(diff.DrainRate))
                yield return new IssueTemplateMoreThanOneDecimal(this).Create("Drain rate", diff.DrainRate);

            if (OutOfRange(diff.DrainRate))
                yield return new IssueTemplateOutOfRange(this).Create("Drain rate", diff.DrainRate);
        }
    }
}
