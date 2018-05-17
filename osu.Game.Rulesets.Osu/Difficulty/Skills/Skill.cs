// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Difficulty.Utils;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills
{
    /// <summary>
    /// Used to processes strain values of <see cref="OsuDifficultyHitObject"/>s, keep track of strain levels caused by the processed objects
    /// and to calculate a final difficulty value representing the difficulty of hitting all the processed objects.
    /// </summary>
    public abstract class Skill
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
        /// <see cref="OsuDifficultyHitObject"/>s that were processed previously. They can affect the strain values of the following objects.
        /// </summary>
        protected readonly History<OsuDifficultyHitObject> Previous = new History<OsuDifficultyHitObject>(2); // Contained objects not used yet

        private double currentStrain = 1; // We keep track of the strain level at all times throughout the beatmap.
        private double currentSectionPeak = 1; // We also keep track of the peak strain level in the current section.
        private readonly List<double> strainPeaks = new List<double>();

        /// <summary>
        /// Process an <see cref="OsuDifficultyHitObject"/> and update current strain values accordingly.
        /// </summary>
        public void Process(OsuDifficultyHitObject current)
        {
            currentStrain *= strainDecay(current.DeltaTime);
            if (!(current.BaseObject is Spinner))
                currentStrain += StrainValueOf(current) * SkillMultiplier;

            currentSectionPeak = Math.Max(currentStrain, currentSectionPeak);

            Previous.Push(current);
        }

        /// <summary>
        /// Saves the current peak strain level to the list of strain peaks, which will be used to calculate an overall difficulty.
        /// </summary>
        public void SaveCurrentPeak()
        {
            if (Previous.Count > 0)
                strainPeaks.Add(currentSectionPeak);
        }

        /// <summary>
        /// Sets the initial strain level for a new section.
        /// </summary>
        /// <param name="offset">The beginning of the new section in milliseconds</param>
        public void StartNewSectionFrom(double offset)
        {
            // The maximum strain of the new section is not zero by default, strain decays as usual regardless of section boundaries.
            // This means we need to capture the strain level at the beginning of the new section, and use that as the initial peak level.
            if (Previous.Count > 0)
                currentSectionPeak = currentStrain * strainDecay(offset - Previous[0].BaseObject.StartTime);
        }

        /// <summary>
        /// Returns the calculated difficulty value representing all processed <see cref="OsuDifficultyHitObject"/>s.
        /// </summary>
        public double DifficultyValue()
        {
            strainPeaks.Sort((a, b) => b.CompareTo(a)); // Sort from highest to lowest strain.

            double difficulty = 0;
            double weight = 1;

            // Difficulty is the weighted sum of the highest strains from every section.
            foreach (double strain in strainPeaks)
            {
                difficulty += strain * weight;
                weight *= 0.9;
            }

            return difficulty;
        }

        /// <summary>
        /// Calculates the strain value of an <see cref="OsuDifficultyHitObject"/>. This value is affected by previously processed objects.
        /// </summary>
        protected abstract double StrainValueOf(OsuDifficultyHitObject current);

        private double strainDecay(double ms) => Math.Pow(StrainDecayBase, ms / 1000);
    }
}
