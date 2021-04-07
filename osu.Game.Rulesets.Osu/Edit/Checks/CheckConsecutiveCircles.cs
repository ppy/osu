// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Screens.Edit.Verify;
using osu.Game.Screens.Edit.Verify.Components;

namespace osu.Game.Rulesets.Osu.Edit.Checks
{
    public class CheckConsecutiveCircles : BeatmapCheck
    {
        private const double consecutive_threshold = 3;
        private const double delta_time_min_expected = 300;
        private const double delta_time_min_threshold = 100;

        public override CheckMetadata Metadata() => new CheckMetadata
        (
            category: CheckMetadata.CheckCategory.Spread,
            description: "Too many or fast consecutive circles."
        );

        private IssueTemplate templateManyInARow = new IssueTemplate
        (
            type: IssueTemplate.IssueType.Problem,
            unformattedMessage: "There are {0} circles in a row here, expected at most {1}."
        );

        private IssueTemplate templateTooFast = new IssueTemplate
        (
            type: IssueTemplate.IssueType.Warning,
            unformattedMessage: "These circles are too fast ({0:0} ms), expected at least {1:0} ms."
        );

        private IssueTemplate templateAlmostTooFast = new IssueTemplate
        (
            type: IssueTemplate.IssueType.Negligible,
            unformattedMessage: "These circles are almost too fast ({0:0} ms), expected at least {1:0} ms."
        );

        public override IEnumerable<IssueTemplate> Templates() => new[]
        {
            templateManyInARow,
            templateTooFast,
            templateAlmostTooFast
        };

        public override IEnumerable<Issue> Run(IBeatmap beatmap)
        {
            List<HitCircle> prevCircles = new List<HitCircle>();

            foreach (HitObject hitobject in beatmap.HitObjects)
            {
                if (!(hitobject is HitCircle circle) || hitobject == beatmap.HitObjects.Last())
                {
                    if (prevCircles.Count > consecutive_threshold)
                    {
                        yield return new Issue(
                            prevCircles,
                            templateManyInARow,
                            prevCircles.Count, consecutive_threshold
                        );
                    }

                    prevCircles.Clear();
                    continue;
                }

                double? prevDeltaTime = circle.StartTime - prevCircles.LastOrDefault()?.StartTime;
                prevCircles.Add(circle);

                if (prevDeltaTime == null || prevDeltaTime >= delta_time_min_expected)
                    continue;

                yield return new Issue(
                    prevCircles.TakeLast(2),
                    prevDeltaTime < delta_time_min_threshold ? templateTooFast : templateAlmostTooFast,
                    prevDeltaTime, delta_time_min_expected
                );
            }
        }
    }
}
