// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Checks.Components;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Edit.Checks
{
    public class CheckTooShortSpinners : ICheck
    {
        public CheckMetadata Metadata { get; } = new CheckMetadata(CheckCategory.Spread, "Too short spinners");

        public IEnumerable<IssueTemplate> PossibleTemplates => new IssueTemplate[]
        {
            new IssueTemplateTooShort(this)
        };

        public IEnumerable<Issue> Run(BeatmapVerifierContext context)
        {
            double od = context.Beatmap.Difficulty.OverallDifficulty;

            // These are meant to reflect the duration necessary for auto to score at least 1000 points on the spinner.
            // It's difficult to eliminate warnings here, as auto achieving 1000 points depends on the approach angle on some spinners.
            double warningThreshold = 500 + (od < 5 ? (5 - od) * -21.8 : (od - 5) * 20); // Anything above this is always ok.
            double problemThreshold = 450 + (od < 5 ? (5 - od) * -17 : (od - 5) * 17); // Anything below this is never ok.

            foreach (var hitObject in context.Beatmap.HitObjects)
            {
                if (!(hitObject is Spinner spinner))
                    continue;

                if (spinner.Duration < problemThreshold)
                    yield return new IssueTemplateTooShort(this).Create(spinner);
                else if (spinner.Duration < warningThreshold)
                    yield return new IssueTemplateVeryShort(this).Create(spinner);
            }
        }

        public class IssueTemplateTooShort : IssueTemplate
        {
            public IssueTemplateTooShort(ICheck check)
                : base(check, IssueType.Problem, "This spinner is too short. Auto cannot achieve 1000 points on this.")
            {
            }

            public Issue Create(Spinner spinner) => new Issue(spinner, this);
        }

        public class IssueTemplateVeryShort : IssueTemplate
        {
            public IssueTemplateVeryShort(ICheck check)
                : base(check, IssueType.Warning, "This spinner may be too short. Ensure auto can achieve 1000 points on this.")
            {
            }

            public Issue Create(Spinner spinner) => new Issue(spinner, this);
        }
    }
}
