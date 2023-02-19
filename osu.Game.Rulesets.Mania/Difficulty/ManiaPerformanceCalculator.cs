// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics;
using MathNet.Numerics.Distributions;
using osu.Framework.Audio.Track;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;

namespace osu.Game.Rulesets.Mania.Difficulty
{
    public class ManiaPerformanceCalculator : PerformanceCalculator
    {
        private int countPerfect;
        private int countGreat;
        private int countGood;
        private int countOk;
        private int countMeh;
        private int countMiss;
        private double? estimatedUr;

        public ManiaPerformanceCalculator()
            : base(new ManiaRuleset())
        {
        }

        protected override PerformanceAttributes CreatePerformanceAttributes(ScoreInfo score, DifficultyAttributes attributes)
        {
            var maniaAttributes = (ManiaDifficultyAttributes)attributes;

            countPerfect = score.Statistics.GetValueOrDefault(HitResult.Perfect);
            countGreat = score.Statistics.GetValueOrDefault(HitResult.Great);
            countGood = score.Statistics.GetValueOrDefault(HitResult.Good);
            countOk = score.Statistics.GetValueOrDefault(HitResult.Ok);
            countMeh = score.Statistics.GetValueOrDefault(HitResult.Meh);
            countMiss = score.Statistics.GetValueOrDefault(HitResult.Miss);
            estimatedUr = computeEstimatedUr(score, maniaAttributes) * 10;

            // Arbitrary initial value for scaling pp in order to standardize distributions across game modes.
            // The specific number has no intrinsic meaning and can be adjusted as needed.
            double multiplier = 8.0;

            if (score.Mods.Any(m => m is ModNoFail))
                multiplier *= 0.75;
            if (score.Mods.Any(m => m is ModEasy))
                multiplier *= 0.5;

            double difficultyValue = computeDifficultyValue(maniaAttributes);
            double totalValue = difficultyValue * multiplier;

            return new ManiaPerformanceAttributes
            {
                Difficulty = difficultyValue,
                Total = totalValue,
                EstimatedUr = estimatedUr
            };
        }

        private double computeDifficultyValue(ManiaDifficultyAttributes attributes)
        {
            double difficultyValue = Math.Pow(Math.Max(attributes.StarRating - 0.15, 0.05), 2.2); // Star rating to pp curve

            if (estimatedUr == null)
                return 0;

            difficultyValue *= Math.Max(1.2 * Math.Pow(SpecialFunctions.Erf(300 / estimatedUr.Value), 1.6) - 0.2, 0); // UR to multiplier curve, see https://www.desmos.com/calculator/xt58vzt2y4

            return difficultyValue;
        }

        private double totalJudgements => countPerfect + countOk + countGreat + countGood + countMeh + countMiss;
        private double totalSuccessfulJudgements => countPerfect + countOk + countGreat + countGood + countMeh;

