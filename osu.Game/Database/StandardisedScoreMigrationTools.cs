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
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Scoring.Legacy;
using osu.Game.Scoring;

namespace osu.Game.Database
{
    public static class StandardisedScoreMigrationTools
    {
        public static bool ShouldMigrateToNewStandardised(ScoreInfo score)
        {
            if (score.IsLegacyScore)
                return false;

            if (score.TotalScoreVersion > 30000002)
                return false;

            // Recalculate the old-style standardised score to see if this was an old lazer score.
            bool oldScoreMatchesExpectations = GetOldStandardised(score) == score.TotalScore;
            // Some older scores don't have correct statistics populated, so let's give them benefit of doubt.
            bool scoreIsVeryOld = score.Date < new DateTime(2023, 1, 1, 0, 0, 0);

            return oldScoreMatchesExpectations || scoreIsVeryOld;
        }

        public static long GetNewStandardised(ScoreInfo score)
        {
            int maxJudgementIndex = 0;

            // Avoid retrieving from realm inside loops.
            int maxCombo = score.MaxCombo;

            var ruleset = score.Ruleset.CreateInstance();
            var processor = ruleset.CreateScoreProcessor();

            processor.TrackHitEvents = false;

            var beatmap = new Beatmap();

            HitResult maxRulesetJudgement = ruleset.GetHitResults().First().result;

            // This is a list of all results, ordered from best to worst.
            // We are constructing a "best possible" score from the statistics provided because it's the best we can do.
            List<HitResult> sortedHits = score.Statistics
                                              .Where(kvp => kvp.Key.AffectsCombo())
                                              .OrderByDescending(kvp => processor.GetBaseScoreForResult(kvp.Key))
                                              .SelectMany(kvp => Enumerable.Repeat(kvp.Key, kvp.Value))
                                              .ToList();

            // Attempt to use maximum statistics from the database.
            var maximumJudgements = score.MaximumStatistics
                                         .Where(kvp => kvp.Key.AffectsCombo())
                                         .OrderByDescending(kvp => processor.GetBaseScoreForResult(kvp.Key))
                                         .SelectMany(kvp => Enumerable.Repeat(new FakeJudgement(kvp.Key), kvp.Value))
                                         .ToList();

            // Some older scores may not have maximum statistics populated correctly.
            // In this case we need to fill them with best-known-defaults.
            if (maximumJudgements.Count != sortedHits.Count)
            {
                maximumJudgements = sortedHits
                                    .Select(r => new FakeJudgement(getMaxJudgementFor(r, maxRulesetJudgement)))
                                    .ToList();
            }

            // This is required to get the correct maximum combo portion.
            foreach (var judgement in maximumJudgements)
                beatmap.HitObjects.Add(new FakeHit(judgement));
            processor.ApplyBeatmap(beatmap);
            processor.Mods.Value = score.Mods;

            // Insert all misses into a queue.
            // These will be nibbled at whenever we need to reset the combo.
            Queue<HitResult> misses = new Queue<HitResult>(score.Statistics
                                                                .Where(kvp => kvp.Key == HitResult.Miss || kvp.Key == HitResult.LargeTickMiss)
                                                                .SelectMany(kvp => Enumerable.Repeat(kvp.Key, kvp.Value)));

            foreach (var result in sortedHits)
            {
                // For the main part of this loop, ignore all misses, as they will be inserted from the queue.
                if (result == HitResult.Miss || result == HitResult.LargeTickMiss)
                    continue;

                // Reset combo if required.
                if (processor.Combo.Value == maxCombo)
                    insertMiss();

                processor.ApplyResult(new JudgementResult(null!, maximumJudgements[maxJudgementIndex++])
                {
                    Type = result
                });
            }

            // Ensure we haven't forgotten any misses.
            while (misses.Count > 0)
                insertMiss();

            var bonusHits = score.Statistics
                                 .Where(kvp => kvp.Key.IsBonus())
                                 .SelectMany(kvp => Enumerable.Repeat(kvp.Key, kvp.Value));

            foreach (var result in bonusHits)
                processor.ApplyResult(new JudgementResult(null!, new FakeJudgement(result)) { Type = result });

            // Not true for all scores for whatever reason. Oh well.
            // Debug.Assert(processor.HighestCombo.Value == score.MaxCombo);

            return processor.TotalScore.Value;

            void insertMiss()
            {
                if (misses.Count > 0)
                {
                    processor.ApplyResult(new JudgementResult(null!, maximumJudgements[maxJudgementIndex++])
                    {
                        Type = misses.Dequeue(),
                    });
                }
                else
                {
                    // We ran out of misses. But we can't let max combo increase beyond the known value,
                    // so let's forge a miss.
                    processor.ApplyResult(new JudgementResult(null!, new FakeJudgement(getMaxJudgementFor(HitResult.Miss, maxRulesetJudgement)))
                    {
                        Type = HitResult.Miss,
                    });
                }
            }
        }

