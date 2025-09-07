// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Models;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Select.Leaderboards;
using Realms;

namespace osu.Game.Scoring
{
    public static class ScoreInfoExtensions
    {
        /// <summary>
        /// A user-presentable display title representing this score.
        /// </summary>
        public static string GetDisplayTitle(this IScoreInfo scoreInfo) => $"{scoreInfo.User.Username} playing {scoreInfo.Beatmap?.GetDisplayTitle() ?? "unknown"}";

        /// <summary>
        /// Orders an array of <see cref="ScoreInfo"/>s by total score.
        /// </summary>
        /// <param name="scores">The array of <see cref="ScoreInfo"/>s to reorder.</param>
        /// <returns>The given <paramref name="scores"/> ordered by decreasing total score.</returns>
        public static IEnumerable<ScoreInfo> OrderByTotalScore(this IEnumerable<ScoreInfo> scores)
            => scores.OrderByDescending(s => s.TotalScore)
                     .ThenBy(s => s.OnlineID)
                     // Local scores may not have an online ID. Fall back to date in these cases.
                     .ThenBy(s => s.Date);

        /// <summary>
        /// Orders an array of <see cref="ScoreInfo"/>s by the selected <see cref="LeaderboardSortMode"/>.
        /// </summary>
        /// <param name="scores">The array of <see cref="ScoreInfo"/>s to reorder.</param>
        /// <param name="leaderboardSortMode">The attribute to sort the scores by.</param>
        /// <returns>The given <paramref name="scores"/> ordered by the selected mode.</returns>
        public static IEnumerable<ScoreInfo> OrderByCriteria(this IEnumerable<ScoreInfo> scores, LeaderboardSortMode leaderboardSortMode)
        {
            switch (leaderboardSortMode)
            {
                case LeaderboardSortMode.Score:
                    return scores.OrderByDescending(s => s.TotalScore);

                case LeaderboardSortMode.Accuracy:
                    return scores.OrderByDescending(s => s.Accuracy).ThenByDescending(s => s.TotalScore);

                case LeaderboardSortMode.MaxCombo:
                    return scores.OrderByDescending(s => s.MaxCombo).ThenByDescending(s => s.TotalScore);

                case LeaderboardSortMode.Misses:
                    return scores.OrderBy(s => s.Statistics.GetValueOrDefault(HitResult.Miss, 0)).ThenByDescending(s => s.TotalScore);

                case LeaderboardSortMode.Date:
                    return scores.OrderByDescending(s => s.Date);

                default:
                    throw new ArgumentOutOfRangeException(nameof(leaderboardSortMode), leaderboardSortMode, null);
            }
        }

        /// <summary>
        /// Retrieves the maximum achievable combo for the provided score.
        /// </summary>
        /// <param name="score">The <see cref="ScoreInfo"/> to compute the maximum achievable combo for.</param>
        /// <returns>The maximum achievable combo.</returns>
        public static int GetMaximumAchievableCombo(this ScoreInfo score) => score.MaximumStatistics.Where(kvp => kvp.Key.AffectsCombo()).Sum(kvp => kvp.Value);

        /// <summary>
        /// Performs a realm filter that returns all scores that belong to the user with the given <paramref name="userId"/>.
        /// <see langword="null"/> <paramref name="userId"/> (for guests) is supported.
        /// </summary>
        /// <remarks>
        /// All guest scores (with user ID of <see cref="APIUser.SYSTEM_USER_ID"/>),
        /// as well as scores of unknown provenance (with default user ID of 1, see <see cref="APIUser.OnlineID"/>),
        /// will be treated as if they belong to the local user.
        /// This may not be necessarily considered fully correct in some circumstances, but in most cases it is the desired effect.
        /// </remarks>
        public static IQueryable<ScoreInfo> GetAllLocalScoresForUser(this Realm realm, int? userId)
        {
            return realm.All<ScoreInfo>()
                        .Filter($@"({nameof(ScoreInfo.User)}.{nameof(RealmUser.OnlineID)} == $0 || {nameof(ScoreInfo.User)}.{nameof(RealmUser.OnlineID)} <= 1)"
                                + $@" && {nameof(ScoreInfo.BeatmapInfo)}.{nameof(BeatmapInfo.Hash)} == {nameof(ScoreInfo.BeatmapHash)}"
                                + $@" && {nameof(ScoreInfo.DeletePending)} == false", userId);
        }
    }
}
