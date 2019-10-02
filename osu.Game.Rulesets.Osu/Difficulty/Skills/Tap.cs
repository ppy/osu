using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using osu.Game.Rulesets.Osu.Objects;
using System;
using System.Collections.Generic;
using System.Linq;


namespace osu.Game.Rulesets.Osu.Difficulty.Skills
{
    class Tap
    {

        private const int mashLevelCount = 11;
        private const double spacedBuffFactor = 0.10f;

        private static readonly Vector<double> decayCoeffs = Vector<double>.Build.Dense(Generate.LinearSpaced(4, 1.7, -0.7))
                                                                                 .PointwiseExp();


        public static (double, double, double[], double[], List<Vector<double>>) CalculateTapAttributes
            (List<OsuHitObject> hitObjects, double clockRate)
        {
            (var strainHistory, var maxTapStrain) = calculateTapStrain(hitObjects, 0, clockRate);
            double burstStrain = strainHistory.Max(v => v[0]);

            var streamnessMask = CalculateStreamnessMask(hitObjects, burstStrain, clockRate);
            double streamNoteCount = streamnessMask.Sum();

            (var mashLevels, var tapSkills) = calculateMashLevelsVSTapSkills(hitObjects, clockRate);

            return (maxTapStrain.Average(), streamNoteCount, mashLevels, tapSkills, strainHistory);
        }

        /// <summary>
        /// Calculates the strain values at each note and the maximum strain values
        /// </summary>
        private static (List<Vector<double>>, Vector<double>) calculateTapStrain(List<OsuHitObject> hitObjects,
                                                                                 double mashLevel,
                                                                                 double clockRate)
        {
            double prevPrevTime = hitObjects[0].StartTime / 1000.0;
            double prevTime = hitObjects[1].StartTime / 1000.0;
            var strainHistory = new List<Vector<double>> { decayCoeffs * 0, decayCoeffs * 0 };
            var currStrain = decayCoeffs * 1;

            for (int i = 2; i < hitObjects.Count; i++)
            {
                double currTime = hitObjects[i].StartTime / 1000.0;
                currStrain = currStrain.PointwiseMultiply((-decayCoeffs * (currTime - prevTime) / clockRate).PointwiseExp());
                strainHistory.Add(currStrain.PointwisePower(1.1 / 3) * 1.5);

                double relativeD = (hitObjects[i].Position - hitObjects[i - 1].Position).Length / (2 * hitObjects[i].Radius);
                double spacedBuff = calculateSpacedness(relativeD) * spacedBuffFactor;

                double deltaTime = (currTime - prevPrevTime) / clockRate;

                // for 1/4 notes above 200 bpm the exponent is -2.3, otherwise it's -2
                double currStrainBase = Math.Max(Math.Pow(deltaTime, -2.5) * 0.387, Math.Pow(deltaTime, -2));

                currStrain += decayCoeffs * currStrainBase *
                              Math.Pow(calculateMashNerfFactor(relativeD, mashLevel), 3) *
                              Math.Pow(1 + spacedBuff, 3);

                prevPrevTime = prevTime;
                prevTime = currTime;
            }

            var strainResult = decayCoeffs * 0;

            for (int j = 0; j < decayCoeffs.Count; j++)
            {
                double[] singleStrainHistory = new double[hitObjects.Count];

                for (int i = 0; i < hitObjects.Count; i++)
                {
                    singleStrainHistory[i] = strainHistory[i][j];
                }

                Array.Sort(singleStrainHistory);
                Array.Reverse(singleStrainHistory);

                double singleStrainResult = 0;
                double k = 0.95;

                for (int i = 0; i < hitObjects.Count; i++)
                {
                    singleStrainResult += singleStrainHistory[i] * Math.Pow(k, i);
                }

                strainResult[j] = singleStrainResult * (1 - k);
            }

            return (strainHistory, strainResult);
        }

        /// <summary>
        /// For every note, calculates the extent to which it is a part of a stream,
        /// and returns all results in an array.
        /// </summary>
        public static double[] CalculateStreamnessMask(List<OsuHitObject> hitObjects, double skill, double clockRate)
        {
            double[] streamnessMask = new double[hitObjects.Count];
            streamnessMask[0] = 0;

            double streamTimeThreshold = Math.Pow(skill, -2.7 / 3.2);

            for (int i = 1; i < hitObjects.Count; i++)
            {
                double t = (hitObjects[i].StartTime - hitObjects[i - 1].StartTime) / 1000 / clockRate;
                streamnessMask[i] = 1 - SpecialFunctions.Logistic((t / streamTimeThreshold - 1) * 15);
            }
            return streamnessMask;
        }

        private static (double[], double[]) calculateMashLevelsVSTapSkills(List<OsuHitObject> hitObjects, double clockRate)
        {
            double[] mashLevels = new double[mashLevelCount];
            double[] tapSkills = new double[mashLevelCount];

            for (int i = 0; i < mashLevelCount; i++)
            {
                double mashLevel = (double)i / (mashLevelCount - 1);
                mashLevels[i] = mashLevel;
                (var strainHistory, var maxTapStrain) = calculateTapStrain(hitObjects, mashLevel, clockRate);
                tapSkills[i] = maxTapStrain.Average();
            }
            return (mashLevels, tapSkills);
        }

        private static double calculateMashNerfFactor(double relativeD, double mashLevel)
        {
            double fullMashFactor = 0.73 + 0.27 * SpecialFunctions.Logistic(relativeD * 7 - 6);
            return mashLevel * fullMashFactor + (1 - mashLevel);
        }

        private static double calculateSpacedness(double d)
        {
            return SpecialFunctions.Logistic((d - 0.5) * 10) - SpecialFunctions.Logistic(-5);
        }
	        
    }
}
