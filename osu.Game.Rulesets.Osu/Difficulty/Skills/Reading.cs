// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Utils;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Difficulty.Utils;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Difficulty.Evaluators;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Mods;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills
{
    public class Reading : HarmonicSkill
    {
        private readonly List<DifficultyHitObject> objectList = new List<DifficultyHitObject>();

        private readonly bool hasHiddenMod;

        public Reading(Mod[] mods)
            : base(mods)
        {
            hasHiddenMod = mods.OfType<OsuModHidden>().Any(m => !m.OnlyFadeApproachCircles.Value);
        }

        private double currentStrain;

        private double strainDecay(double ms) => DiffUtils.Pow(0.8, ms / 1000);

        protected override double ObjectDifficultyOf(DifficultyHitObject current)
        {
            const double skill_multiplier = 2.5;

            objectList.Add(current);

            double decay = strainDecay(current.DeltaTime);

            currentStrain *= decay;
            currentStrain += calculateAdjustedDifficulty(current) * (1 - decay) * skill_multiplier;

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

        protected override List<double> GetTransformedDifficulties(List<double> difficulties)
        {
            difficulties = difficulties.Where(v => v > 0).ToList();

            const double reduced_difficulty_base_line = 0.0; // Assume the first seconds are completely memorised

            int reducedNoteCount = calculateReducedNoteCount();

            for (int i = 0; i < Math.Min(difficulties.Count, reducedNoteCount); i++)
            {
                double scale = Math.Log10(Interpolation.Lerp(1, 10, Math.Clamp((double)i / reducedNoteCount, 0, 1)));
                difficulties[i] *= Interpolation.Lerp(reduced_difficulty_base_line, 1.0, scale);
            }

            return difficulties;
        }

        private int calculateReducedNoteCount()
        {
            const double reduced_difficulty_duration = 60 * 1000;

            if (objectList.Count == 0)
                return 0;

            double reducedDuration = objectList.First().StartTime + reduced_difficulty_duration;

            int reducedNoteCount = 0;

            foreach (var hitObject in objectList)
            {
                if (hitObject.StartTime > reducedDuration)
                    break;

                reducedNoteCount++;
            }

            return reducedNoteCount;
        }

        public override double CountTopWeightedObjectDifficulties(double difficultyValue)
        {
            if (ObjectDifficulties.Count == 0)
                return 0.0;

            if (ObjectWeightSum == 0)
                return 0.0;

            double consistentTopNote = difficultyValue / ObjectWeightSum; // What would the top difficulty be if all object difficulties were identical

            if (consistentTopNote == 0)
                return 0;

            return ObjectDifficulties.Sum(d => DiffUtils.Logistic(d / consistentTopNote, 1.15, 5, 1.1));
        }
    }
}
