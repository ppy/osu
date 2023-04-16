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
using osu.Game.Rulesets.Mania.Mods;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using Precision = osu.Framework.Utils.Precision;

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
        private double scoreAccuracy;
        private double? estimatedUr;
        private bool isLegacyScore;
        private double[] hitWindows;

        public ManiaPerformanceCalculator()
            : base(new ManiaRuleset())
        {
        }

        public new ManiaPerformanceAttributes Calculate(ScoreInfo score, DifficultyAttributes attributes)
            => (ManiaPerformanceAttributes)CreatePerformanceAttributes(score, attributes);

        protected override PerformanceAttributes CreatePerformanceAttributes(ScoreInfo score, DifficultyAttributes attributes)
        {
            var maniaAttributes = (ManiaDifficultyAttributes)attributes;

            countPerfect = score.Statistics.GetValueOrDefault(HitResult.Perfect);
            countGreat = score.Statistics.GetValueOrDefault(HitResult.Great);
            countGood = score.Statistics.GetValueOrDefault(HitResult.Good);
            countOk = score.Statistics.GetValueOrDefault(HitResult.Ok);
            countMeh = score.Statistics.GetValueOrDefault(HitResult.Meh);
            countMiss = score.Statistics.GetValueOrDefault(HitResult.Miss);
            scoreAccuracy = calculateCustomAccuracy();
            isLegacyScore = score.Mods.Any(m => m is ManiaModClassic) && !Precision.DefinitelyBigger(totalJudgements, maniaAttributes.NoteCount + maniaAttributes.HoldNoteCount);
            hitWindows = isLegacyScore ? getLegacyHitWindows(score, maniaAttributes) : getLazerHitWindows(score, maniaAttributes);
            estimatedUr = computeEstimatedUr(maniaAttributes);

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
                EstimatedUr = estimatedUr,
                HitWindows = hitWindows
            };
        }

        private double computeDifficultyValue(ManiaDifficultyAttributes attributes)
        {
            double difficultyValue = Math.Pow(Math.Max(attributes.StarRating - 0.15, 0.05), 2.2) // Star rating to pp curve
                                     * Math.Max(0, 5 * scoreAccuracy - 4) // From 80% accuracy, 1/20th of total pp is awarded per additional 1% accuracy
                                     * (1 + 0.1 * Math.Min(1, (attributes.NoteCount + attributes.HoldNoteCount) / 1500.0)); // Length bonus, capped at 1500 notes

            return difficultyValue;
        }

        private double totalJudgements => countPerfect + countOk + countGreat + countGood + countMeh + countMiss;
        private double totalSuccessfulJudgements => countPerfect + countOk + countGreat + countGood + countMeh;

        private double calculateCustomAccuracy()
        {
            if (totalJudgements == 0)
                return 0;

            return (countPerfect * 320 + countGreat * 300 + countGood * 200 + countOk * 100 + countMeh * 50) / (totalJudgements * 320);
        }

        /// <summary>
        /// Returns the estimated tapping deviation of the score, assuming the average hit location is in the center of the hit window.
        /// </summary>
        private double? computeEstimatedUr(ManiaDifficultyAttributes attributes)
        {
            if (totalSuccessfulJudgements == 0 || attributes.NoteCount + attributes.HoldNoteCount == 0)
                return null;

            // Lazer LN heads are the same as Notes, so return NoteCount + HoldNoteCount for lazer scores.
            double logNoteCount = isLegacyScore ? Math.Log(attributes.NoteCount) : Math.Log(attributes.NoteCount + attributes.HoldNoteCount);
            double logHoldCount = Math.Log(attributes.HoldNoteCount);

            double likelihoodGradient(double d)
            {
                if (d <= 0)
                    return 0;

                JudgementProbs pNotes = pNote(d);
                // Since lazer tails have the same hit behaviour as Notes, return pNote instead of pHold for them.
                JudgementProbs pHolds = isLegacyScore ? pHold(d) : pNote(d, tail_multiplier);

                return -totalProb(pNotes, pHolds, logNoteCount, logHoldCount);
            }

            // Finding the minimum of the function returns the most likely deviation for the hit results. UR is deviation * 10.
            double deviation = FindMinimum.OfScalarFunction(likelihoodGradient, 30);

            return deviation * 10;
        }

        private double[] getLegacyHitWindows(ScoreInfo score, ManiaDifficultyAttributes attributes)
        {
            double[] legacyHitWindows = new double[5];

            double overallDifficulty = attributes.OverallDifficulty;
            double greatWindowLeniency = 0;
            double goodWindowLeniency = 0;

            // When converting beatmaps to osu!mania in stable, the resulting hit window sizes are dependent on whether the beatmap's OD is above or below 4.
            if (attributes.IsConvert)
            {
                overallDifficulty = 10;

                if (attributes.OverallDifficulty <= 4)
                {
                    greatWindowLeniency = 13;
                    goodWindowLeniency = 10;
                }
            }

            double windowMultiplier = 1;

            if (score.Mods.Any(m => m is ModHardRock))
                windowMultiplier *= 1 / 1.4;
            else if (score.Mods.Any(m => m is ModEasy))
                windowMultiplier *= 1.4;

            legacyHitWindows[0] = Math.Floor(16 * windowMultiplier);
            legacyHitWindows[1] = Math.Floor((64 - 3 * overallDifficulty + greatWindowLeniency) * windowMultiplier);
            legacyHitWindows[2] = Math.Floor((97 - 3 * overallDifficulty + goodWindowLeniency) * windowMultiplier);
            legacyHitWindows[3] = Math.Floor((127 - 3 * overallDifficulty) * windowMultiplier);
            legacyHitWindows[4] = Math.Floor((151 - 3 * overallDifficulty) * windowMultiplier);

            return legacyHitWindows;
        }

        private double[] getLazerHitWindows(ScoreInfo score, ManiaDifficultyAttributes attributes)
        {
            double[] lazerHitWindows = new double[5];

            // Create a new track of arbitrary length, and apply the total rate change of every mod to the track (i.e. DT = 1.01-2x, HT = 0.5-0.99x)
            var track = new TrackVirtual(10000);
            score.Mods.OfType<IApplicableToTrack>().ForEach(m => m.ApplyToTrack(track));
            double clockRate = track.Rate;

            double windowMultiplier = 1 / clockRate;

            if (score.Mods.Any(m => m is ModHardRock))
                windowMultiplier *= 1 / 1.4;
            else if (score.Mods.Any(m => m is ModEasy))
                windowMultiplier *= 1.4;

            if (attributes.OverallDifficulty < 5)
                lazerHitWindows[0] = (22.4 - 0.6 * attributes.OverallDifficulty) * windowMultiplier;
            else
                lazerHitWindows[0] = (24.9 - 1.1 * attributes.OverallDifficulty) * windowMultiplier;
            lazerHitWindows[1] = (64 - 3 * attributes.OverallDifficulty) * windowMultiplier;
            lazerHitWindows[2] = (97 - 3 * attributes.OverallDifficulty) * windowMultiplier;
            lazerHitWindows[3] = (127 - 3 * attributes.OverallDifficulty) * windowMultiplier;
            lazerHitWindows[4] = (151 - 3 * attributes.OverallDifficulty) * windowMultiplier;

            return lazerHitWindows;
        }

        private struct JudgementProbs
        {
            public double PMax;
            public double P300;
            public double P200;
            public double P100;
            public double P50;
            public double P0;
        }

        // Probability of hitting a certain judgement on Notes given a deviation. The multiplier is for lazer LN tails, which are 1.5x as lenient.
        private JudgementProbs pNote(double d, double multiplier = 1)
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

        // Probability of hitting a certain judgement on legacy LNs, which have different hit behaviour to Notes and lazer LNs.
        private JudgementProbs pHold(double d)
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

        /// <summary>
        /// Combines pNotes and pHolds/pTails into a single probability value for each judgement, and compares them to the judgements of the play.
        /// </summary>
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

        /// <summary>
        /// The log complementary probability of hitting within a hit window with a certain deviation.
        /// </summary>
        /// <returns>
        /// A value from 0 (log of 1, 0% chance) to negative infinity (log of 0, 100% chance).
        /// </returns>
        private double logPcNote(double x, double deviation) => logErfc(x / (deviation * Math.Sqrt(2)));

        /// <summary>
        /// The log complementary probability of hitting within a hit window with a certain deviation.
        /// Exclusively for stable LN tails, as they give a result from 2 error values (total error on the head + the tail).
        /// </summary>
        /// <returns>
        /// A value from 0 (log of 1, 0% chance) to negative infinity (log of 0, 100% chance).
        /// </returns>
        private double logPcHoldTail(double x, double deviation) => logProbTail(x / (deviation * Math.Sqrt(2)));

        private double logErfc(double x) => x <= 5
            ? Math.Log(SpecialFunctions.Erfc(x))
            : -Math.Pow(x, 2) - Math.Log(x * Math.Sqrt(Math.PI)); // This is an approximation, https://www.desmos.com/calculator/kdbxwxgf01

        private double logProbTail(double x) => x <= 7
            ? Math.Log(1 - Math.Pow(2 * Normal.CDF(0, 1, x) - 1, 2))
            : Math.Log(2) - Math.Pow(x, 2) / 2 - Math.Log(x / Math.Sqrt(2) * Math.Sqrt(Math.PI)); // This is an approximation, https://www.desmos.com/calculator/lgwyhx0fxo

        private double logSum(double firstLog, double secondLog)
        {
            double maxVal = Math.Max(firstLog, secondLog);
            double minVal = Math.Min(firstLog, secondLog);

            // 0 in log form becomes negative infinity, so return negative infinity if both numbers are negative infinity.
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
