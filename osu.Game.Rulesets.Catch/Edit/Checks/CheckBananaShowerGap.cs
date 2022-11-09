// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Checks.Components;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Rulesets.Catch.Edit.Checks
{
    /// <summary>
    /// Check the spinner/banana shower gaps specified in the osu!catch difficulty specific ranking criteria.
    /// </summary>
    public class CheckBananaShowerGap : ICheck
    {
        private static readonly Dictionary<DifficultyRating, (int startGap, int endGap)> spinner_delta_threshold = new Dictionary<DifficultyRating, (int, int)>
        {
            [DifficultyRating.Easy] = (250, 250),
            [DifficultyRating.Normal] = (250, 250),
            [DifficultyRating.Hard] = (125, 250),
            [DifficultyRating.Insane] = (125, 125),
            [DifficultyRating.Expert] = (62, 125),
            [DifficultyRating.ExpertPlus] = (62, 125)
        };

        public CheckMetadata Metadata { get; } = new CheckMetadata(CheckCategory.Compose, "Too short spinner gap");

        public IEnumerable<IssueTemplate> PossibleTemplates => new IssueTemplate[]
        {
            new IssueTemplateBananaShowerStartGap(this),
            new IssueTemplateBananaShowerEndGap(this)
        };

        public IEnumerable<Issue> Run(BeatmapVerifierContext context)
        {
            var hitObjects = context.Beatmap.HitObjects;
            (int expectedStartDelta, int expectedEndDelta) = spinner_delta_threshold[context.InterpretedDifficulty];

            for (int i = 0; i < hitObjects.Count - 1; ++i)
            {
                if (!(hitObjects[i] is BananaShower bananaShower))
                    continue;

                // Skip if the previous hitobject is a banana shower, consecutive spinners are allowed
                if (i != 0 && hitObjects[i - 1] is CatchHitObject previousHitObject && !(previousHitObject is BananaShower))
                {
                    double spinnerStartDelta = bananaShower.StartTime - previousHitObject.GetEndTime();

                    if (spinnerStartDelta < expectedStartDelta)
                    {
                        yield return new IssueTemplateBananaShowerStartGap(this)
                            .Create(spinnerStartDelta, expectedStartDelta, bananaShower, previousHitObject);
                    }
                }

                // Skip if the next hitobject is a banana shower, consecutive spinners are allowed
                if (hitObjects[i + 1] is CatchHitObject nextHitObject && !(nextHitObject is BananaShower))
                {
                    double spinnerEndDelta = nextHitObject.StartTime - bananaShower.EndTime;

                    if (spinnerEndDelta < expectedEndDelta)
                    {
                        yield return new IssueTemplateBananaShowerEndGap(this)
                            .Create(spinnerEndDelta, expectedEndDelta, bananaShower, nextHitObject);
                    }
                }
            }
        }

        public abstract class IssueTemplateBananaShowerGap : IssueTemplate
        {
            protected IssueTemplateBananaShowerGap(ICheck check, IssueType issueType, string unformattedMessage)
                : base(check, issueType, unformattedMessage)
            {
            }

            public Issue Create(double deltaTime, int expectedDeltaTime, params HitObject[] hitObjects)
            {
                return new Issue(hitObjects, this, Math.Floor(deltaTime), expectedDeltaTime);
            }
        }

        public class IssueTemplateBananaShowerStartGap : IssueTemplateBananaShowerGap
        {
            public IssueTemplateBananaShowerStartGap(ICheck check)
                : base(check, IssueType.Problem, "There is only {0} ms between the start of the spinner and the last object, it should not be less than {1} ms.")
            {
            }
        }

        public class IssueTemplateBananaShowerEndGap : IssueTemplateBananaShowerGap
        {
            public IssueTemplateBananaShowerEndGap(ICheck check)
                : base(check, IssueType.Problem, "There is only {0} ms between the end of the spinner and the next object, it should not be less than {1} ms.")
            {
            }
        }
    }
}
