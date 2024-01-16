// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Difficulty.Skills
{
    /// <summary>
    /// A abstract skill with available per objet difficulty.
    /// </summary>
    /// <remarks>
    /// This class should be considered a "processing" class and not persisted.
    /// </remarks>
    public abstract class GraphSkill : Skill
    {
        protected GraphSkill(Mod[] mods)
            : base(mods)
        {
        }

        /// <summary>
        /// The length of each section.
        /// </summary>
        protected virtual int SectionLength => 400;

        protected double CurrentSectionPeak; // We also keep track of the peak level in the current section.

        protected double CurrentSectionEnd;

        protected readonly List<double> StrainPeaks = new List<double>();

        /// <summary>
        /// Returns a live enumerable of the difficulties
        /// </summary>
        public virtual IEnumerable<double> GetCurrentStrainPeaks() => StrainPeaks.Append(CurrentSectionPeak);
    }
}
