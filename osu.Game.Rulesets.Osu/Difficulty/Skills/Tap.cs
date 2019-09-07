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

        private static readonly Vector<double> decayCoeffs = Vector<double>.Build.Dense(Generate.LinearSpaced(4, 1.7, -0.7))
                                                                                 .PointwiseExp();


        public static (double, double, double[], double[], List<Vector<double>>) CalculateTapAttributes
            (List<OsuHitObject> hitObjects, double clockRate)
        {
            (var strainHistory, var maxTapStrain) = calculateTapStrain(hitObjects, clockRate);
            double burstStrain = maxTapStrain[0];

            var streamnessMask = CalculateStreamnessMask(hitObjects, burstStrain, clockRate);
            double streamNoteCount = streamnessMask.Sum();

            (var mashLevels, var tapSkills) = calculateMashLevelsVSTapSkills(hitObjects, clockRate);

            return (maxTapStrain.Average(), streamNoteCount, mashLevels, tapSkills, strainHistory);
        }

        /// <summary>
        /// Calculates the strain values at each note and the maximum strain values
        /// </summary>
        private static (List<Vector<double>>, Vector<double>) calculateTapStrain(List<OsuHitObject> hitObjects, double clockRate)
        {
            double prevTime = hitObjects[0].StartTime / 1000.0;
            var currStrain = decayCoeffs * 1;
            var maxStrain = decayCoeffs * 1;
            var strainHistory = new List<Vector<double>> {currStrain};

            for (int i = 1; i < hitObjects.Count; i++)
            {
                double currTime = hitObjects[i].StartTime / 1000.0;
                currStrain = currStrain.PointwiseMultiply((-decayCoeffs * (currTime - prevTime) / clockRate).PointwiseExp());
                maxStrain = maxStrain.PointwiseMaximum(currStrain);
                strainHistory.Add(currStrain);

                double relativeD = (hitObjects[i].Position - hitObjects[i - 1].Position).Length / (2 * hitObjects[i].Radius);
                double spacedBuff = calculateSpacedness(relativeD) * 0.07;
                currStrain += decayCoeffs * (1 + spacedBuff);
                prevTime = currTime;
            }

            return (strainHistory, maxStrain);
        }

        /// <summary>
        /// For every note, calculates the extent to which it is a part of a stream,
        /// and returns all results in an array.
        /// </summary>
        public static double[] CalculateStreamnessMask(List<OsuHitObject> hitObjects, double skill, double clockRate)
        {
            double[] streamnessMask = new double[hitObjects.Count];
            streamnessMask[0] = 0;

            for (int i = 1; i < hitObjects.Count; i++)
            {
                double t = (hitObjects[i].StartTime - hitObjects[i - 1].StartTime) / 1000 / clockRate;
                streamnessMask[i] = 1 - SpecialFunctions.Logistic((t / (1 / skill) - 1.3) * 15);
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
                tapSkills[i] = calculateMashTapSkill(hitObjects, mashLevel, clockRate);
            }
            return (mashLevels, tapSkills);
        }

        private static double calculateMashTapSkill(List<OsuHitObject> hitObjects, double mashLevel, double clockRate)
        {
            double prevTime = hitObjects[0].StartTime / 1000.0;
            var currStrain = decayCoeffs * 1;
            var maxStrain = decayCoeffs * 1;

            for (int i = 1; i < hitObjects.Count; i++)
            {
                double currTime = hitObjects[i].StartTime / 1000.0;
                currStrain = currStrain.PointwiseMultiply((-decayCoeffs * (currTime - prevTime) / clockRate).PointwiseExp());
                maxStrain = maxStrain.PointwiseMaximum(currStrain);

                double relativeD = (hitObjects[i].Position - hitObjects[i - 1].Position).Length / (2 * hitObjects[i].Radius);
                double spacedBuff = calculateSpacedness(relativeD) * 0.07;
                currStrain += decayCoeffs * calculateMashNerfFactor(relativeD, mashLevel) * (1 + spacedBuff);
                prevTime = currTime;
            }
            return maxStrain.Average();
        }

        private static double calculateMashNerfFactor(double relativeD, double mashLevel)
        {
            double fullMashFactor = 0.8 + 0.2 * SpecialFunctions.Logistic(relativeD * 7 - 6);
            return mashLevel * fullMashFactor + (1 - mashLevel);
        }

        private static double calculateSpacedness(double d)
        {
            return SpecialFunctions.Logistic((d - 0.4) * 10) - SpecialFunctions.Logistic(-4);
        }
	        
    }
}
