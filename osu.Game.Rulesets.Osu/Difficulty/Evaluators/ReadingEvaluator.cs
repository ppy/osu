// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Difficulty.Evaluators
{
    // Main class with some util functions
    public static class ReadingEvaluator
    {
        private const double reading_window_size = 3000;

        private const double overlap_multiplier = 1;

        public static double EvaluateDensityOf(DifficultyHitObject current, bool applyDistanceNerf = true)
        {
            var currObj = (OsuDifficultyHitObject)current;

            double density = 0;
            double densityAnglesNerf = -2; // we have threshold of 2

            // Despite being called prev, it's actually more late in time
            OsuDifficultyHitObject prevObj0 = currObj;

            var readingObjects = currObj.ReadingObjects;
            for (int i = 0; i < readingObjects.Count; i++)
            {
                var loopObj = readingObjects[i].HitObject;

                if (loopObj.Index < 1)
                    continue; // Don't look on the first object of the map

                double loopDifficulty = currObj.OpacityAt(loopObj.BaseObject.StartTime, false);

                // Small distances means objects may be cheesed, so it doesn't matter whether they are arranged confusingly.
                if (applyDistanceNerf) loopDifficulty *= (logistic((loopObj.MinimumJumpDistance - 80) / 10) + 0.2) / 1.2;

                // Reduce density bonus for this object if they're too apart in time
                // Nerf starts on 1500ms and reaches maximum (*=0) on 3000ms
                double timeBetweenCurrAndLoopObj = currObj.StartTime - loopObj.StartTime;
                loopDifficulty *= getTimeNerfFactor(timeBetweenCurrAndLoopObj);

                // Only if next object is slower, representing break from many notes in a row
                if (loopObj.StrainTime > prevObj0.StrainTime)
                {
                    // Get rhythm similarity: 1 on same rhythms, 0.5 on 1/4 to 1/2
                    double rhythmSimilarity = 1 - getRhythmDifference(loopObj.StrainTime, prevObj0.StrainTime);

                    // Make differentiation going from 1/4 to 1/2 and bigger difference
                    // To 1/3 to 1/2 and smaller difference
                    rhythmSimilarity = Math.Clamp(rhythmSimilarity, 0.5, 0.75);
                    rhythmSimilarity = 4 * (rhythmSimilarity - 0.5);

                    // Reduce density for this objects if rhythms are different
                    loopDifficulty *= rhythmSimilarity;
                }

                density += loopDifficulty;

                // Angles nerf
                double currAngleNerf = (loopObj.AnglePredictability / 2) + 0.5;

                // Apply the nerf only when it's repeated
                double angleNerf = currAngleNerf;

                densityAnglesNerf += angleNerf * loopDifficulty;

                prevObj0 = loopObj;
            }

            // Apply angles nerf
            density -= Math.Max(0, densityAnglesNerf);
            return density;
        }

        public static double EvaluateOverlapDifficultyOf(DifficultyHitObject current)
        {
            var currObj = (OsuDifficultyHitObject)current;
            double screenOverlapDifficulty = 0;

            if (currObj.ReadingObjects.Count == 0)
                return 0;

            var overlapDifficulties = new List<(OsuDifficultyHitObject HitObject, double Difficulty)>();
            var readingObjects = currObj.ReadingObjects;

            // Find initial overlap values
            for (int i = 0; i < readingObjects.Count; i++)
            {
                var loopObj = readingObjects[i].HitObject;
                var loopReadingObjects = (List<OsuDifficultyHitObject.ReadingObject>)loopObj.ReadingObjects;

                if (loopReadingObjects.Count == 0)
                    continue;

                double targetStartTime = currObj.StartTime - currObj.Preempt;
                double overlapness = boundBinarySearch(loopReadingObjects, targetStartTime);

                if (overlapness > 0) overlapDifficulties.Add((loopObj, overlapness));
            }

            if (overlapDifficulties.Count == 0)
                return 0;

            var sortedDifficulties = overlapDifficulties.OrderByDescending(d => d.Difficulty).ToList();

            for (int i = 0; i < sortedDifficulties.Count; i++)
            {
                var harderObject = sortedDifficulties[i];

                // Look for all easier objects
                for (int j = i + 1; j < sortedDifficulties.Count; j++)
                {
                    var easierObject = sortedDifficulties[j];

                    // Get the overlap value
                    double overlapValue = 0;

                    // OverlapValues dict only contains prev objects, so be sure to use right object
                    if (harderObject.HitObject.Index > easierObject.HitObject.Index)
                        harderObject.HitObject.OverlapValues.TryGetValue(easierObject.HitObject.Index, out overlapValue);
                    else
                        easierObject.HitObject.OverlapValues.TryGetValue(harderObject.HitObject.Index, out overlapValue);

                    // Nerf easier object if it overlaps in the same place as hard one
                    easierObject.Difficulty *= Math.Pow(1 - overlapValue, 2);
                }
            }

            const double decay_weight = 0.5;
            const double threshold = 0.6;
            double weight = 1.0;

            foreach (var diffObject in sortedDifficulties.Where(d => d.Difficulty > threshold).OrderByDescending(d => d.Difficulty))
            {
                // Add weighted difficulty
                screenOverlapDifficulty += Math.Max(0, diffObject.Difficulty - threshold) * weight;
                weight *= decay_weight;
            }

            return overlap_multiplier * Math.Max(0, screenOverlapDifficulty);
        }
        public static double EvaluateDifficultyOf(DifficultyHitObject current)
        {
            if (current.BaseObject is Spinner || current.Index == 0)
                return 0;

            double difficulty = Math.Pow(4 * Math.Log(Math.Max(1, ((OsuDifficultyHitObject)current).Density)), 2.5);

            double overlapBonus = EvaluateOverlapDifficultyOf(current) * difficulty;
            difficulty += overlapBonus;

            return difficulty;
        }

        public static double EvaluateAimingDensityFactorOf(DifficultyHitObject current)
        {
            double difficulty = ((OsuDifficultyHitObject)current).Density;

            return Math.Max(0, Math.Pow(difficulty, 1.5) - 1);
        }

        private static double getTimeNerfFactor(double deltaTime) => Math.Clamp(2 - deltaTime / (reading_window_size / 2), 0, 1);
        private static double getRhythmDifference(double t1, double t2) => 1 - Math.Min(t1, t2) / Math.Max(t1, t2);
        private static double logistic(double x) => 1 / (1 + Math.Exp(-x));

        // Finds the overlapness of the last object for which StartTime lower than target
        private static double boundBinarySearch(List<OsuDifficultyHitObject.ReadingObject> arr, double target)
        {
            int low = 0;
            int mid;
            int high = arr.Count;

            int result = -1;

            while (low < high)
            {
                mid = low + (high - low) / 2;

                if (arr[mid].HitObject.StartTime >= target)
                {
                    result = mid;
                    low = mid + 1;
                }
                else high = mid - 1;
            }

            if (result == -1) return 0;
            return arr[result].Overlapness;
        }
    }
}
