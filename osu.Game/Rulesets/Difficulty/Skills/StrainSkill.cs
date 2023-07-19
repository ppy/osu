// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Utils;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Difficulty.Skills
{
    /// <summary>
    /// Convenience base class for a <see cref="Skill"/> making use of <see cref="SectionPeaks"/>
    /// </summary>
    public abstract class StrainSkill : Skill
    {
        /// <summary>
        /// The weight for the exponential sum of strains which produces the final difficulty value
        /// </summary>
        protected virtual double DifficultySumWeight => 0.9;

        protected readonly SectionPeaks StrainPeaks;

        protected StrainSkill(Mod[] mods, int sectionLength = 400)
            : base(mods)
        {
            StrainPeaks = new SectionPeaks(StrainAtTime, sectionLength);
        }

        /// <summary>
        /// Calculates the total strain value at the time of the <see cref="DifficultyHitObject"/>
        /// </summary>
        protected abstract double StrainValueAt(DifficultyHitObject hitObject);

        /// <summary>
        /// Calculate the strain value at a point in time in between hit objects.
        /// </summary>
        protected abstract double StrainAtTime(double time);

        public override void Process(DifficultyHitObject hitObject)
        {
            StrainPeaks.AdvanceTime(hitObject.StartTime);
            StrainPeaks.SetValueAtCurrentTime(StrainValueAt(hitObject));
        }

        public override double DifficultyValue()
        {
            return StrainPeaks.SortedExponentialWeightedSum(DifficultySumWeight);
        }

        public IEnumerable<double> GetCurrentStrainPeaks() => StrainPeaks;
    }
}
