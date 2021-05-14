// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Rulesets.Edit.Checks.Components;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;

namespace osu.Game.Rulesets.Edit.Checks
{
    public class CheckConcurrentObjects : ICheck
    {
        // We guarantee that the objects are either treated as concurrent or unsnapped when near the same beat divisor.
        private const double ms_leniency = CheckUnsnappedObjects.UNSNAP_MS_THRESHOLD;

        public CheckMetadata Metadata { get; } = new CheckMetadata(CheckCategory.Compose, "Concurrent hitobjects");

        public IEnumerable<IssueTemplate> PossibleTemplates => new IssueTemplate[]
        {
            new IssueTemplateConcurrentSame(this),
            new IssueTemplateConcurrentDifferent(this)
        };

        public IEnumerable<Issue> Run(BeatmapVerifierContext context)
        {
            var hitObjects = context.Beatmap.HitObjects;

            for (int i = 0; i < hitObjects.Count - 1; ++i)
            {
                var hitobject = hitObjects[i];

                for (int j = i + 1; j < hitObjects.Count; ++j)
                {
                    var nextHitobject = hitObjects[j];

                    // Accounts for rulesets with hitobjects separated by columns, such as Mania.
                    // In these cases we only care about concurrent objects within the same column.
                    if ((hitobject as IHasColumn)?.Column != (nextHitobject as IHasColumn)?.Column)
                        continue;

                    // Two hitobjects cannot be concurrent without also being concurrent with all objects in between.
                    // So if the next object is not concurrent, then we know no future objects will be either.
                    if (!areConcurrent(hitobject, nextHitobject))
                        break;

                    if (hitobject.GetType() == nextHitobject.GetType())
                        yield return new IssueTemplateConcurrentSame(this).Create(hitobject, nextHitobject);
                    else
                        yield return new IssueTemplateConcurrentDifferent(this).Create(hitobject, nextHitobject);
                }
            }
        }

        private bool areConcurrent(HitObject hitobject, HitObject nextHitobject) => nextHitobject.StartTime <= hitobject.GetEndTime() + ms_leniency;

        public abstract class IssueTemplateConcurrent : IssueTemplate
        {
            protected IssueTemplateConcurrent(ICheck check, string unformattedMessage)
                : base(check, IssueType.Problem, unformattedMessage)
            {
            }

            public Issue Create(HitObject hitobject, HitObject nextHitobject)
            {
                var hitobjects = new List<HitObject> { hitobject, nextHitobject };
                return new Issue(hitobjects, this, hitobject.GetType().Name, nextHitobject.GetType().Name)
                {
                    Time = nextHitobject.StartTime
                };
            }
        }

        public class IssueTemplateConcurrentSame : IssueTemplateConcurrent
        {
            public IssueTemplateConcurrentSame(ICheck check)
                : base(check, "{0}s are concurrent here.")
            {
            }
        }

        public class IssueTemplateConcurrentDifferent : IssueTemplateConcurrent
        {
            public IssueTemplateConcurrentDifferent(ICheck check)
                : base(check, "{0} and {1} are concurrent here.")
            {
            }
        }
    }
}
