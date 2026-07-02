// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Logging;
using osu.Game.Beatmaps;
using osu.Game.Extensions;
using osu.Game.IO.Legacy;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Scoring.Legacy;
using osu.Game.Scoring;
using osu.Game.Scoring.Legacy;

namespace osu.Game.Database
{
    public static class StandardisedScoreMigrationTools
    {
        /// <summary>
        /// Updates <paramref name="score"/>'s <see cref="ScoreInfo.TotalScore"/> to the latest applicable version.
        /// </summary>
        /// <param name="score">The score to update.</param>
        /// <param name="beatmap">The <see cref="WorkingBeatmap"/> applicable for this score.</param>
        public static void UpdateToLatestScoring(ScoreInfo score, WorkingBeatmap beatmap)
        {
            if (score.IsLegacyScore)
                UpdateFromLegacy(score, beatmap);
            else
            {
                var ruleset = score.Ruleset.CreateInstance();
                var scoreProcessor = ruleset.CreateScoreProcessor();

                // accuracy and rank may not be populated on old lazer scores - ensure they're always there.
                // warning: ordering is important here - rank is dependent on accuracy!
                score.Accuracy = ComputeAccuracy(score, scoreProcessor);
                score.Rank = ComputeRank(score, scoreProcessor);

                // update score to latest multipliers too, if possible.
                if (score.TotalScoreWithoutMods > 0)
                    UpdateToLatestScoreMultipliers(score, beatmap.BeatmapInfo.Difficulty);
            }

            score.TotalScoreVersion = LegacyScoreEncoder.LATEST_VERSION;
        }

        /// <summary>
        /// Updates a <see cref="ScoreInfo"/> to standardised scoring.
        /// This will recompite the score's <see cref="ScoreInfo.Accuracy"/> (always), <see cref="ScoreInfo.Rank"/> (always),
        /// and <see cref="ScoreInfo.TotalScore"/> (if the score comes from stable).
        /// The total score from stable - if any applicable - will be stored to <see cref="ScoreInfo.LegacyTotalScore"/>.
        /// </summary>
        /// <param name="score">The score to update.</param>
        /// <param name="beatmap">The <see cref="WorkingBeatmap"/> applicable for this score.</param>
        public static void UpdateFromLegacy(ScoreInfo score, WorkingBeatmap beatmap)
        {
            var ruleset = score.Ruleset.CreateInstance();
            var scoreProcessor = ruleset.CreateScoreProcessor();

            // warning: ordering is important here - both total score and ranks are dependent on accuracy!
            score.Accuracy = ComputeAccuracy(score, scoreProcessor);
            score.Rank = ComputeRank(score, scoreProcessor);
            (score.TotalScoreWithoutMods, score.TotalScore) = convertFromLegacyTotalScore(score, ruleset, beatmap);
        }

        /// <summary>
        /// Updates a <see cref="ScoreInfo"/> to standardised scoring.
        /// This will recompute the score's <see cref="ScoreInfo.Accuracy"/> (always), <see cref="ScoreInfo.Rank"/> (always),
        /// and <see cref="ScoreInfo.TotalScore"/> (if the score comes from stable).
        /// The total score from stable - if any applicable - will be stored to <see cref="ScoreInfo.LegacyTotalScore"/>.
        /// </summary>
        /// <remarks>
        /// This overload is intended for server-side flows.
        /// See: https://github.com/ppy/osu-queue-score-statistics/blob/3681e92ac91c6c61922094bdbc7e92e6217dd0fc/osu.Server.Queues.ScoreStatisticsProcessor/Commands/Queue/BatchInserter.cs
        /// </remarks>
        /// <param name="score">The score to update.</param>
        /// <param name="ruleset">The <see cref="Ruleset"/> in which the score was set.</param>
        /// <param name="difficulty">The beatmap difficulty.</param>
        /// <param name="attributes">The legacy scoring attributes for the beatmap which the score was set on.</param>
        public static void UpdateFromLegacy(ScoreInfo score, Ruleset ruleset, LegacyBeatmapConversionDifficultyInfo difficulty, LegacyScoreAttributes attributes)
        {
            var scoreProcessor = ruleset.CreateScoreProcessor();

            // warning: ordering is important here - both total score and ranks are dependent on accuracy!
            score.Accuracy = ComputeAccuracy(score, scoreProcessor);
            score.Rank = ComputeRank(score, scoreProcessor);
            (score.TotalScoreWithoutMods, score.TotalScore) = convertFromLegacyTotalScore(score, ruleset, difficulty, attributes);
        }

