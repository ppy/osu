// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Game.Audio;
using osu.Game.Rulesets.Edit.Checks.Components;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Rulesets.Edit.Checks
{
    public class CheckFewHitsounds : ICheck
    {
        /// <summary>
        /// 2 measures (4/4) of 120 BPM, typically makes up a few patterns in the map.
        /// This is almost always ok, but can still be useful for the mapper to make sure hitsounding coverage is good.
        /// </summary>
        private const int negligible_threshold_time = 4000;

        /// <summary>
        /// 4 measures (4/4) of 120 BPM, typically makes up a large portion of a section in the song.
        /// This is ok if the section is a quiet intro, for example.
        /// </summary>
        private const int warning_threshold_time = 8000;

        /// <summary>
        /// 12 measures (4/4) of 120 BPM, typically makes up multiple sections in the song.
        /// </summary>
        private const int problem_threshold_time = 24000;

        // Should pass at least this many objects without hitsounds to be considered an issue (should work for Easy diffs too).
        private const int warning_threshold_objects = 4;
        private const int problem_threshold_objects = 16;

        public CheckMetadata Metadata { get; } = new CheckMetadata(CheckCategory.Audio, "Few or no hitsounds");

        public IEnumerable<IssueTemplate> PossibleTemplates => new IssueTemplate[]
        {
            new IssueTemplateLongPeriodProblem(this),
            new IssueTemplateLongPeriodWarning(this),
            new IssueTemplateLongPeriodNegligible(this),
            new IssueTemplateNoHitsounds(this)
        };

        private bool mapHasHitsounds;
        private int objectsWithoutHitsounds;
        private double lastHitsoundTime;

        public IEnumerable<Issue> Run(BeatmapVerifierContext context)
        {
            if (!context.Beatmap.HitObjects.Any())
                yield break;

            mapHasHitsounds = false;
            objectsWithoutHitsounds = 0;
            lastHitsoundTime = context.Beatmap.HitObjects.First().StartTime;

            var hitObjectsIncludingNested = new List<HitObject>();

            foreach (var hitObject in context.Beatmap.HitObjects)
            {
                // Samples play on the end of objects. Some objects have nested objects to accomplish playing them elsewhere (e.g. slider head/repeat).
                foreach (var nestedHitObject in hitObject.NestedHitObjects)
                    hitObjectsIncludingNested.Add(nestedHitObject);

                hitObjectsIncludingNested.Add(hitObject);
            }

            var hitObjectsByEndTime = hitObjectsIncludingNested.OrderBy(o => o.GetEndTime()).ToList();
            int hitObjectCount = hitObjectsByEndTime.Count;

            for (int i = 0; i < hitObjectCount; ++i)
            {
                var hitObject = hitObjectsByEndTime[i];

                // This is used to perform an update at the end so that the period after the last hitsounded object can be an issue.
                bool isLastObject = i == hitObjectCount - 1;

                foreach (var issue in applyHitsoundUpdate(hitObject, isLastObject))
                    yield return issue;
            }

            if (!mapHasHitsounds)
                yield return new IssueTemplateNoHitsounds(this).Create();
        }

        private IEnumerable<Issue> applyHitsoundUpdate(HitObject hitObject, bool isLastObject = false)
        {
            double time = hitObject.GetEndTime();
            bool hasHitsound = hitObject.Samples.Any(isHitsound);
            bool couldHaveHitsound = hitObject.Samples.Any(isHitnormal);

            // Only generating issues on hitsounded or last objects ensures we get one issue per long period.
            // If there are no hitsounds we let the "No hitsounds" template take precedence.
            if (hasHitsound || (isLastObject && mapHasHitsounds))
            {
                double timeWithoutHitsounds = time - lastHitsoundTime;

                if (timeWithoutHitsounds > problem_threshold_time && objectsWithoutHitsounds > problem_threshold_objects)
                    yield return new IssueTemplateLongPeriodProblem(this).Create(lastHitsoundTime, timeWithoutHitsounds);
                else if (timeWithoutHitsounds > warning_threshold_time && objectsWithoutHitsounds > warning_threshold_objects)
                    yield return new IssueTemplateLongPeriodWarning(this).Create(lastHitsoundTime, timeWithoutHitsounds);
                else if (timeWithoutHitsounds > negligible_threshold_time && objectsWithoutHitsounds > warning_threshold_objects)
                    yield return new IssueTemplateLongPeriodNegligible(this).Create(lastHitsoundTime, timeWithoutHitsounds);
            }

            if (hasHitsound)
            {
                mapHasHitsounds = true;
                objectsWithoutHitsounds = 0;
                lastHitsoundTime = time;
            }
            else if (couldHaveHitsound)
                ++objectsWithoutHitsounds;
        }

        private bool isHitsound(HitSampleInfo sample) => HitSampleInfo.AllAdditions.Any(sample.Name.Contains);
        private bool isHitnormal(HitSampleInfo sample) => sample.Name.Contains(HitSampleInfo.HIT_NORMAL);

        public abstract class IssueTemplateLongPeriod : IssueTemplate
        {
            protected IssueTemplateLongPeriod(ICheck check, IssueType type)
                : base(check, type, "Long period without hitsounds ({0:F1} seconds).")
            {
            }

            public Issue Create(double time, double duration) => new Issue(this, duration / 1000f) { Time = time };
        }

        public class IssueTemplateLongPeriodProblem : IssueTemplateLongPeriod
        {
            public IssueTemplateLongPeriodProblem(ICheck check)
                : base(check, IssueType.Problem)
            {
            }
        }

        public class IssueTemplateLongPeriodWarning : IssueTemplateLongPeriod
        {
            public IssueTemplateLongPeriodWarning(ICheck check)
                : base(check, IssueType.Warning)
            {
            }
        }

        public class IssueTemplateLongPeriodNegligible : IssueTemplateLongPeriod
        {
            public IssueTemplateLongPeriodNegligible(ICheck check)
                : base(check, IssueType.Negligible)
            {
            }
        }

        public class IssueTemplateNoHitsounds : IssueTemplate
        {
            public IssueTemplateNoHitsounds(ICheck check)
                : base(check, IssueType.Problem, "There are no hitsounds.")
            {
            }

            public Issue Create() => new Issue(this);
        }
    }
}
