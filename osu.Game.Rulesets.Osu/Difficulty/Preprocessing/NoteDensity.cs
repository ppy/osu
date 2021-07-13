// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Difficulty.Preprocessing
{
    public static class NoteDensity
    {
        /// <summary>
        /// Calculates the note density for every note in the map.
        /// </summary>
        /// <param name="hitObjects">The list of hitobjects in the map</param>
        /// <param name="preempt">The preemption window to use when calculating density of neighbouring notes.</param>
        public static double[] Calculate(List<OsuHitObject> hitObjects, double preempt)
        {
            List<double> noteDensities = new List<double>();

            Queue<OsuHitObject> window = new Queue<OsuHitObject>();

            // TODO: figure out why next is not reset
            int next = 0;

            for (int i = 0; i < hitObjects.Count; i++)
            {
                while (next < hitObjects.Count && hitObjects[next].StartTime < hitObjects[i].StartTime + preempt)
                {
                    window.Enqueue(hitObjects[next]);
                    next++;
                }

                while (window.Peek().StartTime < hitObjects[i].StartTime - preempt)
                    window.Dequeue();

                noteDensities.Add(calculateNoteDensity(hitObjects[i].StartTime, preempt, window));
            }

            return noteDensities.ToArray();
        }

        private static double calculateNoteDensity(double time, double preempt, Queue<OsuHitObject> window)
        {
            double noteDensity = 0;

            foreach (var hitObject in window)
                noteDensity += 1 - Math.Abs(hitObject.StartTime - time) / preempt;

            return noteDensity;
        }
    }
}
