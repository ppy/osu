// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Edit.Checks.Components;
using osu.Game.Rulesets.Osu.Objects;
using osuTK;

namespace osu.Game.Rulesets.Osu.Edit.Checks
{
    public class CheckOffscreenObjects : Check
    {
        // These are close approximates to the edges of the screen
        // in gameplay on a 4:3 aspect ratio for osu!stable.
        private const int min_x = -67;
        private const int min_y = -60;
        private const int max_x = 579;
        private const int max_y = 428;

        // The amount of milliseconds to step through a slider path at a time
        // (higher = more performant, but higher false-negative chance).
        private const int path_step_size = 5;

        public override CheckMetadata Metadata { get; } = new CheckMetadata
        (
            category: CheckCategory.Compose,
            description: "Offscreen hitobjects."
        );

        public override IEnumerable<IssueTemplate> PossibleTemplates => new[]
        {
            templateOffscreen,
            templateOffscreenSliderPath
        };

        private readonly IssueTemplate templateOffscreen = new IssueTemplate
        (
            type: IssueType.Problem,
            unformattedMessage: "This object goes offscreen on a 4:3 aspect ratio."
        );

        private readonly IssueTemplate templateOffscreenSliderPath = new IssueTemplate
        (
            type: IssueType.Problem,
            unformattedMessage: "This slider goes offscreen here on a 4:3 aspect ratio."
        );

        public override IEnumerable<Issue> Run(IBeatmap beatmap)
        {
            foreach (var hitobject in beatmap.HitObjects)
            {
                switch (hitobject)
                {
                    case Slider slider:
                    {
                        foreach (var issue in sliderIssues(slider))
                            yield return issue;

                        break;
                    }

                    case HitCircle circle:
                    {
                        if (isOffscreen(circle.StackedPosition, circle.Radius))
                            yield return new Issue(circle, templateOffscreen);

                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Steps through points on the slider to ensure the entire path is on-screen.
        /// Returns at most one issue.
        /// </summary>
        /// <param name="slider">The slider whose path to check.</param>
        /// <returns></returns>
        private IEnumerable<Issue> sliderIssues(Slider slider)
        {
            for (int i = 0; i < slider.Distance; i += path_step_size)
            {
                double progress = i / slider.Distance;
                Vector2 position = slider.StackedPositionAt(progress);

                if (!isOffscreen(position, slider.Radius))
                    continue;

                // `SpanDuration` ensures we don't include reverses.
                double time = slider.StartTime + progress * slider.SpanDuration;
                yield return new Issue(slider, templateOffscreenSliderPath) { Time = time };

                yield break;
            }

            // Above loop may skip the last position in the slider due to step size.
            if (!isOffscreen(slider.StackedEndPosition, slider.Radius))
                yield break;

            yield return new Issue(slider, templateOffscreenSliderPath) { Time = slider.EndTime };
        }

        private bool isOffscreen(Vector2 position, double radius)
        {
            return position.X - radius < min_x || position.X + radius > max_x ||
                   position.Y - radius < min_y || position.Y + radius > max_y;
        }
    }
}
