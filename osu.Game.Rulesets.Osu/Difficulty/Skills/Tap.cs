// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using osu.Game.Rulesets.Osu.Difficulty.Utils;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills
{
    public static class Tap
    {
        private const double spaced_buff_factor = 0.10;
        private const int timescale_count = 4;

        /// <summary>
        /// Decay coefficient for each timescale.
        /// </summary>
        private static readonly Vector<double> decay_coeffs = Vector<double>.Build.Dense(Generate.LinearSpaced(timescale_count, 2.3, -2.8))
                                                                            .PointwiseExp();

        /// <summary>
        /// For each timescale, the strain result is multiplied by the corresponding factor in timescale_factors.
        /// </summary>
        private static readonly double[] timescale_factors = { 1.02, 1.02, 1.05, 1.15 };

        /// <summary>
        /// Calculates attributes related to tapping difficulty.
        /// </summary>
        public static TapAttributes CalculateTapAttributes(List<OsuHitObject> hitObjects, double clockRate)
        {
            var (strainHistory, tapDiff) = calculateTapStrain(hitObjects, 0, clockRate);
            double burstStrain = strainHistory.Max(v => v[0]);

            var streamnessMask = CalculateStreamnessMask(hitObjects, burstStrain, clockRate);
            double streamNoteCount = streamnessMask.Sum();

            var (_, mashTapDiff) = calculateTapStrain(hitObjects, 1, clockRate);

            return new TapAttributes
            {
                TapDifficulty = tapDiff,
                StreamNoteCount = streamNoteCount,
                MashedTapDifficulty = mashTapDiff,
                StrainHistory = strainHistory.Select(x=> PowerMean.Of(x, 2.0)).ToArray()
            };
        }

        /// <summary>
        /// Calculates the strain values at each note and the maximum strain values
        /// </summary>
        private static (List<Vector<double>>, double) calculateTapStrain(List<OsuHitObject> hitObjects,
                                                                         double mashLevel,
                                                                         double clockRate)
        {
            var strainHistory = new List<Vector<double>>
            {
                Vector<double>.Build.Dense(timescale_count),
                Vector<double>.Build.Dense(timescale_count)
            };
            var currStrain = Vector<double>.Build.Dense(timescale_count);

            // compute strain at each object and store the results into strainHistory
            if (hitObjects.Count >= 2)
            {
                double prevPrevTime = hitObjects[0].StartTime / 1000.0;
                double prevTime = hitObjects[1].StartTime / 1000.0;

                for (int i = 2; i < hitObjects.Count; i++)
                {
                    double currTime = hitObjects[i].StartTime / 1000.0;

                    // compute current strain after decay
                    currStrain = currStrain.PointwiseMultiply((-decay_coeffs * (currTime - prevTime) / clockRate).PointwiseExp());

                    strainHistory.Add(currStrain.PointwisePower(1.1 / 3) * 1.5);

                    double distance = (hitObjects[i].StackedPosition - hitObjects[i - 1].StackedPosition).Length / (2 * hitObjects[i].Radius);
                    double spacedBuff = calculateSpacedness(distance) * spaced_buff_factor;

                    double deltaTime = Math.Max((currTime - prevPrevTime) / clockRate, 0.01);

                    // for 1/4 notes above 200 bpm the exponent is -2.7, otherwise it's -2
                    double strainAddition = Math.Max(Math.Pow(deltaTime, -2.7) * 0.265, Math.Pow(deltaTime, -2));

                    currStrain += decay_coeffs * strainAddition *
                                  Math.Pow(calculateMashNerfFactor(distance, mashLevel), 3) *
                                  Math.Pow(1 + spacedBuff, 3);

                    prevPrevTime = prevTime;
                    prevTime = currTime;
                }
            }

            // compute difficulty by aggregating strainHistory
            var strainResult = Vector<double>.Build.Dense(timescale_count);

            for (int j = 0; j < decay_coeffs.Count; j++)
            {
                double[] singleStrainHistory = new double[hitObjects.Count];

                for (int i = 0; i < hitObjects.Count; i++)
                {
                    singleStrainHistory[i] = strainHistory[i][j];
                }

                Array.Sort(singleStrainHistory);
                Array.Reverse(singleStrainHistory);

                double singleStrainResult = 0;
                double k = 1 - 0.04 * Math.Sqrt(decay_coeffs[j]);

                for (int i = 0; i < hitObjects.Count; i++)
                {
                    singleStrainResult += singleStrainHistory[i] * Math.Pow(k, i);
                }

                strainResult[j] = singleStrainResult * (1 - k) * timescale_factors[j];
            }

            double diff = PowerMean.Of(strainResult, 2);

            return (strainHistory, diff);
        }

        /// <summary>
        /// For every note, calculates the extent to which it is a part of a stream,
        /// and returns all results in an array.
        /// </summary>
        public static double[] CalculateStreamnessMask(List<OsuHitObject> hitObjects, double skill, double clockRate)
        {
            double[] streamnessMask = new double[hitObjects.Count];

            if (hitObjects.Count > 1)
            {
                streamnessMask[0] = 0;
                double streamTimeThreshold = Math.Pow(skill, -2.7 / 3.2);

                for (int i = 1; i < hitObjects.Count; i++)
                {
                    double t = (hitObjects[i].StartTime - hitObjects[i - 1].StartTime) / 1000 / clockRate;
                    streamnessMask[i] = 1 - SpecialFunctions.Logistic((t / streamTimeThreshold - 1) * 15);
                }
            }

            return streamnessMask;
        }

        private static double calculateMashNerfFactor(double relativeD, double mashLevel)
        {
            double fullMashFactor = 0.73 + 0.27 * SpecialFunctions.Logistic(relativeD * 7 - 6);
            return mashLevel * fullMashFactor + (1 - mashLevel);
        }

        private static double calculateSpacedness(double d)
        {
            return SpecialFunctions.Logistic((d - 0.533) / 0.13) - SpecialFunctions.Logistic(-4.1);
        }
    }
}
