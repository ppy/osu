// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Rulesets.Difficulty.Utils
{
    /// <summary>
    /// Store the <see cref="SectionPeaks"/> of a <see cref="DecayingValue"/> strain.
    /// </summary>
    public class DecayingStrainPeaks
    {
        public DecayingValue CurrentStrain;
        public SectionPeaks StrainPeaks;

        public DecayingStrainPeaks(double strainDecayBase, int sectionLength = 400)
        {
            CurrentStrain = new DecayingValue(strainDecayBase);
            StrainPeaks = new SectionPeaks(CurrentStrain.ValueAtTime, sectionLength);
        }

        /// <summary>
        /// Advances time, increments <see cref="CurrentStrain"/> and updates <see cref="StrainPeaks"/>.
        /// </summary>
        /// <returns>The current strain value.</returns>
        public double IncrementStrainAtTime(double time, double strainIncrease)
        {
            StrainPeaks.AdvanceTime(time);
            CurrentStrain.IncrementValueAtTime(time, strainIncrease);
            StrainPeaks.UpdateValue(CurrentStrain.Value);
            return CurrentStrain.Value;
        }
    }
}
