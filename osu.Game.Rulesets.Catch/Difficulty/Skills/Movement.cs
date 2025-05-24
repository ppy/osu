// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Catch.Difficulty.Evaluators;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Catch.Difficulty.Skills
{
    public class Movement : StrainDecaySkill
    {
        protected override double SkillMultiplier => 1;
        protected override double StrainDecayBase => 0.2;

        protected override double DecayWeight => 0.94;

        protected override int SectionLength => 750;

        protected readonly float HalfCatcherWidth;

        /// <summary>
        /// The speed multiplier applied to the player's catcher.
        /// </summary>
        private readonly double catcherSpeedMultiplier;

        public Movement(Mod[] mods, float halfCatcherWidth, double clockRate)
            : base(mods)
        {
            HalfCatcherWidth = halfCatcherWidth;

            // In catch, clockrate adjustments do not only affect the timings of hitobjects,
            // but also the speed of the player's catcher, which has an impact on difficulty
            // TODO: Support variable clockrates caused by mods such as ModTimeRamp
            //  (perhaps by using IApplicableToRate within the CatchDifficultyHitObject constructor to set a catcher speed for each object before processing)
            catcherSpeedMultiplier = clockRate;
        }

        protected override double StrainValueOf(DifficultyHitObject current)
        {
            return MovementEvaluator.EvaluateDifficultyOf(current, catcherSpeedMultiplier);
        }
    }
}
