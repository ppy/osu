// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Mania.Difficulty.Preprocessing;
using osu.Game.Rulesets.Mania.Objects;

namespace osu.Game.Rulesets.Mania.Difficulty.Skills
{
    public class Individual : Skill
    {
        protected override double SkillMultiplier => 1;
        protected override double StrainDecayBase => 0.125;

        private readonly double[] holdEndTimes;

        private readonly int column;

        public Individual(int column, int columnCount)
        {
            this.column = column;

            holdEndTimes = new double[columnCount];
        }

        protected override double StrainValueOf(DifficultyHitObject current)
        {
            var maniaCurrent = (ManiaDifficultyHitObject)current;
            var endTime = (maniaCurrent.BaseObject as HoldNote)?.EndTime ?? maniaCurrent.BaseObject.StartTime;

            try
            {
                if (maniaCurrent.BaseObject.Column != column)
                    return 0;

                // We give a slight bonus if something is held meanwhile
                return holdEndTimes.Any(t => t > endTime) ? 2.5 : 2;
            }
            finally
            {
                holdEndTimes[maniaCurrent.BaseObject.Column] = endTime;
            }
        }
    }
}
