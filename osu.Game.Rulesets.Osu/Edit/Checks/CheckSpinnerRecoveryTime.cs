// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Checks.Components;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Edit.Checks
{
    public class CheckSpinnerRecoveryTime : ICheck
    {
        public CheckMetadata Metadata { get; } = new CheckMetadata(CheckCategory.Compose, "Spinner recovery time");

        public IEnumerable<IssueTemplate> PossibleTemplates => new IssueTemplate[]
        {
            new IssueTemplateSpinnerRecoveryTooShort(this)
        };

        public IEnumerable<Issue> Run(BeatmapVerifierContext context)
        {
            double? recommendedRecoveryTime_ = this.recommendedRecoveryTime(context.InterpretedDifficulty);

            // If anything goes, we can skip this check
            if (recommendedRecoveryTime_ is not double recommendedRecoveryTime)
            {
                yield break;
            }

            foreach (var (firstObject, secondObject) in context.Beatmap.HitObjects.Zip(context.Beatmap.HitObjects.Skip(1)))
            {
                if (firstObject is not Spinner spinner)
                    continue;

                double timeDifference = secondObject.StartTime - spinner.EndTime;

                var timingPoint = context.Beatmap.ControlPointInfo.TimingPointAt(spinner.EndTime);

                double timeDifferenceInBeats = timeDifference / timingPoint.BeatLength;

                if (timeDifferenceInBeats < recommendedRecoveryTime)
                {
                    yield return new IssueTemplateSpinnerRecoveryTooShort(this)
                        .Create(spinner, timeDifferenceInBeats, recommendedRecoveryTime, context.InterpretedDifficulty);
                }
            }

            yield break;
        }

        /// <summary>
        /// Get the recommended number of beats for spinner recovery
        /// </summary>
        /// <param name="interpretedDifficulty">The difficulty to interpret the beatmap as</param>
        /// <returns>Number of beats if there is a guideline for it, null if anything goes</returns>
        private double? recommendedRecoveryTime(DifficultyRating interpretedDifficulty)
        {
            switch (interpretedDifficulty)
            {
                case DifficultyRating.Easy:
                    return 4;

                case DifficultyRating.Normal:
                    return 2;

                case DifficultyRating.Hard:
                    return 1;

                case DifficultyRating.Insane:
                case DifficultyRating.Expert:
                case DifficultyRating.ExpertPlus:
                default:
                    return null;
            }
        }

        public class IssueTemplateSpinnerRecoveryTooShort : IssueTemplate
        {
            public IssueTemplateSpinnerRecoveryTooShort(ICheck check)
                : base(check, IssueType.Warning, "This spinner only has {0:F2} beats of recovery which is less than the recommended {1:F2} beats for {2} difficulties")
            {
            }

            public Issue Create(Spinner spinner, double actualBeats, double expectedBeats, DifficultyRating difficulty) =>
                new Issue(spinner, this, actualBeats, expectedBeats, difficulty.ToString());
        }
    }
}
