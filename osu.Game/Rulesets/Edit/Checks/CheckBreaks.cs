// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps.Timing;
using osu.Game.Rulesets.Edit.Checks.Components;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Rulesets.Edit.Checks
{
    public class CheckBreaks : ICheck
    {
        // Breaks may be off by 1 ms.
        private const int leniency_threshold = 1;
        private const double minimum_gap_before_break = 200;

        // Break end time depends on the upcoming object's pre-empt time.
        // As things stand, "pre-empt time" is only defined for osu! standard
        // This is a generic value representing AR=10
        // Relevant: https://github.com/ppy/osu/issues/14330#issuecomment-1002158551
        private const double min_end_threshold = 450;
        public CheckMetadata Metadata => new CheckMetadata(CheckCategory.Events, "Breaks not achievable using the editor");

        public IEnumerable<IssueTemplate> PossibleTemplates => new IssueTemplate[]
        {
            new IssueTemplateEarlyStart(this),
            new IssueTemplateLateEnd(this),
            new IssueTemplateTooShort(this)
        };

        public IEnumerable<Issue> Run(BeatmapVerifierContext context)
        {
            var startTimes = context.Beatmap.HitObjects.Select(ho => ho.StartTime).Order().ToList();
            var endTimes = context.Beatmap.HitObjects.Select(ho => ho.GetEndTime()).Order().ToList();

            foreach (var breakPeriod in context.Beatmap.Breaks)
            {
                if (breakPeriod.Duration < BreakPeriod.MIN_BREAK_DURATION)
                    yield return new IssueTemplateTooShort(this).Create(breakPeriod.StartTime);

                int previousObjectEndTimeIndex = endTimes.BinarySearch(breakPeriod.StartTime);
                if (previousObjectEndTimeIndex < 0) previousObjectEndTimeIndex = ~previousObjectEndTimeIndex - 1;

                if (previousObjectEndTimeIndex >= 0)
                {
                    double gapBeforeBreak = breakPeriod.StartTime - endTimes[previousObjectEndTimeIndex];
                    if (gapBeforeBreak < minimum_gap_before_break - leniency_threshold)
                        yield return new IssueTemplateEarlyStart(this).Create(breakPeriod.StartTime, minimum_gap_before_break - gapBeforeBreak);
                }

                int nextObjectStartTimeIndex = startTimes.BinarySearch(breakPeriod.EndTime);
                if (nextObjectStartTimeIndex < 0) nextObjectStartTimeIndex = ~nextObjectStartTimeIndex;

                if (nextObjectStartTimeIndex < startTimes.Count)
                {
                    double gapAfterBreak = startTimes[nextObjectStartTimeIndex] - breakPeriod.EndTime;
                    if (gapAfterBreak < min_end_threshold - leniency_threshold)
                        yield return new IssueTemplateLateEnd(this).Create(breakPeriod.StartTime, min_end_threshold - gapAfterBreak);
                }
            }
        }

        public class IssueTemplateEarlyStart : IssueTemplate
        {
            public IssueTemplateEarlyStart(ICheck check)
                : base(check, IssueType.Problem, "Break starts {0} ms early.")
            {
            }

            public Issue Create(double startTime, double diff) => new Issue(startTime, this, (int)diff);
        }

        public class IssueTemplateLateEnd : IssueTemplate
        {
            public IssueTemplateLateEnd(ICheck check)
                : base(check, IssueType.Problem, "Break ends {0} ms late.")
            {
            }

            public Issue Create(double startTime, double diff) => new Issue(startTime, this, (int)diff);
        }

        public class IssueTemplateTooShort : IssueTemplate
        {
            public IssueTemplateTooShort(ICheck check)
                : base(check, IssueType.Warning, "Break is non-functional due to being less than {0} ms.")
            {
            }

            public Issue Create(double startTime) => new Issue(startTime, this, BreakPeriod.MIN_BREAK_DURATION);
        }
    }
}
