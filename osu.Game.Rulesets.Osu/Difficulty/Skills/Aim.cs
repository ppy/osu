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
        private const double probabilityThreshold = 0.02;
        private const double timeThresholdBase = 1200;
        private const double tpMin = 0.1;
        private const double tpMax = 100;
        private const double probPrecision = 1e-4;
        private const double timePrecision = 5e-4;
        private const int maxIterations = 100;

        private const double defaultCheeseLevel = 0.4;
        private const int cheeseLevelCount = 11;

        private const int difficultyCount = 20;


        public static (double, double, double[], double[], double[], double, double[], double[], string)
            CalculateAimAttributes(List<OsuHitObject> hitObjects,
                                   double clockRate,
                                   List<Vector<double>> strainHistory,
                                   List<double> noteDensities)
        {
            List<OsuMovement> movements = createMovements(hitObjects, clockRate, strainHistory);
            List<OsuMovement> movementsHidden = createMovements(hitObjects, clockRate, strainHistory,
                                                                hidden: true, noteDensities: noteDensities);

            var mapHitProbs = new HitProbabilities(movements, defaultCheeseLevel);
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
                var obj0 = i > 1 ? hitObjects[i - 2] : null;
                var obj1 = hitObjects[i - 1];
                var obj2 = hitObjects[i];
                var obj3 = i < hitObjects.Count - 1 ? hitObjects[i + 1] : null;
                var tapStrain = strainHistory[i];

                if (hidden)
                    movements.AddRange(OsuMovement.ExtractMovement(obj0, obj1, obj2, obj3, tapStrain, clockRate,
                                                                   hidden: true, noteDensity: noteDensities[i]));
                else
                    movements.AddRange(OsuMovement.ExtractMovement(obj0, obj1, obj2, obj3, tapStrain, clockRate));

                
            }
            return movements;
        }

        private static double calculateFCProbTP(IEnumerable<OsuMovement> movements, double cheeseLevel = defaultCheeseLevel)
        {
            double fcProbabilityTPMin = calculateFCProb(movements, tpMin, cheeseLevel);

            if (fcProbabilityTPMin >= probabilityThreshold)
                return tpMin;

            double fcProbabilityTPMax = calculateFCProb(movements, tpMax, cheeseLevel);

            if (fcProbabilityTPMax <= probabilityThreshold)
                return tpMax;

            double fcProbMinusThreshold(double tp) => calculateFCProb(movements, tp, cheeseLevel) - probabilityThreshold;
            return Brent.FindRoot(fcProbMinusThreshold, tpMin, tpMax, probPrecision, maxIterations);
        }

        /// <summary>
        /// Calculates the throughput at which the expected time to FC the given movements =
        /// timeThresholdBase + time span of the movements
        /// </summary>
        private static double calculateFCTimeTP(HitProbabilities mapHitProbs, int sectionCount)
        {
            if (mapHitProbs.IsEmpty(sectionCount))
                return 0;

            double maxFCTime = mapHitProbs.MinExpectedTimeForCount(tpMin, sectionCount);

            if (maxFCTime <= timeThresholdBase)
                return tpMin;

            double minFCTime = mapHitProbs.MinExpectedTimeForCount(tpMax, sectionCount);

            if (minFCTime >= timeThresholdBase)
                return tpMax;

            double fcTimeMinusThreshold(double tp) => mapHitProbs.MinExpectedTimeForCount(tp, sectionCount) - timeThresholdBase;
            return Bisection.FindRoot(fcTimeMinusThreshold, tpMin, tpMax, timeThresholdBase * timePrecision, maxIterations);
        }

        private static string generateGraphText(List<OsuMovement> movements, double tp)
        {
            var sw = new StringWriter();

            foreach (var movement in movements)
            {
                double time = movement.Time;
                double ipRaw = movement.IP12;
                double ipCorrected = FittsLaw.CalculateIP(movement.D, movement.MT * (1 + defaultCheeseLevel * movement.CheesableRatio));
                double missProb = 1 - HitProbabilities.CalculateCheeseHitProb(movement, tp, defaultCheeseLevel);

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
            double[] missTPs = new double[difficultyCount];
            double[] missCounts = new double[difficultyCount];
            double fcProb = calculateFCProb(movements, fcTimeTP, defaultCheeseLevel);

            for (int i = 0; i < difficultyCount; i++)
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
                missProbs[i] = 1 - HitProbabilities.CalculateCheeseHitProb(movement, tp, defaultCheeseLevel);
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

            Func<double, double> cdfMinusProb = missCount => distribution.Cdf(missCount) - p;
            return Brent.FindRootExpand(cdfMinusProb, -100, 1000);
        }

        private static (double[], double[]) calculateCheeseLevelsVSCheeseFactors(IList<OsuMovement> movements, double fcProbTP)
        {
            double[] cheeseLevels = new double[cheeseLevelCount];
            double[] cheeseFactors = new double[cheeseLevelCount];

            for (int i = 0; i < cheeseLevelCount; i++)
            {
                double cheeseLevel = (double)i / (cheeseLevelCount - 1);
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
            double[] ComboTPs = new double[difficultyCount];

            for (int i = 1; i <= difficultyCount; ++i)
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


        protected override double SkillMultiplier => throw new NotImplementedException();
        protected override double StrainDecayBase => throw new NotImplementedException();
        protected override double StrainValueOf(DifficultyHitObject current)
        {
            throw new NotImplementedException();
        }
    }
}
