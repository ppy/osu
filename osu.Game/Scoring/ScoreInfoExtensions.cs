// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Scoring
{
    public static class ScoreInfoExtensions
    {
        /// <summary>
        /// A user-presentable display title representing this score.
        /// </summary>
        public static string GetDisplayTitle(this IScoreInfo scoreInfo) => $"{scoreInfo.User.Username} playing {scoreInfo.Beatmap?.GetDisplayTitle() ?? "unknown"}";

        /// <summary>
        /// Orders an array of <typeparamref name="TScoreInfo"/>s by total score.
        /// </summary>
        /// <param name="scores">The array of <typeparamref name="TScoreInfo"/>s to reorder.</param>
        /// <returns>The given <paramref name="scores"/> ordered by decreasing total score.</returns>
        public static IEnumerable<TScoreInfo> OrderByTotalScore<TScoreInfo>(this IEnumerable<TScoreInfo> scores)
            where TScoreInfo : IScoreInfo
            => scores.OrderByDescending(s => s.TotalScore)
                     .ThenBy(s => s.OnlineID)
                     // Local scores may not have an online ID. Fall back to date in these cases.
                     .ThenBy(s => s.Date);

        /// <summary>
        /// Retrieves the maximum achievable combo for the provided score.
        /// </summary>
        /// <param name="score">The <see cref="ScoreInfo"/> to compute the maximum achievable combo for.</param>
        /// <returns>The maximum achievable combo.</returns>
        public static int GetMaximumAchievableCombo(this ScoreInfo score) => score.MaximumStatistics.Where(kvp => kvp.Key.AffectsCombo()).Sum(kvp => kvp.Value);

        public static IEnumerable<HitResultDisplayStatistic> GetStatisticsForDisplay(this ScoreInfo score) => score.GetStatisticsForDisplay(score.Ruleset.CreateInstance());

        public static IEnumerable<HitResultDisplayStatistic> GetStatisticsForDisplay(this IScoreInfo score, Ruleset ruleset)
        {
            foreach (var r in ruleset.GetHitResults())
            {
                int value = score.Statistics.GetValueOrDefault(r.result);

                switch (r.result)
                {
                    case HitResult.SmallTickHit:
                    case HitResult.LargeTickHit:
                    case HitResult.SliderTailHit:
                    case HitResult.LargeBonus:
                    case HitResult.SmallBonus:
                        if (score.MaximumStatistics.TryGetValue(r.result, out int count) && count > 0)
                            yield return new HitResultDisplayStatistic(r.result, value, count, r.displayName);

                        break;

                    case HitResult.SmallTickMiss:
                    case HitResult.LargeTickMiss:
                        break;

                    default:
                        yield return new HitResultDisplayStatistic(r.result, value, null, r.displayName);

                        break;
                }
            }
        }
    }
}
