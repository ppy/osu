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

        private double totalHits => countPerfect + countOk + countGreat + countGood + countMeh + countMiss;
        private double totalSuccessfulHits => countPerfect + countOk + countGreat + countGood + countMeh;

        /// <summary>
        /// Accuracy used to weight judgements independently from the score's actual accuracy.
        /// </summary>
        private double? computeEstimatedUr(ScoreInfo score, ManiaDifficultyAttributes attributes)
        {
            if (totalSuccessfulHits == 0)
                return null;

            bool isLegacyScore = score.Mods.Any(m => m is ModClassic) && totalHits == attributes.NoteCount + attributes.HoldNoteCount;

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

                double pMaxNote = hitProb(hMax, d);
                double p300Note = hitProb(h300, d) - hitProb(hMax, d);
                double p200Note = hitProb(h200, d) - hitProb(h300, d);
                double p100Note = hitProb(h100, d) - hitProb(h200, d);
                double p50Note = hitProb(h50, d) - hitProb(h100, d);
                double p0Note = 1 - hitProb(h50, d);

                // Since long notes only give a specific judgement if both both hits end up within a certain hit window,
                // multiply the probability of hitting in the head hit window by the probability of hitting in the tail hit window.
                // Since legacy LN tails take the absolute error of both hit judgements on an LN, we need to use a folded normal distribution to calculate it.
                double pMaxLn = hitProb(hMax * 1.2, d) * hitProbLn(hMax * 2.4, d);

                double p300Ln = hitProb(h300 * 1.1, d) * hitProbLn(h300 * 2.2, d)
                                - hitProb(hMax * 1.2, d) * hitProbLn(hMax * 2.4, d);

                double p200Ln = hitProb(h200, d) * hitProbLn(h200 * 2, d)
                                - hitProb(h300 * 1.1, d) * hitProbLn(h300 * 2.2, d);

                double p100Ln = hitProb(h100, d) * hitProbLn(h100 * 2, d)
                                - hitProb(h200, d) * hitProbLn(h200 * 2, d);

                double p50Ln = hitProb(h50, d) * hitProbLn(h50 * 2, d)
                               - hitProb(h100, d) * hitProbLn(h100 * 2, d);

                double p0Ln = 1 - hitProb(h50, d) * hitProbLn(h50 * 2, d);

                double pMax = (pMaxNote * attributes.NoteCount + pMaxLn * attributes.HoldNoteCount) / totalHits;
                double p300 = (p300Note * attributes.NoteCount + p300Ln * attributes.HoldNoteCount) / totalHits;
                double p200 = (p200Note * attributes.NoteCount + p200Ln * attributes.HoldNoteCount) / totalHits;
                double p100 = (p100Note * attributes.NoteCount + p100Ln * attributes.HoldNoteCount) / totalHits;
                double p50 = (p50Note * attributes.NoteCount + p50Ln * attributes.HoldNoteCount) / totalHits;
                double p0 = (p0Note * attributes.NoteCount + p0Ln * attributes.HoldNoteCount) / totalHits;

                double gradient = Math.Pow(pMax, countPerfect / totalHits)
                                  * Math.Pow(p300, (countGreat + 0.5) / totalHits)
                                  * Math.Pow(p200, countGood / totalHits)
                                  * Math.Pow(p100, countOk / totalHits)
                                  * Math.Pow(p50, countMeh / totalHits)
                                  * Math.Pow(p0, countMiss / totalHits);

                return -gradient;
            }

            // https://www.desmos.com/calculator/piqxqmnuks
            double lazerLikelihoodGradient(double d)
            {
                if (d <= 0)
                    return 0;

                double pMaxNote = hitProb(hMax, d);
                double p300Note = hitProb(h300, d) - hitProb(hMax, d);
                double p200Note = hitProb(h200, d) - hitProb(h300, d);
                double p100Note = hitProb(h100, d) - hitProb(h200, d);
                double p50Note = hitProb(h50, d) - hitProb(h100, d);
                double p0Note = 1 - hitProb(h50, d);

                // Lazer LN tails are 1.5x the hit window, so calculate the probability of hitting them separately.
                double pMaxTail = hitProb(hMax * 1.5, d);
                double p300Tail = hitProb(h300 * 1.5, d) - hitProb(hMax * 1.5, d);
                double p200Tail = hitProb(h200 * 1.5, d) - hitProb(h300 * 1.5, d);
                double p100Tail = hitProb(h100 * 1.5, d) - hitProb(h200 * 1.5, d);
                double p50Tail = hitProb(h50 * 1.5, d) - hitProb(h100 * 1.5, d);
                double p0Tail = 1 - hitProb(h50 * 1.5, d);

                double pMax = (pMaxNote * (attributes.NoteCount + attributes.HoldNoteCount) + pMaxTail * attributes.HoldNoteCount) / totalHits;
                double p300 = (p300Note * (attributes.NoteCount + attributes.HoldNoteCount) + p300Tail * attributes.HoldNoteCount) / totalHits;
                double p200 = (p200Note * (attributes.NoteCount + attributes.HoldNoteCount) + p200Tail * attributes.HoldNoteCount) / totalHits;
                double p100 = (p100Note * (attributes.NoteCount + attributes.HoldNoteCount) + p100Tail * attributes.HoldNoteCount) / totalHits;
                double p50 = (p50Note * (attributes.NoteCount + attributes.HoldNoteCount) + p50Tail * attributes.HoldNoteCount) / totalHits;
                double p0 = (p0Note * (attributes.NoteCount + attributes.HoldNoteCount) + p0Tail * attributes.HoldNoteCount) / totalHits;

                double gradient = Math.Pow(pMax, countPerfect / totalHits)
                                  * Math.Pow(p300, (countGreat + 0.5) / totalHits)
                                  * Math.Pow(p200, countGood / totalHits)
                                  * Math.Pow(p100, countOk / totalHits)
                                  * Math.Pow(p50, countMeh / totalHits)
                                  * Math.Pow(p0, countMiss / totalHits);

                return -gradient;
            }

            // Finding the minimum of the function returns the most likely deviation for the hit results.
            return isLegacyScore ? FindMinimum.OfScalarFunction(legacyLikelihoodGradient, 30) : FindMinimum.OfScalarFunction(lazerLikelihoodGradient, 30);
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

        private double hitProb(double x, double deviation)
        {
            return SpecialFunctions.Erf(x / (deviation * Math.Sqrt(2)));
        }

        private double hitProbLn(double x, double deviation)
        {
            return Math.Pow(2 * Normal.CDF(0, deviation * Math.Sqrt(2), x) - 1, 2);
        }
    }
}
