// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Checks.Components;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Rulesets.Catch.Edit.Checks
{
    public class CheckTooShortSpinnerGap : ICheck
    {
        private static readonly int[] spinner_start_delta_threshold = { 250, 250, 125, 125, 62, 62 };
        private static readonly int[] spinner_end_delta_threshold = { 250, 250, 250, 125, 125, 125 };

        public CheckMetadata Metadata { get; } = new CheckMetadata(CheckCategory.Compose, "Too short spinner gap");

        public IEnumerable<IssueTemplate> PossibleTemplates => new IssueTemplate[]
        {
            new IssueTemplateSpinnerStartGap(this),
            new IssueTemplateSpinnerEndGap(this)
        };

        public IEnumerable<Issue> Run(BeatmapVerifierContext context)
        {
            var hitObjects = context.Beatmap.HitObjects;
            int interpretedDifficulty = (int)context.InterpretedDifficulty;
            int expectedStartDelta = spinner_start_delta_threshold[interpretedDifficulty];
            int expectedEndDelta = spinner_end_delta_threshold[interpretedDifficulty];

            for (int i = 0; i < hitObjects.Count - 1; ++i)
            {
                if (!(hitObjects[i] is BananaShower bananaShower))
                    continue;

                if (i != 0 && hitObjects[i - 1] is CatchHitObject previousHitObject && !(previousHitObject is BananaShower))
                {
                    double spinnerStartDelta = bananaShower.StartTime - previousHitObject.GetEndTime();

                    if (spinnerStartDelta < expectedStartDelta)
                    {
                        yield return new IssueTemplateSpinnerStartGap(this)
                            .Create(spinnerStartDelta, expectedStartDelta, bananaShower, previousHitObject);
                    }
                }

                if (hitObjects[i + 1] is CatchHitObject nextHitObject && !(nextHitObject is BananaShower))
                {
                    double spinnerEndDelta = nextHitObject.StartTime - bananaShower.EndTime;

                    if (spinnerEndDelta < expectedEndDelta)
                    {
                        yield return new IssueTemplateSpinnerEndGap(this)
                            .Create(spinnerEndDelta, expectedEndDelta, bananaShower, nextHitObject);
                    }
                }
            }
        }

        public abstract class IssueTemplateSpinnerGap : IssueTemplate
        {
            protected IssueTemplateSpinnerGap(ICheck check, IssueType issueType, string unformattedMessage)
                : base(check, issueType, unformattedMessage)
            {
            }

            public Issue Create(double deltaTime, int expectedDeltaTime, params HitObject[] hitObjects)
            {
                return new Issue(hitObjects, this, Math.Floor(deltaTime), expectedDeltaTime);
            }
        }

        public class IssueTemplateSpinnerStartGap : IssueTemplateSpinnerGap
        {
            public IssueTemplateSpinnerStartGap(ICheck check)
                : base(check, IssueType.Problem, "There is only {0} ms apart between the start of the spinner and the last object, there should be {1} ms or more.")
            {
            }
        }

        public class IssueTemplateSpinnerEndGap : IssueTemplateSpinnerGap
        {
            public IssueTemplateSpinnerEndGap(ICheck check)
                : base(check, IssueType.Problem, "There is only {0} ms apart between the end of the spinner and the next object, there should be {1} ms or more.")
            {
            }
        }
    }
}