        /// <summary>
        /// Converts from <see cref="ScoreInfo.LegacyTotalScore"/> to the new standardised scoring of <see cref="ScoreProcessor"/>.
        /// </summary>
        /// <param name="score">The score to convert the total score of.</param>
        /// <param name="ruleset">The <see cref="Ruleset"/> in which the score was set.</param>
        /// <param name="beatmap">The <see cref="WorkingBeatmap"/> applicable for this score.</param>
        /// <returns>The standardised total score.</returns>
        private static (long withoutMods, long withMods) convertFromLegacyTotalScore(ScoreInfo score, Ruleset ruleset, WorkingBeatmap beatmap)
        {
            if (!score.IsLegacyScore)
                return (score.TotalScoreWithoutMods, score.TotalScore);

            if (ruleset is not ILegacyRuleset legacyRuleset)
                return (score.TotalScoreWithoutMods, score.TotalScore);

            var playableBeatmap = beatmap.GetPlayableBeatmap(ruleset.RulesetInfo, score.Mods);

            if (playableBeatmap.HitObjects.Count == 0)
                throw new InvalidOperationException("Beatmap contains no hit objects!");

            ILegacyScoreSimulator sv1Simulator = legacyRuleset.CreateLegacyScoreSimulator();
            LegacyScoreAttributes attributes = sv1Simulator.Simulate(beatmap, playableBeatmap);
            var legacyBeatmapConversionDifficultyInfo = LegacyBeatmapConversionDifficultyInfo.FromBeatmap(beatmap.Beatmap);

            var mods = score.Mods;
            if (mods.Any(mod => mod is ModScoreV2))
                return ((long)Math.Round(score.TotalScore / sv1Simulator.GetLegacyScoreMultiplier(mods, legacyBeatmapConversionDifficultyInfo)), score.TotalScore);

            return convertFromLegacyTotalScore(score, ruleset, legacyBeatmapConversionDifficultyInfo, attributes);
        }