        private static HitResult getMaxJudgementFor(HitResult hitResult, HitResult max)
        {
            switch (hitResult)
            {
                case HitResult.Miss:
                case HitResult.Meh:
                case HitResult.Ok:
                case HitResult.Good:
                case HitResult.Great:
                case HitResult.Perfect:
                    return max;

                case HitResult.SmallTickMiss:
                case HitResult.SmallTickHit:
                    return HitResult.SmallTickHit;

                case HitResult.LargeTickMiss:
                case HitResult.LargeTickHit:
                    return HitResult.LargeTickHit;
            }

            return HitResult.IgnoreHit;
        }

        public static long GetOldStandardised(ScoreInfo score)
        {
            double accuracyScore =
                (double)score.Statistics.Where(kvp => kvp.Key.AffectsAccuracy()).Sum(kvp => numericScoreFor(kvp.Key) * kvp.Value)
                / score.MaximumStatistics.Where(kvp => kvp.Key.AffectsAccuracy()).Sum(kvp => numericScoreFor(kvp.Key) * kvp.Value);
            double comboScore = (double)score.MaxCombo / score.MaximumStatistics.Where(kvp => kvp.Key.AffectsCombo()).Sum(kvp => kvp.Value);
            double bonusScore = score.Statistics.Where(kvp => kvp.Key.IsBonus()).Sum(kvp => numericScoreFor(kvp.Key) * kvp.Value);

            double accuracyPortion = 0.3;

            switch (score.RulesetID)
            {
                case 1:
                    accuracyPortion = 0.75;
                    break;

                case 3:
                    accuracyPortion = 0.99;
                    break;
            }

            double modMultiplier = 1;

            foreach (var mod in score.Mods)
                modMultiplier *= mod.ScoreMultiplier;

            return (long)Math.Round((1000000 * (accuracyPortion * accuracyScore + (1 - accuracyPortion) * comboScore) + bonusScore) * modMultiplier);

            static int numericScoreFor(HitResult result)
            {
                switch (result)
                {
                    default:
                        return 0;

                    case HitResult.SmallTickHit:
                        return 10;

                    case HitResult.LargeTickHit:
                        return 30;

                    case HitResult.Meh:
                        return 50;

                    case HitResult.Ok:
                        return 100;

                    case HitResult.Good:
                        return 200;

                    case HitResult.Great:
                        return 300;

                    case HitResult.Perfect:
                        return 315;

                    case HitResult.SmallBonus:
                        return 10;

                    case HitResult.LargeBonus:
                        return 50;
                }
            }
        }

        /// <summary>
        /// Updates a legacy <see cref="ScoreInfo"/> to standardised scoring.
        /// </summary>
        /// <param name="score">The score to update.</param>
        /// <param name="beatmaps">A <see cref="BeatmapManager"/> used for <see cref="WorkingBeatmap"/> lookups.</param>
        public static void UpdateFromLegacy(ScoreInfo score, BeatmapManager beatmaps)
        {
            score.TotalScore = convertFromLegacyTotalScore(score, beatmaps);
            score.Accuracy = ComputeAccuracy(score);
        }

        /// <summary>
        /// Updates a legacy <see cref="ScoreInfo"/> to standardised scoring.
        /// </summary>
        /// <param name="score">The score to update.</param>
        /// <param name="difficulty">The beatmap difficulty.</param>
        /// <param name="attributes">The legacy scoring attributes for the beatmap which the score was set on.</param>
        public static void UpdateFromLegacy(ScoreInfo score, LegacyBeatmapConversionDifficultyInfo difficulty, LegacyScoreAttributes attributes)
        {
            score.TotalScore = convertFromLegacyTotalScore(score, difficulty, attributes);
            score.Accuracy = ComputeAccuracy(score);
        }

