// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Beatmaps.Timing;
using osu.Game.Rulesets.Edit.Checks.Components;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Rulesets.Edit.Checks
{
    public class CheckBreaks : ICheck
    {
        // Breaks may be off by 1 ms.
        private const int leniency_threshold = 1;
        private const double min_start_threshold = 200;

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
            foreach (var breakPeriod in context.Beatmap.Breaks)
            {
                if (breakPeriod.Duration < BreakPeriod.MIN_BREAK_DURATION)
                    yield return new IssueTemplateTooShort(this).Create(breakPeriod.StartTime);
            }

            foreach (var hitObject in context.Beatmap.HitObjects)
            {
                foreach (var breakPeriod in context.Beatmap.Breaks)
                {
                    double diffStart = breakPeriod.StartTime - hitObject.GetEndTime();
                    double diffEnd = hitObject.StartTime - breakPeriod.EndTime;

                    if (diffStart < min_start_threshold - leniency_threshold && diffStart > 0)
                        yield return new IssueTemplateEarlyStart(this).Create(breakPeriod.StartTime, min_start_threshold - diffStart);
                    else if (diffEnd < min_end_threshold - leniency_threshold && diffEnd > 0)
                        yield return new IssueTemplateLateEnd(this).Create(breakPeriod.StartTime, min_end_threshold - diffEnd);
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
                : base(check, IssueType.Warning, "Break is non-functional due to being less than 650ms.")
            {
            }

            public Issue Create(double startTime) => new Issue(startTime, this);
        }
    }
}
