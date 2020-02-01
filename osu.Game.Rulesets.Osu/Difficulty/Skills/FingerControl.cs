using osu.Game.Rulesets.Osu.Objects;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;


namespace osu.Game.Rulesets.Osu.Difficulty.Skills
{
    class FingerControl
    {

        public static double CalculateFingerControlDiff(List<OsuHitObject> hitObjects, double clockRate)
        {
            if (hitObjects.Count == 0)
            {
                return 0;
            }

            double prevTime = hitObjects[0].StartTime / 1000.0;
            double currStrain = 0;
            double prevStrainTime = 0;
            int repeatStrainCount = 1;
            var strainHistory = new List<double> { 0 };

            for (int i = 1; i < hitObjects.Count; i++)
            {
                double currTime = hitObjects[i].StartTime / 1000.0;
                double deltaTime = (currTime - prevTime) / clockRate;

                double strainTime = Math.Max(deltaTime, 0.046875);
                double strainDecayBase = Math.Pow(0.9, 1 / Math.Min(strainTime, 0.2));

                currStrain *= Math.Pow(strainDecayBase, deltaTime);

                strainHistory.Add(currStrain);

                double strain = 0.1 / strainTime;

                if (Math.Abs(strainTime - prevStrainTime) > 0.004)
                    repeatStrainCount = 1;
                else
                    repeatStrainCount++;

                if (hitObjects[i] is Slider)
                    strain /= 2.0;

                if (repeatStrainCount % 2 == 0)
                    strain = 0;
                else
                    strain /= Math.Pow(1.25, repeatStrainCount);

                currStrain += strain;

                prevTime = currTime;
                prevStrainTime = strainTime;
            }

            var strainHistoryArray = strainHistory.ToArray();

            Array.Sort(strainHistoryArray);
            Array.Reverse(strainHistoryArray);

            double diff = 0;
            double k = 0.95;

            for (int i = 0; i < hitObjects.Count; i++)
            {
                diff += strainHistoryArray[i] * Math.Pow(k, i);
            }

            return diff * (1 - k) * 1.1;
        }
    }
}