        /// <summary>
        /// Converts from <see cref="ScoreInfo.LegacyTotalScore"/> to the new standardised scoring of <see cref="ScoreProcessor"/>.
        /// </summary>
        /// <param name="score">The score to convert the total score of.</param>
        /// <param name="ruleset">The <see cref="Ruleset"/> in which the score was set.</param>
        /// <param name="difficulty">The beatmap difficulty.</param>
        /// <param name="attributes">The legacy scoring attributes for the beatmap which the score was set on.</param>
        /// <returns>The standardised total score.</returns>
        private static (long withoutMods, long withMods) convertFromLegacyTotalScore(ScoreInfo score, Ruleset ruleset, LegacyBeatmapConversionDifficultyInfo difficulty,
                                                                                     LegacyScoreAttributes attributes)
        {
            if (!score.IsLegacyScore)
                return (score.TotalScoreWithoutMods, score.TotalScore);

            Debug.Assert(score.LegacyTotalScore != null);

            if (ruleset is not ILegacyRuleset legacyRuleset)
                return (score.TotalScoreWithoutMods, score.TotalScore);

            double legacyModMultiplier = legacyRuleset.CreateLegacyScoreSimulator().GetLegacyScoreMultiplier(score.Mods, difficulty);
            int maximumLegacyAccuracyScore = attributes.AccuracyScore;
            long maximumLegacyComboScore = (long)Math.Round(attributes.ComboScore * legacyModMultiplier);
            double maximumLegacyBonusRatio = attributes.BonusScoreRatio;
            long maximumLegacyBonusScore = attributes.BonusScore;

            double legacyAccScore = maximumLegacyAccuracyScore * score.Accuracy;

            double comboProportion;

            if (maximumLegacyComboScore + maximumLegacyBonusScore > 0)
            {
                // We can not separate the ComboScore from the BonusScore, so we keep the bonus in the ratio.
                comboProportion = Math.Max((double)score.LegacyTotalScore - legacyAccScore, 0) / (maximumLegacyComboScore + maximumLegacyBonusScore);
            }
            else
            {
                // Two possible causes:
                // the beatmap has no bonus objects *AND*
                // either the active mods have a zero mod multiplier, in which case assume 0,
                // or the *beatmap* has a zero `difficultyPeppyStars` (or just no combo-giving objects), in which case assume 1.
                comboProportion = legacyModMultiplier == 0 ? 0 : 1;
            }

            // We assume the bonus proportion only makes up the rest of the score that exceeds maximumLegacyBaseScore.
            long maximumLegacyBaseScore = maximumLegacyAccuracyScore + maximumLegacyComboScore;
            double bonusProportion = Math.Max(0, ((long)score.LegacyTotalScore - maximumLegacyBaseScore) * maximumLegacyBonusRatio);

            var modMultiplierCalculator = ruleset.CreateScoreMultiplierCalculator(new ScoreMultiplierContext(difficulty));
            double modMultiplier = modMultiplierCalculator.CalculateFor(score.Mods);

            long convertedTotalScoreWithoutMods;

            switch (score.Ruleset.OnlineID)
            {
                case 0:
                    if (score.MaxCombo == 0 || score.Accuracy == 0)
                    {
                        convertedTotalScoreWithoutMods = (long)Math.Round(
                            0
                            + 500000 * Math.Pow(score.Accuracy, 5)
                            + bonusProportion);
                        break;
                    }

                    // see similar check above.
                    // if there is no legacy combo score, all combo conversion operations below
                    // are either pointless or wildly wrong.
                    if (maximumLegacyComboScore + maximumLegacyBonusScore == 0)
                    {
                        convertedTotalScoreWithoutMods = (long)Math.Round(
                            500000 * comboProportion // as above, zero if mods result in zero multiplier, one otherwise
                            + 500000 * Math.Pow(score.Accuracy, 5)
                            + bonusProportion);
                        break;
                    }

                    // Assumptions:
                    // - sliders and slider ticks are uniformly distributed in the beatmap, and thus can be ignored without losing much precision.
                    //   We thus consider a map of hit-circles only, which gives objectCount == maximumCombo.
                    // - the Ok/Meh hit results are uniformly spread in the score, and thus can be ignored without losing much precision.
                    //   We simplify and consider each hit result to have the same hit value of `300 * score.Accuracy`
                    //   (which represents the average hit value over the entire play),
                    //   which allows us to isolate the accuracy multiplier.

                    // This is a very ballpark estimate of the maximum magnitude of the combo portion in score V1.
                    // It is derived by assuming a full combo play and summing up the contribution to combo portion from each individual object.
                    // Because each object's combo contribution is proportional to the current combo at the time of judgement,
                    // this can be roughly represented by summing / integrating f(combo) = combo.
                    // All mod- and beatmap-dependent multipliers and constants are not included here,
                    // as we will only be using the magnitude of this to compute ratios.
                    int maximumLegacyCombo = attributes.MaxCombo;
                    double maximumAchievableComboPortionInScoreV1 = Math.Pow(maximumLegacyCombo, 2);
                    // Similarly, estimate the maximum magnitude of the combo portion in standardised score.
                    // Roughly corresponds to integrating f(combo) = combo ^ COMBO_EXPONENT (omitting constants)
                    double maximumAchievableComboPortionInStandardisedScore = Math.Pow(maximumLegacyCombo, 1 + ScoreProcessor.COMBO_EXPONENT);

                    // This is - roughly - how much score, in the combo portion, the longest combo on this particular play would gain in score V1.
                    double comboPortionFromLongestComboInScoreV1 = Math.Pow(score.MaxCombo, 2);
                    // Same for standardised score.
                    double comboPortionFromLongestComboInStandardisedScore = Math.Pow(score.MaxCombo, 1 + ScoreProcessor.COMBO_EXPONENT);

                    // We estimate the combo portion of the score in score V1 terms.
                    // The division by accuracy is supposed to lessen the impact of accuracy on the combo portion,
                    // but in some edge cases it cannot sanely undo it.
                    // Therefore the resultant value is clamped from both sides for sanity.
                    // The clamp from below to `comboPortionFromLongestComboInScoreV1` targets near-FC scores wherein
                    // the player had bad accuracy at the end of their longest combo, which causes the division by accuracy
                    // to underestimate the combo portion.
                    // Ideally, this would be clamped from above to `maximumAchievableComboPortionInScoreV1` too,
                    // but in practice this appears to fail for some scores (https://github.com/ppy/osu/pull/25876#issuecomment-1862248413).
                    // TODO: investigate the above more closely
                    double comboPortionInScoreV1 = Math.Max(maximumAchievableComboPortionInScoreV1 * comboProportion / score.Accuracy, comboPortionFromLongestComboInScoreV1);

                    // Calculate how many times the longest combo the user has achieved in the play can repeat
                    // without exceeding the combo portion in score V1 as achieved by the player.
                    // This intentionally does not operate on object count and uses only score instead.
                    double maximumOccurrencesOfLongestCombo = Math.Floor(comboPortionInScoreV1 / comboPortionFromLongestComboInScoreV1);
                    double comboPortionFromRepeatedLongestCombosInScoreV1 = maximumOccurrencesOfLongestCombo * comboPortionFromLongestComboInScoreV1;

                    double remainingComboPortionInScoreV1 = comboPortionInScoreV1 - comboPortionFromRepeatedLongestCombosInScoreV1;
                    // `remainingComboPortionInScoreV1` is in the "score ballpark" realm, which means it's proportional to combo squared.
                    // To convert that back to a raw combo length, we need to take the square root...
                    double remainingCombo = Math.Sqrt(remainingComboPortionInScoreV1);
                    // ...and then based on that raw combo length, we calculate how much this last combo is worth in standardised score.
                    double remainingComboPortionInStandardisedScore = Math.Pow(remainingCombo, 1 + ScoreProcessor.COMBO_EXPONENT);

                    double scoreBasedEstimateOfComboPortionInStandardisedScore
                        = maximumOccurrencesOfLongestCombo * comboPortionFromLongestComboInStandardisedScore
                          + remainingComboPortionInStandardisedScore;

                    // Compute approximate upper estimate new score for that play.
                    // This time, divide the remaining combo among remaining objects equally to achieve longest possible combo lengths.
                    remainingComboPortionInScoreV1 = comboPortionInScoreV1 - comboPortionFromLongestComboInScoreV1;
                    double remainingCountOfObjectsGivingCombo = maximumLegacyCombo - score.MaxCombo - score.Statistics.GetValueOrDefault(HitResult.Miss);
                    // Because we assumed all combos were equal, `remainingComboPortionInScoreV1`
                    // can be approximated by n * x^2, wherein n is the assumed number of equal combos,
                    // and x is the assumed length of every one of those combos.
                    // The remaining count of objects giving combo is, using those terms, equal to n * x.
                    // Therefore, dividing the two will result in x, i.e. the assumed length of the remaining combos.
                    double lengthOfRemainingCombos = remainingCountOfObjectsGivingCombo > 0
                        ? remainingComboPortionInScoreV1 / remainingCountOfObjectsGivingCombo
                        : 0;
                    // In standardised scoring, each combo yields a score proportional to combo length to the power 1 + COMBO_EXPONENT.
                    // Using the symbols introduced above, that would be x ^ 1.5 per combo, n times (because there are n assumed equal-length combos).
                    // However, because `remainingCountOfObjectsGivingCombo` - using the symbols introduced above - is assumed to be equal to n * x,
                    // we can skip adding the 1 and just multiply by x ^ 0.5.
                    remainingComboPortionInStandardisedScore = remainingCountOfObjectsGivingCombo * Math.Pow(lengthOfRemainingCombos, ScoreProcessor.COMBO_EXPONENT);

                    double objectCountBasedEstimateOfComboPortionInStandardisedScore = comboPortionFromLongestComboInStandardisedScore + remainingComboPortionInStandardisedScore;

                    // Enforce some invariants on both of the estimates.
                    // In rare cases they can produce invalid results.
                    scoreBasedEstimateOfComboPortionInStandardisedScore =
                        Math.Clamp(scoreBasedEstimateOfComboPortionInStandardisedScore, 0, maximumAchievableComboPortionInStandardisedScore);
                    objectCountBasedEstimateOfComboPortionInStandardisedScore =
                        Math.Clamp(objectCountBasedEstimateOfComboPortionInStandardisedScore, 0, maximumAchievableComboPortionInStandardisedScore);

                    double lowerEstimateOfComboPortionInStandardisedScore = Math.Min(scoreBasedEstimateOfComboPortionInStandardisedScore, objectCountBasedEstimateOfComboPortionInStandardisedScore);
                    double upperEstimateOfComboPortionInStandardisedScore = Math.Max(scoreBasedEstimateOfComboPortionInStandardisedScore, objectCountBasedEstimateOfComboPortionInStandardisedScore);

                    // Approximate by combining lower and upper estimates.
                    // As the lower-estimate is very pessimistic, we use a 30/70 ratio
                    // and cap it with 1.2 times the middle-point to avoid overestimates.
                    double estimatedComboPortionInStandardisedScore = Math.Min(
                        0.3 * lowerEstimateOfComboPortionInStandardisedScore + 0.7 * upperEstimateOfComboPortionInStandardisedScore,
                        1.2 * (lowerEstimateOfComboPortionInStandardisedScore + upperEstimateOfComboPortionInStandardisedScore) / 2
                    );

                    double newComboScoreProportion = estimatedComboPortionInStandardisedScore / maximumAchievableComboPortionInStandardisedScore;

                    convertedTotalScoreWithoutMods = (long)Math.Round(
                        500000 * newComboScoreProportion * score.Accuracy
                        + 500000 * Math.Pow(score.Accuracy, 5)
                        + bonusProportion);
                    break;

                case 1:
                    convertedTotalScoreWithoutMods = (long)Math.Round(
                        250000 * comboProportion
                        + 750000 * Math.Pow(score.Accuracy, 3.6)
                        + bonusProportion);
                    break;

                case 2:
                    // compare logic in `CatchScoreProcessor`.

                    // this could technically be slightly incorrect in the case of stable scores.
                    // because large droplet misses are counted as full misses in stable scores,
                    // `score.MaximumStatistics.GetValueOrDefault(Great)` will be equal to the count of fruits *and* large droplets
                    // rather than just fruits (which was the intent).
                    // this is not fixable without introducing an extra legacy score attribute dedicated for catch,
                    // and this is a ballpark conversion process anyway, so attempt to trudge on.
                    int fruitTinyScaleDivisor = score.MaximumStatistics.GetValueOrDefault(HitResult.SmallTickHit) + score.MaximumStatistics.GetValueOrDefault(HitResult.Great);
                    double fruitTinyScale = fruitTinyScaleDivisor == 0
                        ? 0
                        : (double)score.MaximumStatistics.GetValueOrDefault(HitResult.SmallTickHit) / fruitTinyScaleDivisor;

                    const int max_tiny_droplets_portion = 400000;

                    double comboPortion = 1000000 - max_tiny_droplets_portion + max_tiny_droplets_portion * (1 - fruitTinyScale);
                    double dropletsPortion = max_tiny_droplets_portion * fruitTinyScale;
                    double dropletsHit = score.MaximumStatistics.GetValueOrDefault(HitResult.SmallTickHit) == 0
                        ? 0
                        : (double)score.Statistics.GetValueOrDefault(HitResult.SmallTickHit) / score.MaximumStatistics.GetValueOrDefault(HitResult.SmallTickHit);

                    convertedTotalScoreWithoutMods = (long)Math.Round(
                        comboPortion * estimateComboProportionForCatch(attributes.MaxCombo, score.MaxCombo, score.Statistics.GetValueOrDefault(HitResult.Miss))
                        + dropletsPortion * dropletsHit
                        + bonusProportion);
                    break;

                case 3:
                    convertedTotalScoreWithoutMods = (long)Math.Round(
                        850000 * comboProportion
                        + 150000 * Math.Pow(score.Accuracy, 2 + 2 * score.Accuracy)
                        + bonusProportion);
                    break;

                default:
                    return (score.TotalScoreWithoutMods, score.TotalScore);
            }

            if (convertedTotalScoreWithoutMods < 0)
                throw new InvalidOperationException($"Total score conversion operation returned invalid total of {convertedTotalScoreWithoutMods}");

            long convertedTotalScore = (long)Math.Round(convertedTotalScoreWithoutMods * modMultiplier);
            return (convertedTotalScoreWithoutMods, convertedTotalScore);
        }

