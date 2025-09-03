// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Rulesets.Edit.Checks.Components;
using osu.Game.Rulesets.Objects;
using System;

namespace osu.Game.Rulesets.Edit.Checks
{
    public class CheckConcurrentObjects : ICheck
    {
        // We guarantee that the objects are either treated as concurrent or unsnapped when near the same beat divisor.
        private const double ms_leniency = CheckUnsnappedObjects.UNSNAP_MS_THRESHOLD;
        private const double almost_concurrent_threshold = 10.0;

        public CheckMetadata Metadata { get; } = new CheckMetadata(CheckCategory.Compose, "Concurrent hitobjects");

        public IEnumerable<IssueTemplate> PossibleTemplates => new IssueTemplate[]
        {
            new IssueTemplateConcurrent(this),
            new IssueTemplateAlmostConcurrent(this)
        };

        public virtual IEnumerable<Issue> Run(BeatmapVerifierContext context)
        {
            var hitObjects = context.CurrentDifficulty.Playable.HitObjects;

            for (int i = 0; i < hitObjects.Count - 1; ++i)
            {
                var hitobject = hitObjects[i];

                for (int j = i + 1; j < hitObjects.Count; ++j)
                {
                    var nextHitobject = hitObjects[j];

                    // Two hitobjects cannot be concurrent without also being concurrent with all objects in between.
                    // So if the next object is not concurrent or almost concurrent, then we know no future objects will be either.
                    if (!AreConcurrent(hitobject, nextHitobject) && !AreAlmostConcurrent(hitobject, nextHitobject))
                        break;

                    if (AreConcurrent(hitobject, nextHitobject))
                    {
                        yield return new IssueTemplateConcurrent(this).Create(hitobject, nextHitobject);
                    }
                    else if (AreAlmostConcurrent(hitobject, nextHitobject))
                    {
                        yield return new IssueTemplateAlmostConcurrent(this).Create(hitobject, nextHitobject);
                    }
                }
            }
        }

        protected bool AreConcurrent(HitObject hitobject, HitObject nextHitobject) => nextHitobject.StartTime <= hitobject.GetEndTime() + ms_leniency;

        protected bool AreAlmostConcurrent(HitObject hitobject, HitObject nextHitobject) =>
            Math.Abs(nextHitobject.StartTime - hitobject.GetEndTime()) < almost_concurrent_threshold;

        public class IssueTemplateConcurrent : IssueTemplate
        {
            public IssueTemplateConcurrent(ICheck check)
                : base(check, IssueType.Problem, "{0}")
            {
            }

            public Issue Create(HitObject hitobject, HitObject nextHitobject)
            {
                var hitobjects = new List<HitObject> { hitobject, nextHitobject };
                string message = hitobject.GetType() == nextHitobject.GetType()
                    ? $"{hitobject.GetType().Name}s are concurrent here."
                    : $"{hitobject.GetType().Name} and {nextHitobject.GetType().Name} are concurrent here.";

                return new Issue(hitobjects, this, message)
                {
                    Time = nextHitobject.StartTime
                };
            }
        }

        public class IssueTemplateAlmostConcurrent : IssueTemplate
        {
            public IssueTemplateAlmostConcurrent(ICheck check)
                : base(check, IssueType.Problem, "{0}")
            {
            }

            public Issue Create(HitObject hitobject, HitObject nextHitobject)
            {
                var hitobjects = new List<HitObject> { hitobject, nextHitobject };
                string message = hitobject.GetType() == nextHitobject.GetType()
                    ? $"{hitobject.GetType().Name}s are less than {almost_concurrent_threshold}ms apart."
                    : $"{hitobject.GetType().Name} and {nextHitobject.GetType().Name} are less than {almost_concurrent_threshold}ms apart.";

                return new Issue(hitobjects, this, message)
                {
                    Time = nextHitobject.StartTime
                };
            }
        }
    }
}
