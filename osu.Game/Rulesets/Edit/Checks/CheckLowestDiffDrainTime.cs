// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Edit.Checks.Components;

namespace osu.Game.Rulesets.Edit.Checks
{
    public abstract class CheckLowestDiffDrainTime : ICheck
    {
        /// <summary>
        /// Defines the minimum drain time thresholds for different difficulty ratings.
        /// </summary>
        protected abstract IEnumerable<(DifficultyRating rating, double thresholdMs, string name)> GetThresholds();

        private const double break_time_leniency = 30 * 1000;

        public CheckMetadata Metadata => new CheckMetadata(CheckCategory.Spread, "Lowest difficulty too difficult for the given drain/play time(s)");

        public IEnumerable<IssueTemplate> PossibleTemplates => new IssueTemplate[]
        {
            new IssueTemplateTooShort(this)
        };

        public IEnumerable<Issue> Run(BeatmapVerifierContext context)
        {
            IReadOnlyList<IBeatmap> difficulties = context.BeatmapsetDifficulties
                                                          .Where(d => d.BeatmapInfo.Ruleset.Equals(context.Beatmap.BeatmapInfo.Ruleset))
                                                          .ToList();

            if (difficulties.Count == 0)
                yield break;

            var lowestDifficulty = difficulties.OrderBy(b => b.BeatmapInfo.StarRating).First();

            // Get difficulty rating for the lowest difficulty
            DifficultyRating lowestDifficultyRating = lowestDifficulty == context.Beatmap
                ? context.InterpretedDifficulty
                : StarDifficulty.GetDifficultyRating(lowestDifficulty.BeatmapInfo.StarRating);

            double drainTime = context.Beatmap.CalculateDrainLength();
            double playTime = context.Beatmap.CalculatePlayableLength();

            bool isHighestDifficulty = difficulties.OrderByDescending(b => b.BeatmapInfo.StarRating).First() == context.Beatmap;

            // Use play time unless it's the highest difficulty and has significant breaks
            bool canUsePlayTime = !isHighestDifficulty || context.Beatmap.TotalBreakTime < break_time_leniency;

            double effectiveTime = canUsePlayTime ? playTime : drainTime;
            double thresholdReduction = canUsePlayTime ? 0 : break_time_leniency;

            // Check against thresholds based on the lowest difficulty's rating in the beatmapset
            // Find the most appropriate threshold (highest rating that applies)
            var applicableThreshold = GetThresholds()
                                      .Where(t => lowestDifficultyRating >= t.rating)
                                      .OrderByDescending(t => t.rating)
                                      .FirstOrDefault();

            if (applicableThreshold != default && effectiveTime < applicableThreshold.thresholdMs - thresholdReduction)
            {
                yield return new IssueTemplateTooShort(this).Create(
                    applicableThreshold.name,
                    canUsePlayTime ? "play" : "drain",
                    applicableThreshold.thresholdMs - thresholdReduction,
                    effectiveTime
                );
            }
        }

        public class IssueTemplateTooShort : IssueTemplate
        {
            public IssueTemplateTooShort(ICheck check)
                : base(check, IssueType.Problem, "With the lowest difficulty being \"{0}\", the {1} time of this difficulty must be at least {2}, currently {3}.")
            {
            }

            public Issue Create(string lowestDiffLevel, string timeType, double requiredTime, double currentTime)
                => new Issue(this,
                    lowestDiffLevel,
                    timeType,
                    TimeSpan.FromMilliseconds(requiredTime).ToString(@"m\:ss"),
                    TimeSpan.FromMilliseconds(currentTime).ToString(@"m\:ss"));
        }
    }
}