        /// <summary>
        /// <para>
        /// For catch, the general method of calculating the combo proportion used for other rulesets is generally useless.
        /// This is because in stable score V1, catch has quadratic score progression,
        /// while in stable score V2, score progression is logarithmic up to 200 combo and then linear.
        /// </para>
        /// <para>
        /// This means that applying the naive rescale method to scores with lots of short combos (think 10x 100-long combos on a 1000-object map)
        /// by linearly rescaling the combo portion as given by score V1 leads to horribly underestimating it.
        /// Therefore this method attempts to counteract this by calculating the best case estimate for the combo proportion that takes all of the above into account.
        /// </para>
        /// <para>
        /// The general idea is that aside from the <paramref name="scoreMaxCombo"/> which the player is known to have hit,
        /// the remaining misses are evenly distributed across the rest of the objects that give combo.
        /// This is therefore a worst-case estimate.
        /// </para>
        /// </summary>
        private static double estimateComboProportionForCatch(int beatmapMaxCombo, int scoreMaxCombo, int scoreMissCount)
        {
            if (beatmapMaxCombo == 0)
                return 1;

            if (scoreMaxCombo == 0)
                return 0;

            if (beatmapMaxCombo == scoreMaxCombo)
                return 1;

            double estimatedBestCaseTotal = estimateBestCaseComboTotal(beatmapMaxCombo);

            int remainingCombo = beatmapMaxCombo - (scoreMaxCombo + scoreMissCount);
            double totalDroppedScore = 0;

            int assumedLengthOfRemainingCombos = (int)Math.Floor((double)remainingCombo / scoreMissCount);

            if (assumedLengthOfRemainingCombos > 0)
            {
                int assumedCombosCount = (int)Math.Floor((double)remainingCombo / assumedLengthOfRemainingCombos);
                totalDroppedScore += assumedCombosCount * estimateDroppedComboScoreAfterMiss(assumedLengthOfRemainingCombos);

                remainingCombo -= assumedCombosCount * assumedLengthOfRemainingCombos;

                if (remainingCombo > 0)
                    totalDroppedScore += estimateDroppedComboScoreAfterMiss(remainingCombo);
            }
            else
            {
                // there are so many misses that attempting to evenly divide remaining combo results in 0 length per combo,
                // i.e. all remaining judgements are combo breaks.
                // in that case, presume every single remaining object is a miss and did not give any combo score.
                totalDroppedScore = estimatedBestCaseTotal - estimateBestCaseComboTotal(scoreMaxCombo);
            }

            return estimatedBestCaseTotal == 0
                ? 1
                : 1 - Math.Clamp(totalDroppedScore / estimatedBestCaseTotal, 0, 1);

            double estimateBestCaseComboTotal(int maxCombo)
            {
                if (maxCombo == 0)
                    return 1;

                double estimatedTotal = 0.5 * Math.Min(maxCombo, 2);

                if (maxCombo <= 2)
                    return estimatedTotal;

                // int_2^x log_4(t) dt
                estimatedTotal += (Math.Min(maxCombo, 200) * (Math.Log(Math.Min(maxCombo, 200)) - 1) + 2 - Math.Log(4)) / Math.Log(4);

                if (maxCombo <= 200)
                    return estimatedTotal;

                estimatedTotal += (maxCombo - 200) * Math.Log(200) / Math.Log(4);
                return estimatedTotal;
            }

            double estimateDroppedComboScoreAfterMiss(int lengthOfComboAfterMiss)
            {
                if (lengthOfComboAfterMiss >= 200)
                    lengthOfComboAfterMiss = 200;

                // int_0^x (log_4(200) - log_4(t)) dt
                // note that this is an pessimistic estimate, i.e. it may subtract too much if the miss happened before reaching 200 combo
                return lengthOfComboAfterMiss * (1 + Math.Log(200) - Math.Log(lengthOfComboAfterMiss)) / Math.Log(4);
            }
        }

