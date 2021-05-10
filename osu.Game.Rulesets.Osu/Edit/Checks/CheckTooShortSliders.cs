// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Checks.Components;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Edit.Checks
{
    public class CheckTooShortSliders : ICheck
    {
        // The Ranking Criteria discourages < 1/2 for 180 BPM Easy difficulties.
        // This check adds some leniency before warning, as for example 185 BPM 1/2 is a non-issue.
        private const int problem_threshold = 125; // 240 BPM 1/2
        private const int warning_threshold = 150; // 200 BPM 1/2
        private const int expected_duration = 167; // 180 BPM 1/2

        public CheckMetadata Metadata { get; } = new CheckMetadata(CheckCategory.Spread, "Too short sliders");

        public IEnumerable<IssueTemplate> PossibleTemplates => new IssueTemplate[]
        {
            new IssueTemplateVeryShort(this),
            new IssueTemplateTooShort(this)
        };

        public IEnumerable<Issue> Run(IBeatmap beatmap, IBeatmapVerifier.Context context)
        {
            if (context.InterpretedDifficulty.Value != DifficultyRating.Easy)
                yield break;

            foreach (var hitobject in beatmap.HitObjects)
            {
                if (!(hitobject is Slider slider))
                    continue;

                if (slider.SpanDuration < problem_threshold)
                    yield return new IssueTemplateTooShort(this).Create(slider, slider.SpanDuration);
                else if (slider.SpanDuration < warning_threshold)
                    yield return new IssueTemplateVeryShort(this).Create(slider, slider.SpanDuration);
            }
        }

        public class IssueTemplateVeryShort : IssueTemplate
        {
            public IssueTemplateVeryShort(ICheck check)
                : base(check, IssueType.Problem, "Duration very short for an Easy difficulty ({0:0} ms), expected ~{1:0} ms.")
            {
            }

            public Issue Create(Slider slider, double duration) => new Issue(slider, this, duration, expected_duration);
        }

        public class IssueTemplateTooShort : IssueTemplate
        {
            public IssueTemplateTooShort(ICheck check)
                : base(check, IssueType.Problem, "Duration too short for an Easy difficulty ({0:0} ms), expected ~{1:0} ms.")
            {
            }

            public Issue Create(Slider slider, double duration) => new Issue(slider, this, duration, expected_duration);
        }
    }
}
