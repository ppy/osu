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

            if (score.TotalScoreVersion > 30000004)
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
            int maximumLegacyCombo = attributes.MaxCombo;

            long maximumLegacyBaseScore = maximumLegacyAccuracyScore + maximumLegacyComboScore;
            long maximumLegacyTotalScore = maximumLegacyBaseScore + maximumLegacyBonusScore;

            // The combo proportion is calculated as a proportion of maximumLegacyTotalScore.
            double comboProportion = (double)score.LegacyTotalScore / maximumLegacyTotalScore;

            // The bonus proportion makes up the rest of the score that exceeds maximumLegacyBaseScore.
            double bonusProportion = Math.Max(0, ((long)score.LegacyTotalScore - maximumLegacyBaseScore) * maximumLegacyBonusRatio);

            double modMultiplier = score.Mods.Select(m => m.ScoreMultiplier).Aggregate(1.0, (c, n) => c * n);

            switch (score.Ruleset.OnlineID)
            {
                case 0:
                    if (score.MaxCombo == 0 || score.Accuracy == 0)
                        return (long)Math.Round((
                            0
                            + 300000 * Math.Pow(score.Accuracy, 8)
                            + bonusProportion) * modMultiplier);

                    // Assumption :
                    // - sliders and slider-ticks are uniformly spread arround the beatmap
                    //      thus we can ignore them without losing much precision (consider a map of hit-circles only !)
                    // - the Ok/Meh hit results are uniformly spread in the score
                    //      thus we can simplify and consider each hit result to be score.Accuracy without losing much precision
                    // What is strippedV1/strippedV3 :
                    // This is the ComboScore of v1/v3 were we remove all (map-)constant multipliers and accuracy multipliers (including hit results),
                    // based on the previous assumptions. For Scorev1, this is basically the sum of squared combos (because without sliders: object_count == combo).
                    double maxStrippedV1 = Math.Pow(maximumLegacyCombo, 2);
                    double maxStrippedV3 = Math.Pow(maximumLegacyCombo, 1 + ScoreProcessor.COMBO_EXPONENT);

                    double strippedV1 = maxStrippedV1 * comboProportion / score.Accuracy;

                    double strippedV1FromMaxCombo = Math.Pow(score.MaxCombo, 2);
                    double strippedV3FromMaxCombo = Math.Pow(score.MaxCombo, 1 + ScoreProcessor.COMBO_EXPONENT);

                    // Compute approximate lower estimate scorev3 for that play
                    // That is, a play were we made biggest amount of big combos (Repeat MaxCombo + 1 remaining big combo)
                    // And didn't combo anything in the reminder of the map
                    double possibleMaxComboRepeat = Math.Floor(strippedV1 / strippedV1FromMaxCombo);
                    double strippedV1FromMaxComboRepeat = possibleMaxComboRepeat * strippedV1FromMaxCombo;
                    double remainingStrippedV1 = strippedV1 - strippedV1FromMaxComboRepeat;
                    double remainingCombo = Math.Sqrt(remainingStrippedV1);
                    double remainingStrippedV3 = Math.Pow(remainingCombo, 1 + ScoreProcessor.COMBO_EXPONENT);

                    double newLowerStrippedV3 = (possibleMaxComboRepeat * strippedV3FromMaxCombo) + remainingStrippedV3;

                    // Compute approximate upper estimate scorev3 for that play
                    // That is, a play were all combos were equal (except MaxCombo)
                    remainingStrippedV1 = strippedV1 - strippedV1FromMaxCombo;
                    double remainingComboObjects = maximumLegacyCombo - score.MaxCombo - score.Statistics[HitResult.Miss];
                    double remainingAverageCombo = remainingComboObjects > 0 ? remainingStrippedV1 / remainingComboObjects : 0;
                    remainingStrippedV3 = remainingComboObjects * Math.Pow(remainingAverageCombo, ScoreProcessor.COMBO_EXPONENT);

                    double newUpperStrippedV3 = strippedV3FromMaxCombo + remainingStrippedV3;

                    // Approximate by combining lower and upper estimates
                    // As the lower-estimate is very pessimistic, we use a 30/70 ratio
                    // And cap it with 1.2 times the middle-point to avoid overstimates
                    double strippedV3 = Math.Min(
                        0.3 * newLowerStrippedV3 + 0.7 * newUpperStrippedV3,
                        1.2 * (newLowerStrippedV3 + newUpperStrippedV3) / 2
                    );

                    double newComboScoreProportion = (strippedV3 / maxStrippedV3) * score.Accuracy;

                    return (long)Math.Round((
                        700000 * newComboScoreProportion * score.Accuracy
                        + 300000 * Math.Pow(score.Accuracy, 8)
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
                        990000 * comboProportion
                        + 10000 * Math.Pow(score.Accuracy, 2 + 2 * score.Accuracy)
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
