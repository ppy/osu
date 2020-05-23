// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;

using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.RootFinding;

using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Difficulty.MathUtil;
using System.Linq;
using System.IO;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills
{
    /// <summary>
    /// Represents the skill required to correctly aim at every object in the map with a uniform CircleSize and normalized distances.
    /// </summary>
    public class Aim : Skill
    {
        private const double probability_threshold = 0.02;
        private const double time_threshold_base = 1200;
        private const double tp_min = 0.1;
        private const double tp_max = 100;
        private const double prob_precision = 1e-4;
        private const double time_precision = 5e-4;
        private const int max_iterations = 100;

        private const double default_cheese_level = 0.4;
        private const int cheese_level_count = 11;

        private const int miss_tp_count = 20;
        private const int combo_tp_count = 50;


        public static (double, double, double[], double[], double[], double, double[], double[], string)
            CalculateAimAttributes(List<OsuHitObject> hitObjects,
                                   double clockRate,
                                   List<Vector<double>> strainHistory,
                                   List<double> noteDensities)
        {
            List<OsuMovement> movements = createMovements(hitObjects, clockRate, strainHistory);
            List<OsuMovement> movementsHidden = createMovements(hitObjects, clockRate, strainHistory,
                                                                hidden: true, noteDensities: noteDensities);

            var mapHitProbs = new HitProbabilities(movements, default_cheese_level, difficultyCount: combo_tp_count);
            double fcProbTP = calculateFCProbTP(movements);
            double fcProbTPHidden = calculateFCProbTP(movementsHidden);

            double hiddenFactor = fcProbTPHidden / fcProbTP;

            string graphText = generateGraphText(movements, fcProbTP);

            double[] comboTPs = calculateComboTps(mapHitProbs);
            double fcTimeTP = comboTPs.Last();
            (var missTPs, var missCounts) = calculateMissTPsMissCounts(movements, fcTimeTP);
            (var cheeseLevels, var cheeseFactors) = calculateCheeseLevelsVSCheeseFactors(movements, fcProbTP);
            double cheeseNoteCount = getCheeseNoteCount(movements, fcProbTP);

            return (fcProbTP, hiddenFactor, comboTPs, missTPs, missCounts, cheeseNoteCount, cheeseLevels, cheeseFactors, graphText);
        }

        private static List<OsuMovement> createMovements(List<OsuHitObject> hitObjects, double clockRate, List<Vector<double>> strainHistory,
                                                         bool hidden = false, List<double> noteDensities = null)
        {
            OsuMovement.Initialize();
            var movements = new List<OsuMovement>();

            if (hitObjects.Count == 0)
                return movements;

            // the first object
            movements.AddRange(OsuMovement.ExtractMovement(hitObjects[0]));

            // the rest
            for (int i = 1; i < hitObjects.Count; i++)
            {
                var objMinus2 = i > 3 ? hitObjects[i - 4] : null;
                var obj0 = i > 1 ? hitObjects[i - 2] : null;
                var obj1 = hitObjects[i - 1];
                var obj2 = hitObjects[i];
                var obj3 = i < hitObjects.Count - 1 ? hitObjects[i + 1] : null;
                var tapStrain = strainHistory[i];

                if (hidden)
                    movements.AddRange(OsuMovement.ExtractMovement(obj0, obj1, obj2, obj3, tapStrain, clockRate,
                                                                   hidden: true, noteDensity: noteDensities[i], objMinus2: objMinus2));
                else
                    movements.AddRange(OsuMovement.ExtractMovement(obj0, obj1, obj2, obj3, tapStrain, clockRate, objMinus2: objMinus2));

                
            }
            return movements;
        }

        private static double calculateFCProbTP(IEnumerable<OsuMovement> movements, double cheeseLevel = default_cheese_level)
        {
            double fcProbabilityTPMin = calculateFCProb(movements, tp_min, cheeseLevel);

            if (fcProbabilityTPMin >= probability_threshold)
                return tp_min;

            double fcProbabilityTPMax = calculateFCProb(movements, tp_max, cheeseLevel);

            if (fcProbabilityTPMax <= probability_threshold)
                return tp_max;

            double fcProbMinusThreshold(double tp) => calculateFCProb(movements, tp, cheeseLevel) - probability_threshold;
            return Brent.FindRoot(fcProbMinusThreshold, tp_min, tp_max, prob_precision, max_iterations);
        }

        /// <summary>
        /// Calculates the throughput at which the expected time to FC the given movements =
        /// timeThresholdBase + time span of the movements
        /// </summary>
        private static double calculateFCTimeTP(HitProbabilities mapHitProbs, int sectionCount)
        {
            if (mapHitProbs.IsEmpty(sectionCount))
                return 0;

            double maxFCTime = mapHitProbs.MinExpectedTimeForCount(tp_min, sectionCount);

            if (maxFCTime <= time_threshold_base)
                return tp_min;

            double minFCTime = mapHitProbs.MinExpectedTimeForCount(tp_max, sectionCount);

            if (minFCTime >= time_threshold_base)
                return tp_max;

            double fcTimeMinusThreshold(double tp) => mapHitProbs.MinExpectedTimeForCount(tp, sectionCount) - time_threshold_base;
            return Bisection.FindRoot(fcTimeMinusThreshold, tp_min, tp_max, time_threshold_base * time_precision, max_iterations);
        }

        private static string generateGraphText(List<OsuMovement> movements, double tp)
        {
            var sw = new StringWriter();

            foreach (var movement in movements)
            {
                double time = movement.Time;
                double ipRaw = movement.IP12;
                double ipCorrected = FittsLaw.CalculateIP(movement.D, movement.MT * (1 + default_cheese_level * movement.CheesableRatio));
                double missProb = 1 - HitProbabilities.CalculateCheeseHitProb(movement, tp, default_cheese_level);

                sw.WriteLine($"{time} {ipRaw} {ipCorrected} {missProb}");
            }

            string graphText = sw.ToString();
            sw.Dispose();
            return graphText;
        }



        /// <summary>
        /// Calculate miss count for a list of throughputs (used to evaluate miss count of plays).
        /// </summary>
        private static (double[], double[]) calculateMissTPsMissCounts(IList<OsuMovement> movements, double fcTimeTP)
        {
            double[] missTPs = new double[miss_tp_count];
            double[] missCounts = new double[miss_tp_count];
            double fcProb = calculateFCProb(movements, fcTimeTP, default_cheese_level);

            for (int i = 0; i < miss_tp_count; i++)
            {
                double missTP = fcTimeTP * (1 - Math.Pow(i, 1.5) * 0.005);
                double[] missProbs = getMissProbs(movements, missTP);
                missTPs[i] = missTP;
                missCounts[i] = getMissCount(fcProb, missProbs);
            }
            return (missTPs, missCounts);
        }


        /// <summary>
        /// Calculate the probability of missing each note given a skill level.
        /// </summary>
        private static double[] getMissProbs(IList<OsuMovement> movements, double tp)
        {
            // slider breaks should be a miss :( -- joz, 2019
            var missProbs = new double[movements.Count];

            for (int i = 0; i < movements.Count; ++i)
            {
                var movement = movements[i];
                missProbs[i] = 1 - HitProbabilities.CalculateCheeseHitProb(movement, tp, default_cheese_level);
            }

            return missProbs;
        }

        /// <summary>
        /// Find first miss count achievable with at least probability p
        /// </summary>
        private static double getMissCount(double p, double[] missProbabilities)
        {
            if (missProbabilities.Sum() == 0)
                return 0;

            var distribution = new PoissonBinomial(missProbabilities);

            double cdfMinusProb(double missCount) => distribution.Cdf(missCount) - p;
            return Brent.FindRootExpand(cdfMinusProb, -100, 1000);
        }

        private static (double[], double[]) calculateCheeseLevelsVSCheeseFactors(IList<OsuMovement> movements, double fcProbTP)
        {
            double[] cheeseLevels = new double[cheese_level_count];
            double[] cheeseFactors = new double[cheese_level_count];

            for (int i = 0; i < cheese_level_count; i++)
            {
                double cheeseLevel = (double)i / (cheese_level_count - 1);
                cheeseLevels[i] = cheeseLevel;
                cheeseFactors[i] = calculateFCProbTP(movements, cheeseLevel) / fcProbTP;
            }
            return (cheeseLevels, cheeseFactors);
        }

        private static double getCheeseNoteCount(IList<OsuMovement> movements, double tp)
        {
            double count = 0;
            foreach (var movement in movements)
            {
                double cheeseness = SpecialFunctions.Logistic((movement.IP12 / tp - 0.6) * 15) * movement.Cheesablility;
                count += cheeseness;
            }

            return count;
        }

        private static double[] calculateComboTps(HitProbabilities hitProbabilities)
        {
            double[] ComboTPs = new double[combo_tp_count];

            for (int i = 1; i <= combo_tp_count; ++i)
            {
                ComboTPs[i - 1] = calculateFCTimeTP(hitProbabilities, i);
            }

            return ComboTPs;
        }

        private static double calculateFCProb(IEnumerable<OsuMovement> movements, double tp, double cheeseLevel)
        {
            double fcProb = 1;

            foreach (OsuMovement movement in movements)
            {
                double hitProb = HitProbabilities.CalculateCheeseHitProb(movement, tp, cheeseLevel);
                fcProb *= hitProb;
            }
            return fcProb;
        }


        protected override double SkillMultiplier => 0;
        protected override double StrainDecayBase => 0;
        protected override double StrainValueOf(DifficultyHitObject current)
        {
            throw new NotImplementedException();
        }
    }
}
