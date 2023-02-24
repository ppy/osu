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
        private const double tail_multiplier = 1.5; // Lazer LN tails have 1.5x the hit window of a Note or an LN head.

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
            estimatedUr = computeEstimatedUr(score, maniaAttributes);

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
            if (totalSuccessfulJudgements == 0 || attributes.NoteCount + attributes.HoldNoteCount == 0)
                return null;

            bool isLegacyScore = score.Mods.Any(m => m is ModClassic) && totalJudgements == attributes.NoteCount + attributes.HoldNoteCount;

            double[] hitWindows = isLegacyScore ? getLegacyHitWindows(score, attributes) : getLazerHitWindows(score, attributes);

            double nNoteCount = Math.Log(attributes.NoteCount);
            double nHoldCount = Math.Log(attributes.HoldNoteCount);
            double nNoteHoldCount = Math.Log(attributes.NoteCount + attributes.HoldNoteCount);

            // Find the likelihood of a deviation resulting in the play's judgements. Higher is more likely, so we find the peak of the curve.
            double legacyLikelihoodGradient(double d)
            {
                if (d <= 0)
                    return 0;

                JudgementProbs pNotes = pNote(hitWindows, d);

                JudgementProbs pHolds = pHold(hitWindows, d);

                return -totalProb(pNotes, pHolds, nNoteCount, nHoldCount);
            }

            double lazerLikelihoodGradient(double d)
            {
                if (d <= 0)
                    return 0;

                JudgementProbs pNotes = pNote(hitWindows, d);

                // We use pNote instead of pHold because lazer tails behave the same as Notes.
                JudgementProbs pTails = pNote(hitWindows, d, tail_multiplier);

                return -totalProb(pNotes, pTails, nNoteHoldCount, nHoldCount);
            }

            // Finding the minimum of the function returns the most likely deviation for the hit results. UR is deviation * 10.
            double deviation = isLegacyScore ? FindMinimum.OfScalarFunction(legacyLikelihoodGradient, 30) : FindMinimum.OfScalarFunction(lazerLikelihoodGradient, 30);
            return deviation * 10;
        }

        private double[] getLegacyHitWindows(ScoreInfo score, ManiaDifficultyAttributes attributes)
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

        private double[] getLazerHitWindows(ScoreInfo score, ManiaDifficultyAttributes attributes)
        {
            double[] hitWindows = new double[5];

            // Create a new track of arbitrary length
            var track = new TrackVirtual(10000);
            // Apply the total rate change of every mod to the track (i.e. DT = 1.01-2x, HT = 0.5-0.99x)
            score.Mods.OfType<IApplicableToTrack>().ForEach(m => m.ApplyToTrack(track));
            // The final clock rate is the rate of the track
            double clockRate = track.Rate;

            double windowMultiplier = 1 / clockRate;

            if (score.Mods.Any(m => m is ModHardRock))
                windowMultiplier *= 1 / 1.4;
            else if (score.Mods.Any(m => m is ModEasy))
                windowMultiplier *= 1.4;

            if (attributes.OverallDifficulty < 5)
                hitWindows[0] = (22.4 - 0.6 * attributes.OverallDifficulty) * windowMultiplier;
            else
                hitWindows[0] = (24.9 - 1.1 * attributes.OverallDifficulty) * windowMultiplier;
            hitWindows[1] = (64 - 3 * attributes.OverallDifficulty) * windowMultiplier;
            hitWindows[2] = (97 - 3 * attributes.OverallDifficulty) * windowMultiplier;
            hitWindows[3] = (127 - 3 * attributes.OverallDifficulty) * windowMultiplier;
            hitWindows[4] = (151 - 3 * attributes.OverallDifficulty) * windowMultiplier;

            return hitWindows;
        }

        // This struct allows us to return the probability of hitting every judgement with a single method.
        private struct JudgementProbs
        {
            public double PMax;
            public double P300;
            public double P200;
            public double P100;
            public double P50;
            public double P0;
        }

        // This method finds the probability of hitting a certain judgement on Notes given a deviation. The multiplier is for lazer LN tails, which are 1.5x as lenient.
        private JudgementProbs pNote(double[] hitWindows, double d, double multiplier = 1)
        {
            JudgementProbs probabilities = new JudgementProbs
            {
                PMax = logDiff(0, logPcNote(hitWindows[0] * multiplier, d)),
                P300 = logDiff(logPcNote(hitWindows[0] * multiplier, d), logPcNote(hitWindows[1] * multiplier, d)),
                P200 = logDiff(logPcNote(hitWindows[1] * multiplier, d), logPcNote(hitWindows[2] * multiplier, d)),
                P100 = logDiff(logPcNote(hitWindows[2] * multiplier, d), logPcNote(hitWindows[3] * multiplier, d)),
                P50 = logDiff(logPcNote(hitWindows[3] * multiplier, d), logPcNote(hitWindows[4] * multiplier, d)),
                P0 = logPcNote(hitWindows[4] * multiplier, d)
            };

            return probabilities;
        }

        // This method finds the probability of hitting a certain judgement on legacy LNs, which have different hit behaviour to Notes and lazer LNs.
        private JudgementProbs pHold(double[] hitWindows, double d)
        {
            JudgementProbs probabilities = new JudgementProbs();

            // Since we're using complementary probabilities for precision, multiplying the head and tail probabilities takes the form P(A∩B)' = P(A'∪B') = P(A') + P(B') - P(A'∩B').
            double combinedProb(double p1, double p2) => logDiff(logSum(p1, p2), p1 + p2);

            double logPcMaxHead = logPcNote(hitWindows[0] * 1.2, d);
            double logPcMaxTail = logPcHoldTail(hitWindows[0] * 2.4, d);
            probabilities.PMax = logDiff(0, combinedProb(logPcMaxHead, logPcMaxTail));

            double logPc300Head = logPcNote(hitWindows[1] * 1.1, d);
            double logPc300Tail = logPcHoldTail(hitWindows[1] * 2.2, d);
            probabilities.P300 = logDiff(combinedProb(logPcMaxHead, logPcMaxTail), combinedProb(logPc300Head, logPc300Tail));

            double logPc200Head = logPcNote(hitWindows[2], d);
            double logPc200Tail = logPcHoldTail(hitWindows[2] * 2, d);
            probabilities.P200 = logDiff(combinedProb(logPc300Head, logPc300Tail), combinedProb(logPc200Head, logPc200Tail));

            double logPc100Head = logPcNote(hitWindows[3], d);
            double logPc100Tail = logPcHoldTail(hitWindows[3] * 2, d);
            probabilities.P100 = logDiff(combinedProb(logPc200Head, logPc200Tail), combinedProb(logPc100Head, logPc100Tail));

            double logPc50Head = logPcNote(hitWindows[4], d);
            double logPc50Tail = logPcHoldTail(hitWindows[4] * 2, d);
            probabilities.P50 = logDiff(combinedProb(logPc100Head, logPc100Tail), combinedProb(logPc50Head, logPc50Tail));

            probabilities.P0 = combinedProb(logPc50Head, logPc50Tail);

            return probabilities;
        }

        // Combines pNotes and pHolds/pTails into 1 probability value for each judgement, and compares it to the judgements of the play. A higher output means the deviation is more likely.
        private double totalProb(JudgementProbs firstProbs, JudgementProbs secondProbs, double firstObjectCount, double secondObjectCount)
        {
            // firstObjectCount can be either Notes, or Notes + Holds, as stable LN heads don't behave like Notes but lazer LN heads do.
            double pMax = logSum(firstProbs.PMax + firstObjectCount, secondProbs.PMax + secondObjectCount) - Math.Log(totalJudgements);
            double p300 = logSum(firstProbs.P300 + firstObjectCount, secondProbs.P300 + secondObjectCount) - Math.Log(totalJudgements);
            double p200 = logSum(firstProbs.P200 + firstObjectCount, secondProbs.P200 + secondObjectCount) - Math.Log(totalJudgements);
            double p100 = logSum(firstProbs.P100 + firstObjectCount, secondProbs.P100 + secondObjectCount) - Math.Log(totalJudgements);
            double p50 = logSum(firstProbs.P50 + firstObjectCount, secondProbs.P50 + secondObjectCount) - Math.Log(totalJudgements);
            double p0 = logSum(firstProbs.P0 + firstObjectCount, secondProbs.P0 + secondObjectCount) - Math.Log(totalJudgements);

            double totalProb = Math.Exp(
                (countPerfect * pMax
                 + (countGreat + 0.5) * p300
                 + countGood * p200
                 + countOk * p100
                 + countMeh * p50
                 + countMiss * p0) / totalJudgements
            );

            return totalProb;
        }

        private double logPcNote(double x, double deviation) => logErfcApprox(x / (deviation * Math.Sqrt(2)));

        // Legacy LN tails take the absolute error of both hit judgements on an LN, so we use a folded normal distribution to calculate it.
        private double logPcHoldTail(double x, double deviation) => holdTailApprox(x / (deviation * Math.Sqrt(2)));

        private double logErfcApprox(double x) => x <= 5
            ? Math.Log(SpecialFunctions.Erfc(x))
            : -Math.Pow(x, 2) - Math.Log(x) - Math.Log(Math.Sqrt(Math.PI)); // https://www.desmos.com/calculator/aaftj14euk

        private double holdTailApprox(double x) => x <= 7
            ? Math.Log(1 - Math.Pow(2 * Normal.CDF(0, 1, x) - 1, 2))
            : Math.Log(2) - Math.Pow(x, 2) / 2 - Math.Log(x / Math.Sqrt(2) * Math.Sqrt(Math.PI)); // https://www.desmos.com/calculator/lgwyhx0fxo

        // Log rules make addition and subtraction of the non-log value non-trivial, these methods simply add and subtract the base value of logs.
        private double logSum(double firstLog, double secondLog)
        {
            double maxVal = Math.Max(firstLog, secondLog);
            double minVal = Math.Min(firstLog, secondLog);

            // 0 in log form becomes negative infinity, so return negative infinity if both numbers are negative infinity.
            // Shouldn't happen on any UR>0, but good for redundancy purposes.
            if (double.IsNegativeInfinity(maxVal))
            {
                return maxVal;
            }

            return maxVal + Math.Log(1 + Math.Exp(minVal - maxVal));
        }

        private double logDiff(double firstLog, double secondLog)
        {
            double maxVal = Math.Max(firstLog, secondLog);

            // Avoid negative infinity - negative infinity (NaN) by checking if the higher value is negative infinity. See comment in logSum.
            if (double.IsNegativeInfinity(maxVal))
            {
                return maxVal;
            }

            return firstLog + SpecialFunctions.Log1p(-Math.Exp(-(firstLog - secondLog)));
        }
    }
}
