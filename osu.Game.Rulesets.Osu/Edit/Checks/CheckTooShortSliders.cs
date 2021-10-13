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
        /// <summary>
        /// The shortest acceptable duration between the head and tail of the slider (so ignoring repeats).
        /// </summary>
        private const double span_duration_threshold = 125; // 240 BPM 1/2

        public CheckMetadata Metadata { get; } = new CheckMetadata(CheckCategory.Spread, "Too short sliders");

        public IEnumerable<IssueTemplate> PossibleTemplates => new IssueTemplate[]
        {
            new IssueTemplateTooShort(this)
        };

        public IEnumerable<Issue> Run(BeatmapVerifierContext context)
        {
            if (context.InterpretedDifficulty > DifficultyRating.Easy)
                yield break;

            foreach (var hitObject in context.Beatmap.HitObjects)
            {
                if (hitObject is Slider slider && slider.SpanDuration < span_duration_threshold)
                    yield return new IssueTemplateTooShort(this).Create(slider);
            }
        }

        public class IssueTemplateTooShort : IssueTemplate
        {
            public IssueTemplateTooShort(ICheck check)
                : base(check, IssueType.Problem, "This slider is too short ({0:0} ms), expected at least {1:0} ms.")
            {
            }

            public Issue Create(Slider slider) => new Issue(slider, this, slider.SpanDuration, span_duration_threshold);
        }
    }
}
