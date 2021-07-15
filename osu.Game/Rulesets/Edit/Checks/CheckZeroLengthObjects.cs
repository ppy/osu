// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Rulesets.Edit.Checks.Components;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;

namespace osu.Game.Rulesets.Edit.Checks
{
    public class CheckZeroLengthObjects : ICheck
    {
        /// <summary>
        /// The duration can be this low before being treated as having no length, in case of precision errors. Unit is milliseconds.
        /// </summary>
        private const double leniency = 0.5d;

        public CheckMetadata Metadata { get; } = new CheckMetadata(CheckCategory.Compose, "Zero-length hitobjects");

        public IEnumerable<IssueTemplate> PossibleTemplates => new IssueTemplate[]
        {
            new IssueTemplateZeroLength(this)
        };

        public IEnumerable<Issue> Run(BeatmapVerifierContext context)
        {
            foreach (var hitObject in context.Beatmap.HitObjects)
            {
                if (!(hitObject is IHasDuration hasDuration))
                    continue;

                if (hasDuration.Duration < leniency)
                    yield return new IssueTemplateZeroLength(this).Create(hitObject, hasDuration.Duration);
            }
        }

        public class IssueTemplateZeroLength : IssueTemplate
        {
            public IssueTemplateZeroLength(ICheck check)
                : base(check, IssueType.Problem, "{0} has a duration of {1:0}.")
            {
            }

            public Issue Create(HitObject hitobject, double duration) => new Issue(hitobject, this, hitobject.GetType(), duration);
        }
    }
}
