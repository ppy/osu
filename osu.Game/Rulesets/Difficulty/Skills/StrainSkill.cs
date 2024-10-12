// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Lists;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Difficulty.Skills
{
    /// <summary>
    /// Used to processes strain values of <see cref="DifficultyHitObject"/>s, keep track of strain levels caused by the processed objects
    /// and to calculate a final difficulty value representing the difficulty of hitting all the processed objects.
    /// </summary>
    public abstract class StrainSkill : Skill
    {
        /// <summary>
        /// The weight by which each strain value decays.
        /// </summary>
        protected virtual double DecayWeight => 0.9;

        /// <summary>
        /// The length of each strain section.
        /// </summary>
        protected virtual int SectionLength => 400;

        private double currentSectionPeak; // We also keep track of the peak strain level in the current section.

        private double currentSectionEnd;

        private readonly List<double> strainPeaks = new List<double>();

        protected StrainSkill(Mod[] mods)
            : base(mods)
        {
        }

        /// <summary>
        /// Returns the strain value at <see cref="DifficultyHitObject"/>. This value is calculated with or without respect to previous objects.
        /// </summary>
        protected abstract double StrainValueAt(DifficultyHitObject current);

        /// <summary>
        /// Process a <see cref="DifficultyHitObject"/> and update current strain values accordingly.
        /// </summary>
        public sealed override void Process(DifficultyHitObject current)
        {
            // The first object doesn't generate a strain, so we begin with an incremented section end
            if (current.Index == 0)
                currentSectionEnd = Math.Ceiling(current.StartTime / SectionLength) * SectionLength;

            while (current.StartTime > currentSectionEnd)
            {
                saveCurrentPeak();
                startNewSectionFrom(currentSectionEnd, current);
                currentSectionEnd += SectionLength;
            }

            double currentStrain = StrainValueAt(current);

            if (currentSectionPeak < currentStrain)
                currentSectionPeak = currentStrain;
        }

        /// <summary>
        /// Saves the current peak strain level to the list of strain peaks, which will be used to calculate an overall difficulty.
        /// </summary>
        private void saveCurrentPeak()
        {
            strainPeaks.Add(currentSectionPeak);
        }

        /// <summary>
        /// Sets the initial strain level for a new section.
        /// </summary>
        /// <param name="time">The beginning of the new section in milliseconds.</param>
        /// <param name="current">The current hit object.</param>
        private void startNewSectionFrom(double time, DifficultyHitObject current)
        {
            // The maximum strain of the new section is not zero by default
            // This means we need to capture the strain level at the beginning of the new section, and use that as the initial peak level.
            currentSectionPeak = CalculateInitialStrain(time, current);
        }

        /// <summary>
        /// Retrieves the peak strain at a point in time.
        /// </summary>
        /// <param name="time">The time to retrieve the peak strain at.</param>
        /// <param name="current">The current hit object.</param>
        /// <returns>The peak strain.</returns>
        protected abstract double CalculateInitialStrain(double time, DifficultyHitObject current);

        /// <summary>
        /// Returns a live enumerable of the peak strains for each <see cref="SectionLength"/> section of the beatmap,
        /// including the peak of the current section.
        /// </summary>
        public IEnumerable<double> GetCurrentStrainPeaks() => strainPeaks.Append(currentSectionPeak);

        /// <summary>
        /// Returns the calculated difficulty value representing all <see cref="DifficultyHitObject"/>s that have been processed up to this point.
        /// </summary>
        public override double DifficultyValue()
        {
            double difficulty = 0;
            double weight = 1;

            // Difficulty is the weighted sum of the highest strains from every section.
            // We're sorting from highest to lowest strain.
            foreach (double strain in GetCurrentStrainsSorted())
            {
                difficulty += strain * weight;
                weight *= DecayWeight;
            }

            return difficulty;
        }

        /// <summary>
        /// Amount of strains that will be saved in the sorted strains list.
        /// Use value = 0 in case you want all strains to be saved.
        /// </summary>
        protected virtual int MaxStrainCount => 200;

        protected SortedList<double> GetCurrentStrainsSorted()
        {
            int newStrainsToAdd = strainPeaks.Count - amountOfStrainsAddedSinceSave;

            var peaks = strainPeaks.TakeLast(newStrainsToAdd);
            savedSortedStrains.AddRange(peaks);
            amountOfStrainsAddedSinceSave = strainPeaks.Count;

            // We're saving only the largest 200 strains
            int excessStrainsCount = savedSortedStrains.Count - MaxStrainCount;
            if (MaxStrainCount > 0 && excessStrainsCount > 0)
            {
                savedSortedStrains.RemoveRange(savedSortedStrains.Count - excessStrainsCount, excessStrainsCount);
            }

            var strainsWithCurrent = new SortedList<double>((double a, double b) => { return a < b ? 1 : (a > b ? -1 : 0); });
            strainsWithCurrent.AddRange(savedSortedStrains);
            strainsWithCurrent.Add(currentSectionPeak);

            return strainsWithCurrent;
        }

        private SortedList<double> savedSortedStrains = new SortedList<double>((double a, double b) => { return a < b ? 1 : (a > b ? -1 : 0); });
        private int amountOfStrainsAddedSinceSave = 0;
    }
}
