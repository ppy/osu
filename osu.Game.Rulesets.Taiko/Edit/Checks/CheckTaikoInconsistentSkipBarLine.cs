// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Checks.Components;

namespace osu.Game.Rulesets.Taiko.Edit.Checks
{
    public class CheckTaikoInconsistentSkipBarLine : ICheck
    {
        public CheckMetadata Metadata => new CheckMetadata(CheckCategory.Timing, "Inconsistent \"Skip Bar Line\" setting", CheckScope.BeatmapSet);

        public IEnumerable<IssueTemplate> PossibleTemplates => new IssueTemplate[]
        {
            new IssueTemplateInconsistentOmitFirstBarLine(this)
        };

        public IEnumerable<Issue> Run(BeatmapVerifierContext context)
        {
            if (context.AllDifficulties.Count() <= 1)
                yield break;

            // Inconsistent bar line omission only matters for osu!taiko difficulties, so only check those
            var taikoBeatmaps = context.AllDifficulties.Where(b => b.Playable.BeatmapInfo.Ruleset.ShortName == "taiko").ToList();

            if (taikoBeatmaps.Count <= 1)
                yield break;

            var referenceBeatmap = context.CurrentDifficulty.Playable;
            var referenceTimingPoints = referenceBeatmap.ControlPointInfo.TimingPoints;

            var otherTaikoBeatmaps = taikoBeatmaps.Where(b => b.Playable != referenceBeatmap).ToList();

            foreach (var beatmap in otherTaikoBeatmaps)
            {
                var timingPoints = beatmap.Playable.ControlPointInfo.TimingPoints;

                foreach (var referencePoint in referenceTimingPoints)
                {
                    var matchingPoint = TimingCheckUtils.FindExactMatchingTimingPoint(timingPoints, referencePoint.Time);

                    if (matchingPoint == null)
                        // Inconsistent timing points - that's handled by `CheckInconsistentTimingControlPoints`, so skip
                        continue;

                    if (referencePoint.OmitFirstBarLine != matchingPoint.OmitFirstBarLine)
                    {
                        yield return new IssueTemplateInconsistentOmitFirstBarLine(this).Create(referencePoint.Time, beatmap.Playable.BeatmapInfo.DifficultyName);
                    }
                }
            }
        }

        public class IssueTemplateInconsistentOmitFirstBarLine : IssueTemplate
        {
            public IssueTemplateInconsistentOmitFirstBarLine(ICheck check)
                : base(check, IssueType.Problem, "Inconsistent \"Skip Bar Line\" setting in {0}.")
            {
            }

            public Issue Create(double time, string difficultyName)
                => new Issue(time, this, difficultyName);
        }
    }
}
