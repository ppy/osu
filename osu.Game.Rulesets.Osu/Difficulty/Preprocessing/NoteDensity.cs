using System;
using System.Collections.Generic;
using System.Text;

using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Difficulty.Preprocessing
{
    class NoteDensity
    {

        public static List<double> CalculateNoteDensities(List<OsuHitObject> hitObjects, double preempt)
        {
            List<double> noteDensities = new List<double>();

            Queue<OsuHitObject> window = new Queue<OsuHitObject>();

            int next = 0;

            for (int i = 0; i < hitObjects.Count; i++)
            {
                while (next < hitObjects.Count && hitObjects[next].StartTime < hitObjects[i].StartTime + preempt)
                {
                    window.Enqueue(hitObjects[next]);
                    next++;
                }

                while (window.Peek().StartTime < hitObjects[i].StartTime - preempt)
                {
                    window.Dequeue();
                }

                noteDensities.Add(calculateNoteDensity(hitObjects[i].StartTime, preempt, window));
            }

            return noteDensities;
        }


        private static double calculateNoteDensity(double time, double preempt, Queue<OsuHitObject> window)
        {
            double noteDensity = 0;

            foreach (var hitObject in window)
            {
                noteDensity += 1 - Math.Abs(hitObject.StartTime - time) / preempt;
            }

            return noteDensity;
        }
    }
}
