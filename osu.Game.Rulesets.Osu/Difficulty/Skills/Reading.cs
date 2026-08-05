// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Game.Rulesets.Difficulty.Aggregation;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Difficulty.Utils;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Difficulty.Evaluators;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Mods;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills
{
    public class Reading : Skill
    {
        private readonly bool hasHiddenMod;
        private double harmonicWeightSum;

        public Reading(Mod[] mods)
            : base(mods)
        {
            hasHiddenMod = mods.OfType<OsuModHidden>().Any(m => !m.OnlyFadeApproachCircles.Value);
        }

        private double currentStrain;

        private double? firstObjectStartTime;

        private double strainDecay(double ms) => DiffUtils.Pow(0.8, ms / 1000);

        protected override double ProcessInternal(DifficultyHitObject current)
        {
            const double skill_multiplier = 2.5;
            const double reduced_difficulty_duration = 40 * 1000;

            double decay = strainDecay(current.DeltaTime);

            // This currently operates under the assumption that `ObjectDifficultyOf` is called once per object, and in order.
            // Under that assumption, we can trust that `current.StartTime` refers to the start time of the first object in the case that `firstObjectStartTime` is yet to be set.
            firstObjectStartTime ??= current.StartTime;

            const double reduced_difficulty_base_line = 0.2;

            double currentObjectStrain = calculateAdjustedDifficulty(current) * (1 - decay) * skill_multiplier;

            if (current.StartTime <= firstObjectStartTime + reduced_difficulty_duration)
            {
                double scale = Math.Log10(double.Lerp(1, 10, Math.Clamp((current.StartTime - firstObjectStartTime.Value) / reduced_difficulty_duration, 0, 1)));
                currentObjectStrain *= double.Lerp(reduced_difficulty_base_line, 1.0, scale);
            }

            currentStrain *= decay;
            currentStrain += currentObjectStrain;

            return currentStrain;
        }

        private double calculateAdjustedDifficulty(DifficultyHitObject current)
        {
            double difficulty = ReadingEvaluator.EvaluateDifficultyOf(current, hasHiddenMod);

            if (Mods.Any(m => m is OsuModTouchDevice))
                difficulty = DiffUtils.Pow(difficulty, 0.89);

            if (Mods.Any(m => m is OsuModMagnetised))
            {
                float magnetisedStrength = Mods.OfType<OsuModMagnetised>().First().AttractionStrength.Value;
                difficulty *= 1.0 - magnetisedStrength;
            }

            if (Mods.Any(m => m is OsuModRelax))
                difficulty *= 0.4;

            if (Mods.Any(m => m is OsuModAutopilot))
                difficulty *= 0.1;

            difficulty *= 0.825 + DiffUtils.Pow(Math.Max(0, ((OsuDifficultyHitObject)current).OverallDifficulty), 2.2) / 1125.0;

            return difficulty;
        }

        public override double DifficultyValue()
        {
            if (ObjectDifficulties.Count == 0)
                return 0;

            (double difficulty, harmonicWeightSum) = HarmonicSeries.Aggregate(ObjectDifficulties);

            return difficulty;
        }

        public double CountTopWeightedObjectDifficulties(double difficultyValue)
        {
            if (ObjectDifficulties.Count == 0)
                return 0.0;

            if (harmonicWeightSum == 0)
                return 0.0;

            double consistentTopNote = difficultyValue / harmonicWeightSum; // What would the top difficulty be if all object difficulties were identical

            if (consistentTopNote == 0)
                return 0;

            return ObjectDifficulties.Sum(d => DiffUtils.Logistic(d / consistentTopNote, 1.15, 5, 1.1));
        }
    }
}
