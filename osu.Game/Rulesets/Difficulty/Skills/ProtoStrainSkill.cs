// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Difficulty.Skills
{
    /// <summary>
    /// Used to processes strain values of <see cref="DifficultyHitObject"/>s, keep track of strain levels caused by the processed objects
    /// and to calculate a final difficulty value representing the difficulty of hitting all the processed objects.
    /// </summary>
    public abstract class ProtoStrainSkill : Skill
    {
        /// <summary>
        /// The weight by which each strain value decays.
        /// </summary>
        protected virtual double DecayWeight => 0.9;

        /// <summary>
        /// The length of each strain section.
        /// </summary>
        protected virtual int SectionLength => 400;

        protected double CurrentSectionPeak; // We also keep track of the peak strain level in the current section.
        protected double CurrentSectionEnd = -1;

        private readonly List<double> strainPeaks = new List<double>();
        protected readonly List<double> ObjectStrains = new List<double>(); // Store individual strains

        protected ProtoStrainSkill(Mod[] mods)
            : base(mods)
        {
        }

        /// <summary>
        /// Updates the strain graph to include the new strain.
        /// This function will yield so that new sections can begin. It must be fully enumerated.
        /// </summary>
        public void Push(double time, Func<double> getStrain, Action<double> onNewStrain)
        {
            // The first object doesn't generate a strain, so we begin with an incremented section end
            if (CurrentSectionEnd == -1)
            {
                CurrentSectionEnd = Math.Ceiling(time / SectionLength) * SectionLength;
            }

            while (time > CurrentSectionEnd)
            {
                saveCurrentPeak();
                onNewStrain(CurrentSectionEnd);
                CurrentSectionEnd += SectionLength;
            }

            double strain = getStrain();
            CurrentSectionPeak = Math.Max(strain, CurrentSectionPeak);

            // Store the strain value for the object
            ObjectStrains.Add(strain);
        }

        /// <summary>
        /// Calculates the number of strains weighted against the top strain.
        /// The result is scaled by clock rate as it affects the total number of strains.
        /// </summary>
        public virtual double CountTopWeightedStrains()
        {
            if (ObjectStrains.Count == 0)
                return 0.0;

            double consistentTopStrain = DifficultyValue() / 10; // What would the top strain be if all strain values were identical

            if (consistentTopStrain == 0)
                return ObjectStrains.Count;

            // Use a weighted sum of all strains. Constants are arbitrary and give nice values
            return ObjectStrains.Sum(s => 1.1 / (1 + Math.Exp(-10 * (s / consistentTopStrain - 0.88))));
        }

        /// <summary>
        /// Saves the current peak strain level to the list of strain peaks, which will be used to calculate an overall difficulty.
        /// </summary>
        private void saveCurrentPeak()
        {
            strainPeaks.Add(CurrentSectionPeak);
        }

        /// <summary>
        /// Returns a live enumerable of the peak strains for each <see cref="SectionLength"/> section of the beatmap,
        /// including the peak of the current section.
        /// </summary>
        public IEnumerable<double> GetCurrentStrainPeaks() => strainPeaks.Append(CurrentSectionPeak);

        public IEnumerable<double> GetObjectStrains() => ObjectStrains;

        /// <summary>
        /// Returns the calculated difficulty value representing all <see cref="DifficultyHitObject"/>s that have been processed up to this point.
        /// </summary>
        public override double DifficultyValue()
        {
            double difficulty = 0;
            double weight = 1;

            // Sections with 0 strain are excluded to avoid worst-case time complexity of the following sort (e.g. /b/2351871).
            // These sections will not contribute to the difficulty.
            var peaks = GetCurrentStrainPeaks().Where(p => p > 0);

            // Difficulty is the weighted sum of the highest strains from every section.
            // We're sorting from highest to lowest strain.
            foreach (double strain in peaks.OrderDescending())
            {
                difficulty += strain * weight;
                weight *= DecayWeight;
            }

            return difficulty;
        }
    }
}
