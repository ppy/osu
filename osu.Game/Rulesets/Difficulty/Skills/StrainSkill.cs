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
    public abstract class StrainSkill : Skill
    {
        /// <summary>
        /// Strain values are multiplied by this number for the given skill. Used to balance the value of different skills between each other.
        /// </summary>
        protected abstract double SkillMultiplier { get; }

        /// <summary>
        /// Determines how quickly strain decays for the given skill.
        /// For example a value of 0.15 indicates that strain decays to 15% of its original value in one second.
        /// </summary>
        protected abstract double StrainDecayBase { get; }

        /// <summary>
        /// The weight by which each strain value decays.
        /// </summary>
        protected virtual double DecayWeight => 0.9;

        /// <summary>
        /// The current strain level.
        /// </summary>
        protected double CurrentStrain { get; private set; } = 1;

        /// <summary>
        /// The length of each strain section.
        /// </summary>
        protected virtual int SectionLength => 400;

        private double currentSectionPeak = 1; // We also keep track of the peak strain level in the current section.

        private double currentSectionEnd;

        private readonly List<double> strainPeaks = new List<double>();

        protected StrainSkill(Mod[] mods)
            : base(mods)
        {
        }

        /// <summary>
        /// Process a <see cref="DifficultyHitObject"/> and update current strain values accordingly.
        /// </summary>
        protected sealed override void Process(DifficultyHitObject current)
        {
            // The first object doesn't generate a strain, so we begin with an incremented section end
            if (Previous.Count == 0)
                currentSectionEnd = Math.Ceiling(current.StartTime / SectionLength) * SectionLength;

            while (current.StartTime > currentSectionEnd)
            {
                saveCurrentPeak();
                startNewSectionFrom(currentSectionEnd);
                currentSectionEnd += SectionLength;
            }

            CurrentStrain *= strainDecay(current.DeltaTime);
            CurrentStrain += StrainValueOf(current) * SkillMultiplier;

            currentSectionPeak = Math.Max(CurrentStrain, currentSectionPeak);
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
        private void startNewSectionFrom(double time)
        {
            // The maximum strain of the new section is not zero by default, strain decays as usual regardless of section boundaries.
            // This means we need to capture the strain level at the beginning of the new section, and use that as the initial peak level.
            currentSectionPeak = GetPeakStrain(time);
        }

        /// <summary>
        /// Retrieves the peak strain at a point in time.
        /// </summary>
        /// <param name="time">The time to retrieve the peak strain at.</param>
        /// <returns>The peak strain.</returns>
        protected virtual double GetPeakStrain(double time) => CurrentStrain * strainDecay(time - Previous[0].StartTime);

        /// <summary>
        /// Returns a live enumerable of the peak strains for each <see cref="SectionLength"/> section of the beatmap,
        /// including the peak of the current section.
        /// </summary>
        public IEnumerable<double> GetCurrentStrainPeaks() => strainPeaks.Append(currentSectionPeak);

        /// <summary>
        /// Returns the calculated difficulty value representing all <see cref="DifficultyHitObject"/>s that have been processed up to this point.
        /// </summary>
        public sealed override double DifficultyValue()
        {
            double difficulty = 0;
            double weight = 1;

            // Difficulty is the weighted sum of the highest strains from every section.
            // We're sorting from highest to lowest strain.
            foreach (double strain in GetCurrentStrainPeaks().OrderByDescending(d => d))
            {
                difficulty += strain * weight;
                weight *= DecayWeight;
            }

            return difficulty;
        }

        /// <summary>
        /// Calculates the strain value of a <see cref="DifficultyHitObject"/>. This value is affected by previously processed objects.
        /// </summary>
        protected abstract double StrainValueOf(DifficultyHitObject current);

        private double strainDecay(double ms) => Math.Pow(StrainDecayBase, ms / 1000);
    }
}