        /// <summary>
        /// Converts from <see cref="ScoreInfo.LegacyTotalScore"/> to the new standardised scoring of <see cref="ScoreProcessor"/>.
        /// </summary>
        /// <param name="score">The score to convert the total score of.</param>
        /// <param name="beatmaps">A <see cref="BeatmapManager"/> used for <see cref="WorkingBeatmap"/> lookups.</param>
        /// <returns>The standardised total score.</returns>
        private static long convertFromLegacyTotalScore(ScoreInfo score, BeatmapManager beatmaps)
        {
            if (!score.IsLegacyScore)
                return score.TotalScore;

            WorkingBeatmap beatmap = beatmaps.GetWorkingBeatmap(score.BeatmapInfo);
            Ruleset ruleset = score.Ruleset.CreateInstance();

            if (ruleset is not ILegacyRuleset legacyRuleset)
                return score.TotalScore;

            var mods = score.Mods;
            if (mods.Any(mod => mod is ModScoreV2))
                return score.TotalScore;

            var playableBeatmap = beatmap.GetPlayableBeatmap(ruleset.RulesetInfo, score.Mods);

            if (playableBeatmap.HitObjects.Count == 0)
                throw new InvalidOperationException("Beatmap contains no hit objects!");

            ILegacyScoreSimulator sv1Simulator = legacyRuleset.CreateLegacyScoreSimulator();
            LegacyScoreAttributes attributes = sv1Simulator.Simulate(beatmap, playableBeatmap);

            return convertFromLegacyTotalScore(score, LegacyBeatmapConversionDifficultyInfo.FromBeatmap(beatmap.Beatmap), attributes);
        }

