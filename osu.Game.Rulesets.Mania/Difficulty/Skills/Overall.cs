// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Mania.Difficulty.Preprocessing;
using osu.Game.Rulesets.Mania.Objects;

namespace osu.Game.Rulesets.Mania.Difficulty.Skills
{
    public class Overall : Skill
    {
        protected override double SkillMultiplier => 1;
        protected override double StrainDecayBase => 0.3;

        private readonly double[] holdEndTimes;

        private readonly int columnCount;

        public Overall(int columnCount)
        {
            this.columnCount = columnCount;

            holdEndTimes = new double[columnCount];
        }

        protected override double StrainValueOf(DifficultyHitObject current)
        {
            var maniaCurrent = (ManiaDifficultyHitObject)current;
            var endTime = (maniaCurrent.BaseObject as HoldNote)?.EndTime ?? maniaCurrent.BaseObject.StartTime;

            double holdFactor = 1.0; // Factor in case something else is held
            double holdAddition = 0; // Addition to the current note in case it's a hold and has to be released awkwardly

            for (int i = 0; i < columnCount; i++)
            {
                // If there is at least one other overlapping end or note, then we get an addition, buuuuuut...
                if (current.BaseObject.StartTime < holdEndTimes[i] && endTime > holdEndTimes[i])
                    holdAddition = 1.0;

                // ... this addition only is valid if there is _no_ other note with the same ending.
                // Releasing multiple notes at the same time is just as easy as releasing one
                if (endTime == holdEndTimes[i])
                    holdAddition = 0;

                // We give a slight bonus if something is held meanwhile
                if (holdEndTimes[i] > endTime)
                    holdFactor = 1.25;
            }

            holdEndTimes[maniaCurrent.BaseObject.Column] = endTime;

            return (1 + holdAddition) * holdFactor;
        }
    }
}
