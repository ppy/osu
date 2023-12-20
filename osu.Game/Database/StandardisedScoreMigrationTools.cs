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
                                              .OrderByDescending(kvp => Judgement.ToNumericResult(kvp.Key))
                                              .SelectMany(kvp => Enumerable.Repeat(kvp.Key, kvp.Value))
                                              .ToList();

            // Attempt to use maximum statistics from the database.
            var maximumJudgements = score.MaximumStatistics
                                         .Where(kvp => kvp.Key.AffectsCombo())
                                         .OrderByDescending(kvp => Judgement.ToNumericResult(kvp.Key))
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
                (double)score.Statistics.Where(kvp => kvp.Key.AffectsAccuracy()).Sum(kvp => Judgement.ToNumericResult(kvp.Key) * kvp.Value)
                / score.MaximumStatistics.Where(kvp => kvp.Key.AffectsAccuracy()).Sum(kvp => Judgement.ToNumericResult(kvp.Key) * kvp.Value);
            double comboScore = (double)score.MaxCombo / score.MaximumStatistics.Where(kvp => kvp.Key.AffectsCombo()).Sum(kvp => kvp.Value);
            double bonusScore = score.Statistics.Where(kvp => kvp.Key.IsBonus()).Sum(kvp => Judgement.ToNumericResult(kvp.Key) * kvp.Value);

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
        }

        /// <summary>
        /// Converts from <see cref="ScoreInfo.LegacyTotalScore"/> to the new standardised scoring of <see cref="ScoreProcessor"/>.
        /// </summary>
        /// <param name="score">The score to convert the total score of.</param>
        /// <param name="beatmaps">A <see cref="BeatmapManager"/> used for <see cref="WorkingBeatmap"/> lookups.</param>
        /// <returns>The standardised total score.</returns>
        public static long ConvertFromLegacyTotalScore(ScoreInfo score, BeatmapManager beatmaps)
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

            return ConvertFromLegacyTotalScore(score, LegacyBeatmapConversionDifficultyInfo.FromBeatmap(beatmap.Beatmap), attributes);
        }

        /// <summary>
        /// Converts from <see cref="ScoreInfo.LegacyTotalScore"/> to the new standardised scoring of <see cref="ScoreProcessor"/>.
        /// </summary>
        /// <param name="score">The score to convert the total score of.</param>
        /// <param name="difficulty">The beatmap difficulty.</param>
        /// <param name="attributes">The legacy scoring attributes for the beatmap which the score was set on.</param>
        /// <returns>The standardised total score.</returns>
        public static long ConvertFromLegacyTotalScore(ScoreInfo score, LegacyBeatmapConversionDifficultyInfo difficulty, LegacyScoreAttributes attributes)
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
            double comboProportion =
                ((double)score.LegacyTotalScore - legacyAccScore) / (maximumLegacyComboScore + maximumLegacyBonusScore);

            // We assume the bonus proportion only makes up the rest of the score that exceeds maximumLegacyBaseScore.
            long maximumLegacyBaseScore = maximumLegacyAccuracyScore + maximumLegacyComboScore;
            double bonusProportion = Math.Max(0, ((long)score.LegacyTotalScore - maximumLegacyBaseScore) * maximumLegacyBonusRatio);

            double modMultiplier = score.Mods.Select(m => m.ScoreMultiplier).Aggregate(1.0, (c, n) => c * n);

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

                    return (long)Math.Round((
                        500000 * newComboScoreProportion * score.Accuracy
                        + 500000 * Math.Pow(score.Accuracy, 5)
                        + bonusProportion) * modMultiplier);

                case 1:
                    return (long)Math.Round((
                        250000 * comboProportion
                        + 750000 * Math.Pow(score.Accuracy, 3.6)
                        + bonusProportion) * modMultiplier);

                case 2:
                    return (long)Math.Round((
                        600000 * comboProportion
                        + 400000 * score.Accuracy
                        + bonusProportion) * modMultiplier);

                case 3:
                    return (long)Math.Round((
                        850000 * comboProportion
                        + 150000 * Math.Pow(score.Accuracy, 2 + 2 * score.Accuracy)
                        + bonusProportion) * modMultiplier);

                default:
                    return score.TotalScore;
            }
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
