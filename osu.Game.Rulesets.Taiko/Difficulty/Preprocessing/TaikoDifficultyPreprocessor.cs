// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Taiko.Difficulty.Preprocessing.Colour;

namespace osu.Game.Rulesets.Taiko.Difficulty.Preprocessing
{
    public class TaikoDifficultyPreprocessor
    {
        /// <summary>
        /// Creates a list of <see cref="TaikoDifficultyHitObject"/>s from a <see cref="IBeatmap"/>s.
        /// This is placed here in a separate class to avoid <see cref="TaikoDifficultyCalculator"/> having to know
        /// too much implementation details of the preprocessing, and avoid  <see cref="TaikoDifficultyHitObject"/>
        /// having circular dependencies with various preprocessing and evaluator classes.
        /// </summary>
        /// <param name="beatmap">The beatmap from which the list of <see cref="TaikoDifficultyHitObject"/> is created.</param>
        /// <param name="clockRate">The rate at which the gameplay clock is run at.</param>
        public static List<DifficultyHitObject> CreateDifficultyHitObjects(IBeatmap beatmap, double clockRate)
        {
            List<DifficultyHitObject> difficultyHitObjects = new List<DifficultyHitObject>();
            List<TaikoDifficultyHitObject> centreObjects = new List<TaikoDifficultyHitObject>();
            List<TaikoDifficultyHitObject> rimObjects = new List<TaikoDifficultyHitObject>();
            List<TaikoDifficultyHitObject> noteObjects = new List<TaikoDifficultyHitObject>();

            for (int i = 2; i < beatmap.HitObjects.Count; i++)
            {
                difficultyHitObjects.Add(
                    new TaikoDifficultyHitObject(
                        beatmap.HitObjects[i], beatmap.HitObjects[i - 1], beatmap.HitObjects[i - 2], clockRate, difficultyHitObjects,
                        centreObjects, rimObjects, noteObjects, difficultyHitObjects.Count)
                );
            }

            TaikoColourDifficultyPreprocessor.ProcessAndAssign(difficultyHitObjects);

            return difficultyHitObjects;
        }
    }
}
