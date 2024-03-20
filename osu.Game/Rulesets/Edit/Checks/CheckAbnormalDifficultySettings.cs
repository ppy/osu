// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Rulesets.Edit.Checks.Components;


namespace osu.Game.Rulesets.Edit.Checks
{
    public class CheckAbnormalDifficultySettings : ICheck
    {
        public CheckMetadata Metadata => new CheckMetadata(CheckCategory.Settings, "Abnormal difficulty settings");

        public IEnumerable<IssueTemplate> PossibleTemplates => new IssueTemplate[]
        {
            new IssueTemplateMoreThanOneDecimal(this),
            new IssueTemplateOutOfRange(this),
        };

        public IEnumerable<Issue> Run(BeatmapVerifierContext context)
        {
            var diff = context.Beatmap.Difficulty;

            if (hasMoreThanOneDecimalPlace(diff.ApproachRate))
                yield return new IssueTemplateMoreThanOneDecimal(this).Create("Approach rate", diff.ApproachRate);

            if (isOutOfRange(diff.ApproachRate))
                yield return new IssueTemplateOutOfRange(this).Create("Approach rate", diff.ApproachRate);


            if (hasMoreThanOneDecimalPlace(diff.OverallDifficulty))
                yield return new IssueTemplateMoreThanOneDecimal(this).Create("Overall difficulty", diff.OverallDifficulty);

            if (isOutOfRange(diff.OverallDifficulty))
                yield return new IssueTemplateOutOfRange(this).Create("Overall difficulty", diff.OverallDifficulty);


            if (hasMoreThanOneDecimalPlace(diff.CircleSize))
                yield return new IssueTemplateMoreThanOneDecimal(this).Create("Circle size", diff.CircleSize);

            if (isOutOfRange(diff.CircleSize))
                yield return new IssueTemplateOutOfRange(this).Create("Circle size", diff.CircleSize);


            if (hasMoreThanOneDecimalPlace(diff.DrainRate))
                yield return new IssueTemplateMoreThanOneDecimal(this).Create("Drain rate", diff.DrainRate);

            if (isOutOfRange(diff.DrainRate))
                yield return new IssueTemplateOutOfRange(this).Create("Drain rate", diff.DrainRate);
        }

        private bool isOutOfRange(float setting)
        {
            return setting < 0f || setting > 10f;
        }

        private bool hasMoreThanOneDecimalPlace(float setting)
        {
            return float.Round(setting, 1) != setting;
        }

        public class IssueTemplateMoreThanOneDecimal : IssueTemplate
        {
            public IssueTemplateMoreThanOneDecimal(ICheck check)
                : base(check, IssueType.Problem, "{0} {1} has more than one decimal place.")
            {
            }

            public Issue Create(string settingName, float settingValue) => new Issue(this, settingName, settingValue);
        }

        public class IssueTemplateOutOfRange : IssueTemplate
        {
            public IssueTemplateOutOfRange(ICheck check)
                : base(check, IssueType.Warning, "{0} is {1} although it is capped between 0 to 10 in-game.")
            {
            }

            public Issue Create(string settingName, float settingValue) => new Issue(this, settingName, settingValue);
        }
    }
}
