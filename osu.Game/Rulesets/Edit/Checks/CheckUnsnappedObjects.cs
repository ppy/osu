// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Rulesets.Edit.Checks.Components;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;

namespace osu.Game.Rulesets.Edit.Checks
{
    public class CheckUnsnappedObjects : ICheck
    {
        public const double UNSNAP_MS_THRESHOLD = 2;

        public CheckMetadata Metadata { get; } = new CheckMetadata(CheckCategory.Timing, "Unsnapped hitobjects");

        public IEnumerable<IssueTemplate> PossibleTemplates => new IssueTemplate[]
        {
            new IssueTemplateLargeUnsnap(this),
            new IssueTemplateSmallUnsnap(this)
        };

        public IEnumerable<Issue> Run(BeatmapVerifierContext context)
        {
            var controlPointInfo = context.Beatmap.ControlPointInfo;

            foreach (var hitobject in context.Beatmap.HitObjects)
            {
                double startUnsnap = hitobject.StartTime - controlPointInfo.GetClosestSnappedTime(hitobject.StartTime);
                string startPostfix = hitobject is IHasDuration ? "start" : "";
                foreach (var issue in getUnsnapIssues(hitobject, startUnsnap, hitobject.StartTime, startPostfix))
                    yield return issue;

                if (hitobject is IHasRepeats hasRepeats)
                {
                    for (int repeatIndex = 0; repeatIndex < hasRepeats.RepeatCount; ++repeatIndex)
                    {
                        double spanDuration = hasRepeats.Duration / (hasRepeats.RepeatCount + 1);
                        double repeatTime = hitobject.StartTime + spanDuration * (repeatIndex + 1);
                        double repeatUnsnap = repeatTime - controlPointInfo.GetClosestSnappedTime(repeatTime);
                        foreach (var issue in getUnsnapIssues(hitobject, repeatUnsnap, repeatTime, "repeat"))
                            yield return issue;
                    }
                }

                if (hitobject is IHasDuration hasDuration)
                {
                    double endUnsnap = hasDuration.EndTime - controlPointInfo.GetClosestSnappedTime(hasDuration.EndTime);
                    foreach (var issue in getUnsnapIssues(hitobject, endUnsnap, hasDuration.EndTime, "end"))
                        yield return issue;
                }
            }
        }

        private IEnumerable<Issue> getUnsnapIssues(HitObject hitobject, double unsnap, double time, string postfix = "")
        {
            if (Math.Abs(unsnap) >= UNSNAP_MS_THRESHOLD)
                yield return new IssueTemplateLargeUnsnap(this).Create(hitobject, unsnap, time, postfix);
            else if (Math.Abs(unsnap) >= 1)
                yield return new IssueTemplateSmallUnsnap(this).Create(hitobject, unsnap, time, postfix);

            // We don't care about unsnaps < 1 ms, as all object ends have these due to the way SV works.
        }

        public abstract class IssueTemplateUnsnap : IssueTemplate
        {
            protected IssueTemplateUnsnap(ICheck check, IssueType type)
                : base(check, type, "{0} is unsnapped by {1:0.##} ms.")
            {
            }

            public Issue Create(HitObject hitobject, double unsnap, double time, string postfix = "")
            {
                string objectName = hitobject.GetType().Name;
                if (!string.IsNullOrEmpty(postfix))
                    objectName += " " + postfix;

                return new Issue(hitobject, this, objectName, unsnap) { Time = time };
            }
        }

        public class IssueTemplateLargeUnsnap : IssueTemplateUnsnap
        {
            public IssueTemplateLargeUnsnap(ICheck check)
                : base(check, IssueType.Problem)
            {
            }
        }

        public class IssueTemplateSmallUnsnap : IssueTemplateUnsnap
        {
            public IssueTemplateSmallUnsnap(ICheck check)
                : base(check, IssueType.Negligible)
            {
            }
        }
    }
}