        public static double ComputeAccuracy(ScoreInfo scoreInfo, ScoreProcessor scoreProcessor)
            => ComputeAccuracy(scoreInfo.Statistics, scoreInfo.MaximumStatistics, scoreProcessor);

        public static double ComputeAccuracy(IReadOnlyDictionary<HitResult, int> statistics, IReadOnlyDictionary<HitResult, int> maximumStatistics, ScoreProcessor scoreProcessor)
        {
            int baseScore = statistics.Where(kvp => kvp.Key.AffectsAccuracy())
                                      .Sum(kvp => kvp.Value * scoreProcessor.GetBaseScoreForResult(kvp.Key));
            int maxBaseScore = maximumStatistics.Where(kvp => kvp.Key.AffectsAccuracy())
                                                .Sum(kvp => kvp.Value * scoreProcessor.GetBaseScoreForResult(kvp.Key));

            return maxBaseScore == 0 ? 1 : baseScore / (double)maxBaseScore;
        }

        public static ScoreRank ComputeRank(ScoreInfo scoreInfo) =>
            ComputeRank(scoreInfo.Accuracy, scoreInfo.Statistics, scoreInfo.Mods, scoreInfo.Ruleset.CreateInstance().CreateScoreProcessor());

        public static ScoreRank ComputeRank(ScoreInfo scoreInfo, ScoreProcessor processor) =>
            ComputeRank(scoreInfo.Accuracy, scoreInfo.Statistics, scoreInfo.Mods, processor);

