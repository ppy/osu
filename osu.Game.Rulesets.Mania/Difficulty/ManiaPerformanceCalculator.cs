// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics;
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
        private double estimatedUR;

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
            estimatedUR = computeEstimatedUR(score, maniaAttributes) * 10;

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
                EstimatedUR = estimatedUR
            };
        }

        private double computeDifficultyValue(ManiaDifficultyAttributes attributes)
        {
            double difficultyValue = Math.Pow(Math.Max(attributes.StarRating - 0.15, 0.05), 2.2); // Star rating to pp curve

            difficultyValue *= Math.Max(1.2 * Math.Pow(SpecialFunctions.Erf(300 / estimatedUR), 1.6) - 0.2, 0);

            return difficultyValue;
        }

        private double totalHits => countPerfect + countOk + countGreat + countGood + countMeh + countMiss;
        private double totalSuccessfulHits => countPerfect + countOk + countGreat + countGood + countMeh;

        /// <summary>
        /// Accuracy used to weight judgements independently from the score's actual accuracy.
        /// </summary>
        private double computeEstimatedUR(ScoreInfo score, ManiaDifficultyAttributes attributes)
        {
            if (totalSuccessfulHits == 0)
                return double.PositiveInfinity;

            double[] judgements = new double[5];

            bool isLegacyScore = false;

            // Temporary workaround for lazer not having classic mania behaviour implemented. 
            // Classic scores with only Notes will return incorrect values after the replay is watched.
            if (score.Mods.Any(m => m is ModClassic) && totalHits == attributes.NoteCount + attributes.HoldNoteCount)
                isLegacyScore = true;

            if (isLegacyScore)
                judgements = getLegacyJudgements(score, attributes);
            else
                judgements = getLazerJudgements(score, attributes);

            double hMax = judgements[0];
            double h300 = judgements[1];
            double h200 = judgements[2];
            double h100 = judgements[3];
            double h50 = judgements[4];

            double root2 = Math.Sqrt(2);

            double legacyLikelihoodGradient(double d)
            {
                if (d <= 0)
                    return double.PositiveInfinity;

                double pMaxNote = hitProb(hMax, d);
                double p300Note = hitProb(h300, d) - hitProb(hMax, d);
                double p200Note = hitProb(h200, d) - hitProb(h300, d);
                double p100Note = hitProb(h100, d) - hitProb(h200, d);
                double p50Note = hitProb(h50, d) - hitProb(h100, d);
                double p0Note = 1 - hitProb(h50, d);

                // Effective hit window for LN tails. Should be a value between 1 and 2. This is because the hit window for LN tails in stable
                // arent static, and depend on how far from 0ms offset the hit on the head was. A lower value results in a lower estimated deviation.
                double tailMultipler = 1.5;

                // Since long notes only give a specific judgement if both both hits end up within a certain hit window,
                // multiply the probability of hitting in the head hit window by the probability of hitting in the tail hit window.
                double pMaxLN = hitProb(hMax * 1.2, d) * hitProb(hMax * 1.2 * tailMultipler, d);

                double p300LN = hitProb(h300 * 1.1, d) * hitProb(h300 * 1.1 * tailMultipler, d)
                              - hitProb(hMax * 1.2, d) * hitProb(hMax * 1.2 * tailMultipler, d);

                double p200LN = hitProb(h200, d) * hitProb(h200 * tailMultipler, d)
                              - hitProb(h300 * 1.1, d) * hitProb(h300 * 1.1 * tailMultipler, d);

                double p100LN = hitProb(h100, d) * hitProb(h100 * tailMultipler, d)
                              - hitProb(h200, d) * hitProb(h200 * tailMultipler, d);

                double p50LN = hitProb(h50, d) * hitProb(h50 * tailMultipler, d)
                             - hitProb(h100, d) * hitProb(h100 * tailMultipler, d);

                double p0LN = 1 - hitProb(h50, d) * hitProb(h50 * tailMultipler, d);

                double pMax = ((pMaxNote * attributes.NoteCount) + (pMaxLN * attributes.HoldNoteCount)) / totalHits;
                double p300 = ((p300Note * attributes.NoteCount) + (p300LN * attributes.HoldNoteCount)) / totalHits;
                double p200 = ((p200Note * attributes.NoteCount) + (p200LN * attributes.HoldNoteCount)) / totalHits;
                double p100 = ((p100Note * attributes.NoteCount) + (p100LN * attributes.HoldNoteCount)) / totalHits;
                double p50 = ((p50Note * attributes.NoteCount) + (p50LN * attributes.HoldNoteCount)) / totalHits;
                double p0 = ((p0Note * attributes.NoteCount) + (p0LN * attributes.HoldNoteCount)) / totalHits;

                double gradient = Math.Pow(pMax, countPerfect / totalHits)
                * Math.Pow(p300, (countGreat + 0.5) / totalHits)
                * Math.Pow(p200, countGood / totalHits)
                * Math.Pow(p100, countOk / totalHits)
                * Math.Pow(p50, countMeh / totalHits)
                * Math.Pow(p0, countMiss / totalHits);

                return -gradient;
            }

            double lazerLikelihoodGradient(double d)
            {
                if (d <= 0)
                    return double.PositiveInfinity;

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

                double pMax = ((pMaxNote * (attributes.NoteCount + attributes.HoldNoteCount)) + (pMaxTail * attributes.HoldNoteCount)) / totalHits;
                double p300 = ((p300Note * (attributes.NoteCount + attributes.HoldNoteCount)) + (p300Tail * attributes.HoldNoteCount)) / totalHits;
                double p200 = ((p200Note * (attributes.NoteCount + attributes.HoldNoteCount)) + (p200Tail * attributes.HoldNoteCount)) / totalHits;
                double p100 = ((p100Note * (attributes.NoteCount + attributes.HoldNoteCount)) + (p100Tail * attributes.HoldNoteCount)) / totalHits;
                double p50 = ((p50Note * (attributes.NoteCount + attributes.HoldNoteCount)) + (p50Tail * attributes.HoldNoteCount)) / totalHits;
                double p0 = ((p0Note * (attributes.NoteCount + attributes.HoldNoteCount)) + (p0Tail * attributes.HoldNoteCount)) / totalHits;

                double gradient = Math.Pow(pMax, countPerfect / totalHits)
                * Math.Pow(p300, (countGreat + 0.5) / totalHits)
                * Math.Pow(p200, countGood / totalHits)
                * Math.Pow(p100, countOk / totalHits)
                * Math.Pow(p50, countMeh / totalHits)
                * Math.Pow(p0, countMiss / totalHits);

                return -gradient;
            }

            // Finding the minimum of the function returns the most likely deviation for the hit results.
            if (isLegacyScore)
                return FindMinimum.OfScalarFunction(legacyLikelihoodGradient, 30);
            else
                return FindMinimum.OfScalarFunction(lazerLikelihoodGradient, 30);
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
            return erfApprox(x / (deviation * Math.Sqrt(2)));
        }

        private double erfApprox(double x)
        {
            if (x <= 5)
                return SpecialFunctions.Erf(x);

            // This approximation is very accurate with values over 5, and is much more performant than the Erf function
            return 1 - Math.Exp(-Math.Pow(x, 2) - Math.Log(x * Math.Sqrt(Math.PI)));
        }
    }
}