        /// <summary>
        /// Converts from <see cref="ScoreInfo.LegacyTotalScore"/> to the new standardised scoring of <see cref="ScoreProcessor"/>.
        /// </summary>
        /// <param name="score">The score to convert the total score of.</param>
        /// <param name="difficulty">The beatmap difficulty.</param>
        /// <param name="attributes">The legacy scoring attributes for the beatmap which the score was set on.</param>
        /// <returns>The standardised total score.</returns>
        private static long convertFromLegacyTotalScore(ScoreInfo score, LegacyBeatmapConversionDifficultyInfo difficulty, LegacyScoreAttributes attributes)
        {
            if (!score.IsLegacyScore)
                return score.TotalScore;

            Debug.Assert(score.LegacyTotalScore != null);

            Ruleset ruleset = score.Ruleset.CreateInstance();
            if (ruleset is not ILegacyRuleset legacyRuleset)
                return score.TotalScore;

            double legacyModMultiplier = legacyRuleset.CreateLegacyScoreSimulator().GetLegacyScoreMultiplier(score.Mods, difficulty);
            int maximumLegacyAccuracyScore = attributes.AccuracyScore;
            long maximumLegacyComboScore = (long)Math.Round(attributes.ComboScore * legacyModMultiplier);
            double maximumLegacyBonusRatio = attributes.BonusScoreRatio;
            long maximumLegacyBonusScore = attributes.BonusScore;

            double legacyAccScore = maximumLegacyAccuracyScore * score.Accuracy;
            // We can not separate the ComboScore from the BonusScore, so we keep the bonus in the ratio.
            // Note that `maximumLegacyComboScore + maximumLegacyBonusScore` can actually be 0
            // when playing a beatmap with no bonus objects, with mods that have a 0.0x multiplier on stable (relax/autopilot).
            // In such cases, just assume 0.
            double comboProportion = maximumLegacyComboScore + maximumLegacyBonusScore > 0
                ? Math.Max((double)score.LegacyTotalScore - legacyAccScore, 0) / (maximumLegacyComboScore + maximumLegacyBonusScore)
                : 0;

            // We assume the bonus proportion only makes up the rest of the score that exceeds maximumLegacyBaseScore.
            long maximumLegacyBaseScore = maximumLegacyAccuracyScore + maximumLegacyComboScore;
            double bonusProportion = Math.Max(0, ((long)score.LegacyTotalScore - maximumLegacyBaseScore) * maximumLegacyBonusRatio);

            double modMultiplier = score.Mods.Select(m => m.ScoreMultiplier).Aggregate(1.0, (c, n) => c * n);

            long convertedTotalScore;

            switch (score.Ruleset.OnlineID)
            {
                case 0:
                    if (score.MaxCombo == 0 || score.Accuracy == 0)
                    {
                        return (long)Math.Round((
                            0
                            + 500000 * Math.Pow(score.Accuracy, 5)
                            + bonusProportion) * modMultiplier);
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
                    // This is a pessimistic estimate; it intentionally does not operate on object count and uses only score instead.
                    double maximumOccurrencesOfLongestCombo = Math.Floor(comboPortionInScoreV1 / comboPortionFromLongestComboInScoreV1);
                    double comboPortionFromRepeatedLongestCombosInScoreV1 = maximumOccurrencesOfLongestCombo * comboPortionFromLongestComboInScoreV1;

                    double remainingComboPortionInScoreV1 = comboPortionInScoreV1 - comboPortionFromRepeatedLongestCombosInScoreV1;
                    // `remainingComboPortionInScoreV1` is in the "score ballpark" realm, which means it's proportional to combo squared.
                    // To convert that back to a raw combo length, we need to take the square root...
                    double remainingCombo = Math.Sqrt(remainingComboPortionInScoreV1);
                    // ...and then based on that raw combo length, we calculate how much this last combo is worth in standardised score.
                    double remainingComboPortionInStandardisedScore = Math.Pow(remainingCombo, 1 + ScoreProcessor.COMBO_EXPONENT);

                    double lowerEstimateOfComboPortionInStandardisedScore
                        = maximumOccurrencesOfLongestCombo * comboPortionFromLongestComboInStandardisedScore
                          + remainingComboPortionInStandardisedScore;

                    // Compute approximate upper estimate new score for that play.
                    // This time, divide the remaining combo among remaining objects equally to achieve longest possible combo lengths.
                    // There is no rigorous proof that doing this will yield a correct upper bound, but it seems to work out in practice.
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

                    double upperEstimateOfComboPortionInStandardisedScore = comboPortionFromLongestComboInStandardisedScore + remainingComboPortionInStandardisedScore;

                    // Approximate by combining lower and upper estimates.
                    // As the lower-estimate is very pessimistic, we use a 30/70 ratio
                    // and cap it with 1.2 times the middle-point to avoid overestimates.
                    double estimatedComboPortionInStandardisedScore = Math.Min(
                        0.3 * lowerEstimateOfComboPortionInStandardisedScore + 0.7 * upperEstimateOfComboPortionInStandardisedScore,
                        1.2 * (lowerEstimateOfComboPortionInStandardisedScore + upperEstimateOfComboPortionInStandardisedScore) / 2
                    );

                    double newComboScoreProportion = estimatedComboPortionInStandardisedScore / maximumAchievableComboPortionInStandardisedScore;

                    convertedTotalScore = (long)Math.Round((
                        500000 * newComboScoreProportion * score.Accuracy
                        + 500000 * Math.Pow(score.Accuracy, 5)
                        + bonusProportion) * modMultiplier);
                    break;

                case 1:
                    convertedTotalScore = (long)Math.Round((
                        250000 * comboProportion
                        + 750000 * Math.Pow(score.Accuracy, 3.6)
                        + bonusProportion) * modMultiplier);
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

                    convertedTotalScore = (long)Math.Round((
                        comboPortion * estimateComboProportionForCatch(attributes.MaxCombo, score.MaxCombo, score.Statistics.GetValueOrDefault(HitResult.Miss))
                        + dropletsPortion * dropletsHit
                        + bonusProportion) * modMultiplier);
                    break;

                case 3:
                    convertedTotalScore = (long)Math.Round((
                        850000 * comboProportion
                        + 150000 * Math.Pow(score.Accuracy, 2 + 2 * score.Accuracy)
                        + bonusProportion) * modMultiplier);
                    break;

                default:
                    convertedTotalScore = score.TotalScore;
                    break;
            }

            if (convertedTotalScore < 0)
                throw new InvalidOperationException($"Total score conversion operation returned invalid total of {convertedTotalScore}");

            return convertedTotalScore;
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
                while (remainingCombo > 0)
                {
                    int comboLength = Math.Min(assumedLengthOfRemainingCombos, remainingCombo);

                    remainingCombo -= comboLength;
                    totalDroppedScore += estimateDroppedComboScoreAfterMiss(comboLength);
                }
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

        public static double ComputeAccuracy(ScoreInfo scoreInfo)
        {
            Ruleset ruleset = scoreInfo.Ruleset.CreateInstance();
            ScoreProcessor scoreProcessor = ruleset.CreateScoreProcessor();

            int baseScore = scoreInfo.Statistics.Where(kvp => kvp.Key.AffectsAccuracy())
                                     .Sum(kvp => kvp.Value * scoreProcessor.GetBaseScoreForResult(kvp.Key));
            int maxBaseScore = scoreInfo.MaximumStatistics.Where(kvp => kvp.Key.AffectsAccuracy())
                                        .Sum(kvp => kvp.Value * scoreProcessor.GetBaseScoreForResult(kvp.Key));

            return maxBaseScore == 0 ? 1 : baseScore / (double)maxBaseScore;
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

        private class FakeHit : HitObject
        {
            private readonly Judgement judgement;

            public override Judgement CreateJudgement() => judgement;

            public FakeHit(Judgement judgement)
            {
                this.judgement = judgement;
            }
        }

        private class FakeJudgement : Judgement
        {
            public override HitResult MaxResult { get; }

            public FakeJudgement(HitResult maxResult)
            {
                MaxResult = maxResult;
            }
        }
    }
}
