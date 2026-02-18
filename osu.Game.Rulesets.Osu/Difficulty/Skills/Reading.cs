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

        private double currentDifficulty;

        private double skillMultiplier => 2.5;
        private double strainDecayBase => 0.8;

        private double strainDecay(double ms) => Math.Pow(strainDecayBase, ms / 1000);

        protected override double ObjectDifficultyOf(DifficultyHitObject current)
        {
            objectList.Add(current);

            currentDifficulty *= strainDecay(current.DeltaTime);

            currentDifficulty += ReadingEvaluator.EvaluateDifficultyOf(current, hasHiddenMod) * skillMultiplier;

            return currentDifficulty;
        }

        protected override void ApplyDifficultyTransformation(double[] difficulties)
        {
            const double reduced_difficulty_base_line = 0.0; // Assume the first seconds are completely memorised

            int reducedNoteCount = calculateReducedNoteCount();

            for (int i = 0; i < Math.Min(difficulties.Length, reducedNoteCount); i++)
            {
                double scale = Math.Log10(Interpolation.Lerp(1, 10, Math.Clamp((double)i / reducedNoteCount, 0, 1)));
                difficulties[i] *= Interpolation.Lerp(reduced_difficulty_base_line, 1.0, scale);
            }
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

            if (NoteWeightSum == 0)
                return 0.0;

            double consistentTopNote = difficultyValue / NoteWeightSum; // What would the top difficulty be if all object difficulties were identical

            if (consistentTopNote == 0)
                return 0;

            return ObjectDifficulties.Sum(d => DifficultyCalculationUtils.Logistic(d / consistentTopNote, 1.15, 5, 1.1));
        }
    }
}
