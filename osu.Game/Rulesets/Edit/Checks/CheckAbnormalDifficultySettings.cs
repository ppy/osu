// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using osu.Game.Rulesets.Edit.Checks.Components;

namespace osu.Game.Rulesets.Edit.Checks
{
    public abstract class CheckAbnormalDifficultySettings : ICheck
    {
        public abstract CheckMetadata Metadata { get; }

        public IEnumerable<IssueTemplate> PossibleTemplates => new IssueTemplate[]
        {
            new IssueTemplateMoreThanOneDecimal(this),
            new IssueTemplateOutOfRange(this),
        };

        public abstract IEnumerable<Issue> Run(BeatmapVerifierContext context);

        /// <summary>
        /// If the setting is out of the boundaries set by the editor (0 - 10)
        /// </summary>
        protected bool OutOfRange(string setting, float value, [NotNullWhen(true)] out Issue? issue)
        {
            bool hasIssue = value < 0f || value > 10f;
            issue = hasIssue ? new IssueTemplateOutOfRange(this).Create(setting, value) : null;
            return hasIssue;
        }

        protected bool HasMoreThanOneDecimalPlace(string setting, float value, [NotNullWhen(true)] out Issue? issue)
        {
            bool hasIssue = float.Round(value, 1) != value;
            issue = hasIssue ? new IssueTemplateMoreThanOneDecimal(this).Create(setting, value) : null;
            return hasIssue;
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