        public static ScoreRank ComputeRank(double accuracy, IReadOnlyDictionary<HitResult, int> statistics, IList<Mod> mods, ScoreProcessor scoreProcessor)
        {
            var rank = scoreProcessor.RankFromScore(accuracy, statistics);

            foreach (var mod in mods.OfType<IApplicableToScoreProcessor>())
                rank = mod.AdjustRank(rank, accuracy);

            return rank;
        }

        /// <summary>
        /// Updates <paramref name="scoreInfo"/>'s <see cref="ScoreInfo.TotalScore"/> to the newest score multipliers
        /// using <see cref="ScoreInfo.TotalScoreWithoutMods"/> and <see cref="ScoreInfo.Mods"/>.
        /// </summary>
        /// <param name="scoreInfo">The score to update.</param>
        /// <param name="beatmapDifficultyWithoutMods">The difficulty parameters of the beatmap applicable for the score, before application of mods.</param>
        /// <exception cref="InvalidOperationException"><paramref name="scoreInfo"/> does not have <see cref="ScoreInfo.TotalScoreWithoutMods"/> correctly populated.</exception>
        public static void UpdateToLatestScoreMultipliers(ScoreInfo scoreInfo, IBeatmapDifficultyInfo beatmapDifficultyWithoutMods)
        {
            // do nothing if the score multiplier is already up to date.
            if (scoreInfo.TotalScoreVersion >= 30000017)
                return;

            if (scoreInfo.TotalScoreWithoutMods == 0 && scoreInfo.TotalScore > 0)
                throw new InvalidOperationException($"Cannot recalculate score multiplier as {nameof(scoreInfo.TotalScoreWithoutMods)} is missing.");

            var ruleset = scoreInfo.Ruleset.CreateInstance();
            var scoreMultiplierCalculator = ruleset.CreateScoreMultiplierCalculator(new ScoreMultiplierContext(beatmapDifficultyWithoutMods));
            double scoreMultiplier = scoreMultiplierCalculator.CalculateFor(scoreInfo.Mods);
            scoreInfo.TotalScore = (long)Math.Round(scoreInfo.TotalScoreWithoutMods * scoreMultiplier);
        }

        /// <summary>
        /// Used to populate the <paramref name="score"/> model using data parsed from its corresponding replay file.
        /// </summary>
        /// <param name="score">The score to run population from replay for.</param>
        /// <param name="files">A <see cref="RealmFileStore"/> instance to use for fetching replay.</param>
        /// <param name="populationFunc">
        /// Delegate describing the population to execute.
        /// The delegate's argument is a <see cref="SerializationReader"/> instance which permits to read data from the replay stream.
        /// </param>
        public static void PopulateFromReplay(this ScoreInfo score, RealmFileStore files, Action<SerializationReader> populationFunc)
        {
            string? replayFilename = score.Files.FirstOrDefault(f => f.Filename.EndsWith(@".osr", StringComparison.InvariantCultureIgnoreCase))?.File.GetStoragePath();
            if (replayFilename == null)
                return;

            try
            {
                using (var stream = files.Store.GetStream(replayFilename))
                {
                    if (stream == null)
                        return;

                    using (SerializationReader sr = new SerializationReader(stream))
                        populationFunc.Invoke(sr);
                }
            }
            catch (Exception e)
            {
                Logger.Error(e, $"Failed to read replay {replayFilename} during score migration", LoggingTarget.Database);
            }
        }
    }
}
