// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Difficulty.Aggregation;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Difficulty.Utils;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Difficulty.Evaluators.Speed;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills
{
    public class SpeedAttributes : ISkillAttributes
    {
        public required double Difficulty { get; init; }
        public required List<double> ObjectDifficulties { get; init; }
        public required double RelevantObjectCount { get; init; }
        public required double TopWeightedSlidersCount { get; init; }
        public required double TopWeightedObjectDifficultiesCount { get; init; }
    }

    /// <summary>
    /// Represents the skill required to press keys with regards to keeping up with the speed at which objects need to be hit.
    /// </summary>
    public class Speed : ISkill
    {
        public IReadOnlyList<Mod> Mods { get; init; }
        public IReadOnlyList<DifficultyHitObject> DifficultyHitObjects { get; init; }

        private readonly List<double> sliderStrains = new List<double>();

        private double currentStrain;

        private const double harmonic_scale = 20.0;

        public Speed(Mod[] mods, DifficultyHitObject[] difficultyHitObjects)
        {
            Mods = mods;
            DifficultyHitObjects = difficultyHitObjects;
        }

        private double strainDecay(double ms) => DiffUtils.Pow(0.3, ms / 1000);

        private double objectDifficultyOf(DifficultyHitObject current)
        {
            const double skill_multiplier = 1.16;

            if (Mods.Any(m => m is OsuModRelax))
                return 0;

            double decay = strainDecay(((OsuDifficultyHitObject)current).AdjustedDeltaTime);

            currentStrain *= decay;
            currentStrain += calculateAdjustedDifficulty(current) * (1 - decay) * skill_multiplier;

            double currentRhythm = RhythmEvaluator.EvaluateDifficultyOf(current);

            double totalStrain = currentStrain * currentRhythm;

            if (current.BaseObject is Slider)
                sliderStrains.Add(totalStrain);

            return totalStrain;
        }

        private double calculateAdjustedDifficulty(DifficultyHitObject current)
        {
            double difficulty = SpeedEvaluator.EvaluateDifficultyOf(current);

            if (Mods.Any(m => m is OsuModAutopilot))
                difficulty *= 0.5;

            return difficulty;
        }

        public ISkillAttributes Process()
        {
            var objectDifficulties = new List<double>();

            foreach (var difficultyHitObject in DifficultyHitObjects)
            {
                objectDifficulties.Add(objectDifficultyOf(difficultyHitObject));
            }

            (double difficulty, double harmonicWeightSum) = HarmonicSeries.Aggregate(objectDifficulties, harmonicScale: harmonic_scale);

            return new SpeedAttributes
            {
                Difficulty = difficulty,
                ObjectDifficulties = objectDifficulties,
                RelevantObjectCount = relevantObjectCount(objectDifficulties),
                TopWeightedSlidersCount = countTopWeightedDifficulties(sliderStrains, harmonicWeightSum, difficulty),
                TopWeightedObjectDifficultiesCount = countTopWeightedDifficulties(objectDifficulties, harmonicWeightSum, difficulty)
            };
        }

        public IEnumerable<TimedSkillAttributes> ProcessTimed()
        {
            var objectDifficulties = new List<double>();

            foreach (var difficultyHitObject in DifficultyHitObjects)
            {
                objectDifficulties.Add(objectDifficultyOf(difficultyHitObject));

                (double difficulty, double harmonicWeightSum) = HarmonicSeries.Aggregate(objectDifficulties, harmonicScale: harmonic_scale);

                yield return new TimedSkillAttributes(new SpeedAttributes
                {
                    Difficulty = difficulty,
                    ObjectDifficulties = objectDifficulties,
                    RelevantObjectCount = relevantObjectCount(objectDifficulties),
                    TopWeightedSlidersCount = countTopWeightedDifficulties(sliderStrains, harmonicWeightSum, difficulty),
                    TopWeightedObjectDifficultiesCount = countTopWeightedDifficulties(objectDifficulties, harmonicWeightSum, difficulty)
                }, difficultyHitObject.EndTime);
            }
        }

        private double relevantObjectCount(List<double> objectDifficulties)
        {
            if (objectDifficulties.Count == 0)
                return 0;

            double maxStrain = objectDifficulties.Max();

            if (maxStrain == 0)
                return 0;

            return objectDifficulties.Sum(strain => DiffUtils.Logistic(strain / maxStrain, 0.5, 12.0));
        }

        private double countTopWeightedDifficulties(List<double> difficulties, double harmonicWeightSum, double difficultyValue)
        {
            if (difficulties.Count == 0)
                return 0.0;

            if (harmonicWeightSum == 0)
                return 0.0;

            double consistentTopObject = difficultyValue / harmonicWeightSum; // What would the top difficulty be if all object difficulties were identical

            if (consistentTopObject == 0)
                return 0;

            return difficulties.Sum(d => DiffUtils.Logistic(d / consistentTopObject, 0.88, 10, 1.1));
        }
    }
}
