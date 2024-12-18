// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Edit.Checks.Components;

namespace osu.Game.Rulesets.Edit.Checks
{
    public class CheckDrainLength : ICheck
    {
        private const int min_drain_threshold = 30 * 1000;

        public CheckMetadata Metadata => new CheckMetadata(CheckCategory.Compose, "Drain length is too short");

        public IEnumerable<IssueTemplate> PossibleTemplates => new IssueTemplate[]
        {
            new IssueTemplateTooShort(this)
        };

        public IEnumerable<Issue> Run(BeatmapVerifierContext context)
        {
            double drainTime = context.Beatmap.CalculateDrainLength();

            if (drainTime < min_drain_threshold)
                yield return new IssueTemplateTooShort(this).Create((int)(drainTime / 1000));
        }

        public class IssueTemplateTooShort : IssueTemplate
        {
            public IssueTemplateTooShort(ICheck check)
                : base(check, IssueType.Problem, "Less than 30 seconds of drain time, currently {0}.")
            {
            }

            public Issue Create(int drainTimeSeconds) => new Issue(this, drainTimeSeconds);
        }
    }
}
