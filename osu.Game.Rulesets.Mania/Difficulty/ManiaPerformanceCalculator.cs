// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using MathNet.Numerics;
using osu.Framework.Audio.Track;
using osu.Framework.Extensions.IEnumerableExtensions;

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
                // Total = totalValue,
                Total = estimatedUR,
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
                return Double.PositiveInfinity;

            double[] judgements = new double[5];

            // Legacy scores have the same number of judgements and total notes 
            bool isLegacyScore = totalHits == attributes.NoteCount + attributes.HoldNoteCount;

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

            // The deviation estimation cannot handle SS scores, so always assume there is one extra note to keep the perfect hit probability below 100%.
            double totalHitsP1 = totalHits + 1;

            double legacyLikelihoodGradient(double d)
            {
                if (d <= 0)
                    return double.PositiveInfinity;

                double pMax = 1 - erfcApprox(hMax / (d * root2));
                double p300 = erfcApprox(hMax / (d * root2)) - erfcApprox(h300 / (d * root2));
                double p200 = erfcApprox(h300 / (d * root2)) - erfcApprox(h200 / (d * root2));
                double p100 = erfcApprox(h200 / (d * root2)) - erfcApprox(h100 / (d * root2));
                double p50 = erfcApprox(h100 / (d * root2)) - erfcApprox(h50 / (d * root2));
                double p0 = erfcApprox(h50 / (d * root2));

                double gradient = Math.Pow(pMax, countPerfect / totalHitsP1)
                * Math.Pow(p300, countGreat / totalHitsP1)
                * Math.Pow(p200, countGood / totalHitsP1)
                * Math.Pow(p100, countOk / totalHitsP1)
                * Math.Pow(p50, countMeh / totalHitsP1)
                * Math.Pow(p0, countMiss / totalHitsP1);

                return -gradient;
            }

            double lazerLikelihoodGradient(double d)
            {
                if (d <= 0)
                    return double.PositiveInfinity;

                double pMaxNote = 1 - erfcApprox(hMax / (d * root2));
                double p300Note = erfcApprox(hMax / (d * root2)) - erfcApprox(h300 / (d * root2));
                double p200Note = erfcApprox(h300 / (d * root2)) - erfcApprox(h200 / (d * root2));
                double p100Note = erfcApprox(h200 / (d * root2)) - erfcApprox(h100 / (d * root2));
                double p50Note = erfcApprox(h100 / (d * root2)) - erfcApprox(h50 / (d * root2));
                double p0Note = erfcApprox(h50 / (d * root2));

                double pMaxTail = 1 - erfcApprox((hMax * 1.5) / (d * root2));
                double p300Tail = erfcApprox((hMax * 1.5) / (d * root2)) - erfcApprox((h300 * 1.5) / (d * root2));
                double p200Tail = erfcApprox((h300 * 1.5) / (d * root2)) - erfcApprox((h200 * 1.5) / (d * root2));
                double p100Tail = erfcApprox((h200 * 1.5) / (d * root2)) - erfcApprox((h100 * 1.5) / (d * root2));
                double p50Tail = erfcApprox((h100 * 1.5) / (d * root2)) - erfcApprox((h50 * 1.5) / (d * root2));
                double p0Tail = erfcApprox((h50 * 1.5) / (d * root2));

                double pMax = ((pMaxNote * (attributes.NoteCount + attributes.HoldNoteCount)) + (pMaxTail * attributes.HoldNoteCount)) / totalHits;
                double p300 = ((p300Note * (attributes.NoteCount + attributes.HoldNoteCount)) + (p300Tail * attributes.HoldNoteCount)) / totalHits;
                double p200 = ((p200Note * (attributes.NoteCount + attributes.HoldNoteCount)) + (p200Tail * attributes.HoldNoteCount)) / totalHits;
                double p100 = ((p100Note * (attributes.NoteCount + attributes.HoldNoteCount)) + (p100Tail * attributes.HoldNoteCount)) / totalHits;
                double p50 = ((p50Note * (attributes.NoteCount + attributes.HoldNoteCount)) + (p50Tail * attributes.HoldNoteCount)) / totalHits;
                double p0 = ((p0Note * (attributes.NoteCount + attributes.HoldNoteCount)) + (p0Tail * attributes.HoldNoteCount)) / totalHits;

                double gradient = Math.Pow(pMax, countPerfect / totalHitsP1)
                * Math.Pow(p300, countGreat / totalHitsP1)
                * Math.Pow(p200, countGood / totalHitsP1)
                * Math.Pow(p100, countOk / totalHitsP1)
                * Math.Pow(p50, countMeh / totalHitsP1)
                * Math.Pow(p0, countMiss / totalHitsP1);

                return -gradient;
            }

            // Finding the minimum of the inverse likelihood function returns the most likely deviation for a play
            if  (isLegacyScore)
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

            double overallDifficulty = attributes.OverallDifficulty;

            if (attributes.Convert)
                overallDifficulty = 10;

            double windowMultiplier = 1 / clockRate;

            if (score.Mods.Any(m => m is ModHardRock))
                windowMultiplier *= 1 / 1.4;
            else if (score.Mods.Any(m => m is ModEasy))
                windowMultiplier *= 1.4;

            if (overallDifficulty < 5)
                judgements[0] = (22.4 - 0.6 * overallDifficulty) * windowMultiplier;
            else
                judgements[0] = (24.9 - 1.1 * overallDifficulty) * windowMultiplier;
            judgements[1] = (64 - 3 * overallDifficulty) * windowMultiplier;
            judgements[2] = (97 - 3 * overallDifficulty) * windowMultiplier;
            judgements[3] = (127 - 3 * overallDifficulty) * windowMultiplier;
            judgements[4] = (151 - 3 * overallDifficulty) * windowMultiplier;

            return judgements;
        }
 
        double erfcApprox(double x)
        {
            if (x <= 5)
                return SpecialFunctions.Erfc(x);

            // This approximation is very accurate with values over 5, and is much more performant than the Erfc function
            return Math.Pow(Math.E, -Math.Pow(x, 2) - Math.Log(x * Math.Sqrt(Math.PI)));
        } 
    }
}
