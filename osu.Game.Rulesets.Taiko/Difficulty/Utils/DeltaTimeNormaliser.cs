// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Taiko.Difficulty.Preprocessing;

namespace osu.Game.Rulesets.Taiko.Difficulty.Utils
{
    /// <summary>
    /// Normalises deltaTime values for TaikoDifficultyHitObjects.
    /// </summary>
    public static class DeltaTimeNormaliser
    {
        /// <summary>
        /// Combines deltaTime values that differ by at most <paramref name="marginOfError"/>
        /// and replaces each value with the median of its range. This is used to reduce timing noise
        /// and improve rhythm grouping consistency, especially for maps with inconsistent or 'off-snapped' timing.
        /// </summary>
        public static Dictionary<TaikoDifficultyHitObject, double> Normalise(
            IReadOnlyList<TaikoDifficultyHitObject> hitObjects,
            double marginOfError)
        {
            var deltaTimes = hitObjects.Select(h => h.DeltaTime).Distinct().OrderBy(d => d).ToList();

            var sets = new List<List<double>>();
            List<double>? current = null;

            foreach (double value in deltaTimes)
            {
                // Add to the current group if within margin of error
                if (current != null && Math.Abs(value - current[0]) <= marginOfError)
                {
                    current.Add(value);
                    continue;
                }

                // Otherwise begin a new group
                current = new List<double> { value };
                sets.Add(current);
            }

            // Compute median for each group
            var medianLookup = new Dictionary<double, double>();

            foreach (var set in sets)
            {
                set.Sort();
                int mid = set.Count / 2;
                double median = set.Count % 2 == 1
                    ? set[mid]
                    : (set[mid - 1] + set[mid]) / 2;

                foreach (double v in set)
                    medianLookup[v] = median;
            }

            // Assign each hitobjects deltaTime the corresponding median value
            return hitObjects.ToDictionary(
                h => h,
                h => medianLookup.TryGetValue(h.DeltaTime, out double median) ? median : h.DeltaTime
            );
        }
    }
}
