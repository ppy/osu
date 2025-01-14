// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Difficulty.Utils;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Taiko.Difficulty.Evaluators;

namespace osu.Game.Rulesets.Taiko.Difficulty.Skills
{
    /// <summary>
    /// Calculates the rhythm coefficient of taiko difficulty.
    /// </summary>
    public class Rhythm : StrainDecaySkill
    {
        protected override double SkillMultiplier => 1.0;
        protected override double StrainDecayBase => 0.4;

        private readonly double greatHitWindow;

        public Rhythm(Mod[] mods, double greatHitWindow)
            : base(mods)
        {
            this.greatHitWindow = greatHitWindow;
        }

        protected override double StrainValueOf(DifficultyHitObject current)
        {
            double difficulty = RhythmEvaluator.EvaluateDifficultyOf(current, greatHitWindow);

            // To prevent abuse of exceedingly long intervals between awkward rhythms, we penalise its difficulty.
            difficulty *= DifficultyCalculationUtils.Logistic(current.DeltaTime, 350, -1 / 25.0, 0.5) + 0.5;

            return difficulty;
        }
    }
}
