// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Rulesets.Edit.Checks.Components;

namespace osu.Game.Rulesets.Edit.Checks
{
    public class CheckInconsistentTimingControlPoints : ICheck
    {
        public CheckMetadata Metadata => new CheckMetadata(CheckCategory.Timing, "Inconsistent timing control points", CheckScope.BeatmapSet);

        public IEnumerable<IssueTemplate> PossibleTemplates => new IssueTemplate[]
        {
            new IssueTemplateMissingTimingPoint(this),
            new IssueTemplateExtraTimingPoint(this),
            new IssueTemplateMissingTimingPointMinor(this),
            new IssueTemplateInconsistentMeter(this),
            new IssueTemplateInconsistentBPM(this)
        };

        public IEnumerable<Issue> Run(BeatmapVerifierContext context)
        {
            var difficulties = context.BeatmapsetDifficulties;

            if (difficulties.Count <= 1)
                yield break;

            // Use the current difficulty as reference
            var referenceBeatmap = context.Beatmap;
            var referenceTimingPoints = referenceBeatmap.ControlPointInfo.TimingPoints;

            foreach (var beatmap in difficulties)
            {
                if (beatmap == referenceBeatmap)
                    continue;

                var timingPoints = beatmap.ControlPointInfo.TimingPoints;

                // Check each timing point in the reference against this difficulty
                foreach (var referencePoint in referenceTimingPoints)
                {
                    var matchingPoint = TimingCheckUtils.FindMatchingTimingPoint(timingPoints, referencePoint.Time);
                    var exactMatchingPoint = TimingCheckUtils.FindExactMatchingTimingPoint(timingPoints, referencePoint.Time);

                    if (matchingPoint == null)
                    {
                        yield return new IssueTemplateMissingTimingPoint(this).Create(referencePoint.Time, beatmap.BeatmapInfo.DifficultyName);
                    }
                    else
                    {
                        // Check for meter signature inconsistency
                        if (!referencePoint.TimeSignature.Equals(matchingPoint.TimeSignature))
                        {
                            yield return new IssueTemplateInconsistentMeter(this).Create(referencePoint.Time, beatmap.BeatmapInfo.DifficultyName);
                        }

                        // Check for BPM inconsistency
                        if (Math.Abs(referencePoint.BeatLength - matchingPoint.BeatLength) > TimingCheckUtils.TIME_OFFSET_TOLERANCE_MS)
                        {
                            yield return new IssueTemplateInconsistentBPM(this).Create(referencePoint.Time, beatmap.BeatmapInfo.DifficultyName);
                        }

                        // Check for exact timing match (decimal precision)
                        if (exactMatchingPoint == null)
                        {
                            yield return new IssueTemplateMissingTimingPointMinor(this).Create(referencePoint.Time, beatmap.BeatmapInfo.DifficultyName);
                        }
                    }
                }

                // Check timing points in this difficulty that aren't in the reference
                foreach (var timingPoint in timingPoints)
                {
                    var matchingReferencePoint = TimingCheckUtils.FindMatchingTimingPoint(referenceTimingPoints, timingPoint.Time);
                    var exactMatchingReferencePoint = TimingCheckUtils.FindExactMatchingTimingPoint(referenceTimingPoints, timingPoint.Time);

                    if (matchingReferencePoint == null)
                    {
                        yield return new IssueTemplateExtraTimingPoint(this).Create(timingPoint.Time, beatmap.BeatmapInfo.DifficultyName);
                    }
                    else if (exactMatchingReferencePoint == null)
                    {
                        yield return new IssueTemplateMissingTimingPointMinor(this).Create(timingPoint.Time, beatmap.BeatmapInfo.DifficultyName);
                    }
                }
            }
        }

        public class IssueTemplateMissingTimingPoint : IssueTemplate
        {
            public IssueTemplateMissingTimingPoint(ICheck check)
                : base(check, IssueType.Problem, "Missing timing control point in {0}.")
            {
            }

            public Issue Create(double time, string difficultyName)
                => new Issue(time, this, difficultyName);
        }

        public class IssueTemplateExtraTimingPoint : IssueTemplate
        {
            public IssueTemplateExtraTimingPoint(ICheck check)
                : base(check, IssueType.Problem, "Extra timing control point in {0}.")
            {
            }

            public Issue Create(double time, string difficultyName)
                => new Issue(time, this, difficultyName);
        }

        public class IssueTemplateMissingTimingPointMinor : IssueTemplate
        {
            public IssueTemplateMissingTimingPointMinor(ICheck check)
                : base(check, IssueType.Negligible, "Timing control point has decimally different offset in {0}.")
            {
            }

            public Issue Create(double time, string difficultyName)
                => new Issue(time, this, difficultyName);
        }

        public class IssueTemplateInconsistentMeter : IssueTemplate
        {
            public IssueTemplateInconsistentMeter(ICheck check)
                : base(check, IssueType.Problem, "Inconsistent time signature in {0}.")
            {
            }

            public Issue Create(double time, string difficultyName)
                => new Issue(time, this, difficultyName);
        }

        public class IssueTemplateInconsistentBPM : IssueTemplate
        {
            public IssueTemplateInconsistentBPM(ICheck check)
                : base(check, IssueType.Problem, "Inconsistent BPM in {0}.")
            {
            }

            public Issue Create(double time, string difficultyName)
                => new Issue(time, this, difficultyName);
        }
    }
}
