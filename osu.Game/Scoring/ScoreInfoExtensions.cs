// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Scoring
{
    public static class ScoreInfoExtensions
    {
        /// <summary>
        /// Computes the total score based on the given scoring mode.
        /// </summary>
        public static long GetDisplayScore(this IScoreInfo score, ScoringMode mode)
        {
            switch (score)
            {
                case ScoreInfo scoreInfo:
                    return Legacy.ScoreInfoExtensions.GetDisplayScore(scoreInfo, mode);

                case SoloScoreInfo soloScoreInfo:
                    return Legacy.ScoreInfoExtensions.GetDisplayScore(soloScoreInfo, mode);

                default:
                    throw new InvalidOperationException();
            }
        }

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
        /// Returns the list of hit result statistics applicable for this score.
        /// </summary>
        public static IEnumerable<HitResultDisplayStatistic> GetStatisticsForDisplay(this IScoreInfo score, Ruleset ruleset)
        {
            if (ruleset.RulesetInfo.OnlineID != score.Ruleset.OnlineID)
                throw new InvalidOperationException($@"The ID of the given ruleset instance ({ruleset.RulesetInfo.OnlineID}) does not match the score's ruleset ({score.Ruleset.OnlineID})");

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

        /// <summary>
        /// Retrieves a bindable that represents the total score of a <see cref="IScoreInfo"/>.
        /// </summary>
        /// <remarks>
        /// Responds to changes in the currently-selected <see cref="ScoringMode"/>.
        /// </remarks>
        /// <param name="score">The <see cref="IScoreInfo"/> to retrieve the bindable for.</param>
        /// <param name="config">The <see cref="OsuConfigManager"/> to read and listen to the active scoring mode from.</param>
        /// <returns>The bindable containing the total score.</returns>
        public static Bindable<long> GetBindableTotalScore(this IScoreInfo score, OsuConfigManager config) => new TotalScoreBindable(score, config);

        /// <summary>
        /// Retrieves a bindable that represents the formatted total score string of a <see cref="IScoreInfo"/>.
        /// </summary>
        /// <remarks>
        /// Responds to changes in the currently-selected <see cref="ScoringMode"/>.
        /// </remarks>
        /// <param name="score">The <see cref="IScoreInfo"/> to retrieve the bindable for.</param>
        /// <param name="config">The <see cref="OsuConfigManager"/> to read and listen to the active scoring mode from.</param>
        /// <returns>The bindable containing the formatted total score string.</returns>
        public static Bindable<string> GetBindableTotalScoreString(this IScoreInfo score, OsuConfigManager config) => new TotalScoreStringBindable(GetBindableTotalScore(score, config));

        #region ScoreInfo-specific extensions

        /// <summary>
        /// Retrieves the maximum achievable combo for the provided score.
        /// </summary>
        /// <param name="score">The <see cref="ScoreInfo"/> to compute the maximum achievable combo for.</param>
        /// <returns>The maximum achievable combo.</returns>
        public static int GetMaximumAchievableCombo(this ScoreInfo score) => score.MaximumStatistics.Where(kvp => kvp.Key.AffectsCombo()).Sum(kvp => kvp.Value);

        /// <summary>
        /// Returns the list of hit result statistics applicable for this score.
        /// </summary>
        public static IEnumerable<HitResultDisplayStatistic> GetStatisticsForDisplay(this ScoreInfo score) => score.GetStatisticsForDisplay(score.Ruleset.CreateInstance());

        #endregion

        /// <summary>
        /// Provides the total score of a <see cref="IScoreInfo"/>. Responds to changes in the currently-selected <see cref="ScoringMode"/>.
        /// </summary>
        private class TotalScoreBindable : Bindable<long>
        {
            private readonly Bindable<ScoringMode> scoringMode = new Bindable<ScoringMode>();

            /// <summary>
            /// Creates a new <see cref="TotalScoreBindable"/>.
            /// </summary>
            /// <param name="score">The <see cref="IScoreInfo"/> to provide the total score of.</param>
            /// <param name="config">The config.</param>
            public TotalScoreBindable(IScoreInfo score, OsuConfigManager? config)
            {
                config?.BindWith(OsuSetting.ScoreDisplayMode, scoringMode);
                scoringMode.BindValueChanged(mode => Value = score.GetDisplayScore(mode.NewValue), true);
            }
        }

        /// <summary>
        /// Provides the total score of a <see cref="IScoreInfo"/> as a formatted string. Responds to changes in the currently-selected <see cref="ScoringMode"/>.
        /// </summary>
        private class TotalScoreStringBindable : Bindable<string>
        {
            // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable (need to hold a reference)
            private readonly IBindable<long> totalScore;

            public TotalScoreStringBindable(IBindable<long> totalScore)
            {
                this.totalScore = totalScore;
                this.totalScore.BindValueChanged(v => Value = v.NewValue.ToString("N0"), true);
            }
        }
    }
}
