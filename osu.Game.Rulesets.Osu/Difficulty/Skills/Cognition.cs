// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Difficulty.Evaluators;
using osu.Game.Rulesets.Osu.Mods;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills
{
    public class Cognition : Skill
    {
        private readonly List<double> difficulties = new List<double>();
        private readonly bool hasHiddenMod;
        private const double skill_multiplier = 34;

        public Cognition(Mod[] mods)
            : base(mods)
        {
            hasHiddenMod = mods.Any(m => m is OsuModHidden);
        }

        public override void Process(DifficultyHitObject current) => difficulties.Add(CognitionEvaluator.EvaluateDifficultyOf(current, hasHiddenMod) * skill_multiplier);

        public override double DifficultyValue()
        {
            double difficulty = 0;

            for (int i = 0; i < difficulties.Count; i++)
                difficulty += difficulties[i] * weight(i);

            return Math.Pow(difficulty, 0.5);
        }

        private double weight(int x) => x / (x + 200.0);
    }
}