        /// <summary>
        /// Accuracy used to weight judgements independently from the score's actual accuracy.
        /// </summary>
        private double? computeEstimatedUr(ScoreInfo score, ManiaDifficultyAttributes attributes)
        {
            if (totalSuccessfulJudgements == 0)
                return null;

            bool isLegacyScore = score.Mods.Any(m => m is ModClassic) && totalJudgements == attributes.NoteCount + attributes.HoldNoteCount;

            double[] judgements = isLegacyScore ? getLegacyJudgements(score, attributes) : getLazerJudgements(score, attributes);

            double hMax = judgements[0];
            double h300 = judgements[1];
            double h200 = judgements[2];
            double h100 = judgements[3];
            double h50 = judgements[4];

            // https://www.desmos.com/calculator/tybjpjfjlz
            double legacyLikelihoodGradient(double d)
            {
                if (d <= 0)
                    return 0;

                double pMaxNote = logDiff(0, logPcNote(hMax, d));
                double p300Note = logDiff(logPcNote(hMax, d), logPcNote(h300, d));
                double p200Note = logDiff(logPcNote(h300, d), logPcNote(h200, d));
                double p100Note = logDiff(logPcNote(h200, d), logPcNote(h100, d));
                double p50Note = logDiff(logPcNote(h100, d), logPcNote(h50, d));
                double p0Note = logPcNote(h50, d);

                // Since we're using complementary probabilities for precision, multiplying the head and tail probabilities takes the form P(A∩B)' = P(A'∪B') = P(A') + P(B') - P(A'∩B').
                double combinedProb(double p1, double p2) => logDiff(logSum(p1, p2), p1 + p2);

                // The variable name 'logPc' means 'log probability complementary'.
                double logPcMaxHead = logPcNote(hMax * 1.2, d);
                double logPcMaxTail = logPcHitHoldTail(hMax * 2.4, d);
                double pMaxHold = logDiff(0, combinedProb(logPcMaxHead, logPcMaxTail));

                double logPc300Head = logPcNote(h300 * 1.1, d);
                double logPc300Tail = logPcHitHoldTail(h300 * 2.2, d);
                double p300Hold = logDiff(combinedProb(logPcMaxHead, logPcMaxTail), combinedProb(logPc300Head, logPc300Tail));

                double logPc200Head = logPcNote(h200, d);
                double logPc200Tail = logPcHitHoldTail(h200 * 2, d);
                double p200Hold = logDiff(combinedProb(logPc300Head, logPc300Tail), combinedProb(logPc200Head, logPc200Tail));

                double logPc100Head = logPcNote(h100, d);
                double logPc100Tail = logPcHitHoldTail(h100 * 2, d);
                double p100Hold = logDiff(combinedProb(logPc200Head, logPc200Tail), combinedProb(logPc100Head, logPc100Tail));

                double logPc50Head = logPcNote(h50, d);
                double logPc50Tail = logPcHitHoldTail(h50 * 2, d);
                double p50Hold = logDiff(combinedProb(logPc100Head, logPc100Tail), combinedProb(logPc50Head, logPc50Tail));

                double p0Hold = combinedProb(logPc50Head, logPc50Tail);

                double pMax = logSum(pMaxNote + Math.Log(attributes.NoteCount), pMaxHold + Math.Log(attributes.HoldNoteCount)) - Math.Log(totalJudgements);
                double p300 = logSum(p300Note + Math.Log(attributes.NoteCount), p300Hold + Math.Log(attributes.HoldNoteCount)) - Math.Log(totalJudgements);
                double p200 = logSum(p200Note + Math.Log(attributes.NoteCount), p200Hold + Math.Log(attributes.HoldNoteCount)) - Math.Log(totalJudgements);
                double p100 = logSum(p100Note + Math.Log(attributes.NoteCount), p100Hold + Math.Log(attributes.HoldNoteCount)) - Math.Log(totalJudgements);
                double p50 = logSum(p50Note + Math.Log(attributes.NoteCount), p50Hold + Math.Log(attributes.HoldNoteCount)) - Math.Log(totalJudgements);
                double p0 = logSum(p0Note + Math.Log(attributes.NoteCount), p0Hold + Math.Log(attributes.HoldNoteCount)) - Math.Log(totalJudgements);

                double gradient = Math.Exp(
                    (countPerfect * pMax
                     + (countGreat + 0.5) * p300
                     + countGood * p200
                     + countOk * p100
                     + countMeh * p50
                     + countMiss * p0) / totalJudgements
                );

                return -gradient;
            }

            // https://www.desmos.com/calculator/piqxqmnuks
            double lazerLikelihoodGradient(double d)
            {
                if (d <= 0)
                    return 0;

                double pMaxNote = logDiff(0, logPcNote(hMax, d));
                double p300Note = logDiff(logPcNote(hMax, d), logPcNote(h300, d));
                double p200Note = logDiff(logPcNote(h300, d), logPcNote(h200, d));
                double p100Note = logDiff(logPcNote(h200, d), logPcNote(h100, d));
                double p50Note = logDiff(logPcNote(h100, d), logPcNote(h50, d));
                double p0Note = logPcNote(h50, d);

                // Lazer LN tails are 1.5x the hit window, so calculate the probability of hitting them separately.
                // We don't use "logPcHitHoldTail` for these since they have the same hit mechanics as a regular note.
                double pMaxTail = logDiff(0, logPcNote(hMax * 1.5, d));
                double p300Tail = logDiff(logPcNote(hMax * 1.5, d), logPcNote(h300 * 1.5, d));
                double p200Tail = logDiff(logPcNote(h300 * 1.5, d), logPcNote(h200 * 1.5, d));
                double p100Tail = logDiff(logPcNote(h200 * 1.5, d), logPcNote(h100 * 1.5, d));
                double p50Tail = logDiff(logPcNote(h100 * 1.5, d), logPcNote(h50 * 1.5, d));
                double p0Tail = logPcNote(h50 * 1.5, d);

                double pMax = logSum(pMaxNote + Math.Log(attributes.NoteCount + attributes.HoldNoteCount), pMaxTail + Math.Log(attributes.HoldNoteCount)) - Math.Log(totalJudgements);
                double p300 = logSum(p300Note + Math.Log(attributes.NoteCount + attributes.HoldNoteCount), p300Tail + Math.Log(attributes.HoldNoteCount)) - Math.Log(totalJudgements);
                double p200 = logSum(p200Note + Math.Log(attributes.NoteCount + attributes.HoldNoteCount), p200Tail + Math.Log(attributes.HoldNoteCount)) - Math.Log(totalJudgements);
                double p100 = logSum(p100Note + Math.Log(attributes.NoteCount + attributes.HoldNoteCount), p100Tail + Math.Log(attributes.HoldNoteCount)) - Math.Log(totalJudgements);
                double p50 = logSum(p50Note + Math.Log(attributes.NoteCount + attributes.HoldNoteCount), p50Tail + Math.Log(attributes.HoldNoteCount)) - Math.Log(totalJudgements);
                double p0 = logSum(p0Note + Math.Log(attributes.NoteCount + attributes.HoldNoteCount), p0Tail + Math.Log(attributes.HoldNoteCount)) - Math.Log(totalJudgements);

                double gradient = Math.Exp(
                    (countPerfect * pMax
                     + (countGreat + 0.5) * p300
                     + countGood * p200
                     + countOk * p100
                     + countMeh * p50
                     + countMiss * p0) / totalJudgements
                );

                return -gradient;
            }

            // Finding the minimum of the function returns the most likely deviation for the hit results.
            return isLegacyScore ? FindMinimum.OfScalarFunction(legacyLikelihoodGradient, 10) : FindMinimum.OfScalarFunction(lazerLikelihoodGradient, 10);
        }

