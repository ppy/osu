// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Edit.Checks.Components;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;

namespace osu.Game.Rulesets.Edit.Checks
{
    public class CheckUnsnaps : ICheck
    {
        private const double unsnap_ms_threshold = 2;

        private static readonly int[] greatest_common_divisors = { 16, 12, 9, 7, 5 };

        public CheckMetadata Metadata { get; } = new CheckMetadata(CheckCategory.Compose, "Unsnapped hitobjects");

        public IEnumerable<IssueTemplate> PossibleTemplates => new IssueTemplate[]
        {
            new IssueTemplate2MsOrMore(this),
            new IssueTemplate1MsOrMore(this)
        };

        public IEnumerable<Issue> Run(IBeatmap playableBeatmap, IWorkingBeatmap workingBeatmap)
        {
            foreach (var hitobject in playableBeatmap.HitObjects)
            {
                double startUnsnap = hitobject.StartTime - closestSnapTime(playableBeatmap, hitobject.StartTime);
                string startPostfix = hitobject is IHasDuration ? "start" : "";
                foreach (var issue in getUnsnapIssues(hitobject, startUnsnap, hitobject.StartTime, startPostfix))
                    yield return issue;

                if (hitobject is IHasRepeats hasRepeats)
                {
                    for (int repeatIndex = 0; repeatIndex < hasRepeats.RepeatCount; ++repeatIndex)
                    {
                        double spanDuration = hasRepeats.Duration / (hasRepeats.RepeatCount + 1);
                        double repeatTime = hitobject.StartTime + spanDuration * (repeatIndex + 1);
                        double repeatUnsnap = repeatTime - closestSnapTime(playableBeatmap, repeatTime);
                        foreach (var issue in getUnsnapIssues(hitobject, repeatUnsnap, repeatTime, "repeat"))
                            yield return issue;
                    }
                }

                if (hitobject is IHasDuration hasDuration)
                {
                    double endUnsnap = hasDuration.EndTime - closestSnapTime(playableBeatmap, hasDuration.EndTime);
                    foreach (var issue in getUnsnapIssues(hitobject, endUnsnap, hasDuration.EndTime, "end"))
                        yield return issue;
                }
            }
        }

        private IEnumerable<Issue> getUnsnapIssues(HitObject hitobject, double unsnap, double time, string postfix = "")
        {
            if (Math.Abs(unsnap) >= unsnap_ms_threshold)
                yield return new IssueTemplate2MsOrMore(this).Create(hitobject, unsnap, time, postfix);
            else if (Math.Abs(unsnap) >= 1)
                yield return new IssueTemplate1MsOrMore(this).Create(hitobject, unsnap, time, postfix);

            // We don't care about unsnaps < 1 ms, as all object ends have these due to the way SV works.
        }

        private int closestSnapTime(IBeatmap playableBeatmap, double time)
        {
            var timingPoint = playableBeatmap.ControlPointInfo.TimingPointAt(time);
            double smallestUnsnap = greatest_common_divisors.Select(divisor => Math.Abs(time - snapTime(timingPoint, time, divisor))).Min();

            return (int)Math.Round(time + smallestUnsnap);
        }

        private int snapTime(TimingControlPoint timingPoint, double time, int beatDivisor)
        {
            double beatLength = timingPoint.BeatLength / beatDivisor;
            int beatLengths = (int)Math.Round((time - timingPoint.Time) / beatLength, MidpointRounding.AwayFromZero);

            // Casting to int matches the editor in both stable and lazer.
            return (int)(timingPoint.Time + beatLengths * beatLength);
        }

        public abstract class IssueTemplateUnsnap : IssueTemplate
        {
            protected IssueTemplateUnsnap(ICheck check, IssueType type)
                : base(check, type, "{0:0.##} is unsnapped by {1:0.##} ms.")
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

        public class IssueTemplate2MsOrMore : IssueTemplateUnsnap
        {
            public IssueTemplate2MsOrMore(ICheck check)
                : base(check, IssueType.Problem)
            {
            }
        }

        public class IssueTemplate1MsOrMore : IssueTemplateUnsnap
        {
            public IssueTemplate1MsOrMore(ICheck check)
                : base(check, IssueType.Negligible)
            {
            }
        }
    }
}
