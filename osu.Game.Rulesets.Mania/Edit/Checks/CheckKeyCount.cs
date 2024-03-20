// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Checks.Components;

namespace osu.Game.Rulesets.Mania.Edit.Checks
{
    public class CheckKeyCount : ICheck
    {
        public CheckMetadata Metadata => new CheckMetadata(CheckCategory.Settings, "Check mania keycount.");

        public IEnumerable<IssueTemplate> PossibleTemplates => new IssueTemplate[]
        {
            new IssueTemplateKeycountTooLow(this),
        };

        public IEnumerable<Issue> Run(BeatmapVerifierContext context)
        {
            var diff = context.Beatmap.Difficulty;

            if (diff.CircleSize < 4)
            {
                yield return new IssueTemplateKeycountTooLow(this).Create(diff.CircleSize);
            }
        }

        public class IssueTemplateKeycountTooLow : IssueTemplate
        {
            public IssueTemplateKeycountTooLow(ICheck check)
                : base(check, IssueType.Problem, "Key count is {0} and must be 4 or higher.")
            {
            }

            public Issue Create(float current) => new Issue(this, current);
        }
    }
}