        private double[] getLegacyJudgements(ScoreInfo score, ManiaDifficultyAttributes attributes)
        {
            double[] judgements = new double[5];

            double overallDifficulty = attributes.OverallDifficulty;

            if (attributes.Convert)
                overallDifficulty = 10;

            double windowMultiplier = 1;

            if (score.Mods.Any(m => m is ModHardRock))
                windowMultiplier *= 1 / 1.4;
            else if (score.Mods.Any(m => m is ModEasy))
                windowMultiplier *= 1.4;

            judgements[0] = Math.Floor(16 * windowMultiplier);
            judgements[1] = Math.Floor((64 - 3 * overallDifficulty) * windowMultiplier);
            judgements[2] = Math.Floor((97 - 3 * overallDifficulty) * windowMultiplier);
            judgements[3] = Math.Floor((127 - 3 * overallDifficulty) * windowMultiplier);
            judgements[4] = Math.Floor((151 - 3 * overallDifficulty) * windowMultiplier);

            return judgements;
        }

        private double[] getLazerJudgements(ScoreInfo score, ManiaDifficultyAttributes attributes)
        {
            double[] judgements = new double[5];

            var track = new TrackVirtual(10000);
            score.Mods.OfType<IApplicableToTrack>().ForEach(m => m.ApplyToTrack(track));
            double clockRate = track.Rate;

            double windowMultiplier = 1 / clockRate;

            if (score.Mods.Any(m => m is ModHardRock))
                windowMultiplier *= 1 / 1.4;
            else if (score.Mods.Any(m => m is ModEasy))
                windowMultiplier *= 1.4;

            if (attributes.OverallDifficulty < 5)
                judgements[0] = (22.4 - 0.6 * attributes.OverallDifficulty) * windowMultiplier;
            else
                judgements[0] = (24.9 - 1.1 * attributes.OverallDifficulty) * windowMultiplier;
            judgements[1] = (64 - 3 * attributes.OverallDifficulty) * windowMultiplier;
            judgements[2] = (97 - 3 * attributes.OverallDifficulty) * windowMultiplier;
            judgements[3] = (127 - 3 * attributes.OverallDifficulty) * windowMultiplier;
            judgements[4] = (151 - 3 * attributes.OverallDifficulty) * windowMultiplier;

            return judgements;
        }

        private double logPcNote(double x, double deviation) => logErfcApprox(x / (deviation * Math.Sqrt(2)));
        private double logErfcApprox(double x) => x <= 5 ? Math.Log(SpecialFunctions.Erfc(x)) : -Math.Pow(x, 2) - Math.Log(x) - Math.Log(Math.Sqrt(Math.PI));

        // Legacy LN tails take the absolute error of both hit judgements on an LN, so we use a folded normal distribution to calculate it.
        private double logPcHitHoldTail(double x, double deviation) => Math.Log(1 - Math.Pow(2 * Normal.CDF(0, deviation * Math.Sqrt(2), x) - 1, 2));

        // Log rules make addition and subtraction of the non-log value non-trivial, these methods simply add and subtract the base value of logs.
        private double logDiff(double l1, double l2) => l1 + SpecialFunctions.Log1p(-Math.Exp(-(l1 - l2)));

        private double logSum(double l1, double l2)
        {
            double maxVal = Math.Max(l1, l2);
            double minVal = Math.Min(l1, l2);

            if (double.IsNegativeInfinity(maxVal))
            {
                return maxVal;
            }

            return maxVal + Math.Log(1 + Math.Exp(minVal - maxVal));
        }
    }
}
