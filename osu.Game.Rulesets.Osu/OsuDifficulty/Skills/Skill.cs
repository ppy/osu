// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.OsuDifficulty.Preprocessing;
using osu.Game.Rulesets.Osu.OsuDifficulty.Utils;

namespace osu.Game.Rulesets.Osu.OsuDifficulty.Skills
{
    public abstract class Skill
    {
        protected abstract double skillMultiplier { get; }
        protected abstract double strainDecayBase { get; }

        protected OsuDifficultyHitObject current;
        protected History<OsuDifficultyHitObject> previous = new History<OsuDifficultyHitObject>(2); // Contained objects not used yet

        private double currentStrain = 1; // We keep track of the strain level at all times throughout the beatmap.
        private double currentSectionPeak = 1; // We also keep track of the peak strain level in the current section.
        private List<double> strainPeaks = new List<double>();

        /// <summary>
        /// Process a HitObject and update current strain values accordingly.
        /// </summary>
        public void Process(OsuDifficultyHitObject h)
        {
            current = h;

            currentStrain *= strainDecay(current.MS);
            if (!(current.BaseObject is Spinner))
                currentStrain += strainValue() * skillMultiplier;

            currentSectionPeak = Math.Max(currentStrain, currentSectionPeak);
            
            previous.Push(current);
        }

        /// <summary>
        /// Saves the current peak strain level to the list of strain peaks, which will be used to calculate an overall difficulty.
        /// </summary>
        public void SaveCurrentPeak()
        {
            if (previous.Count > 0)
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
            if (previous.Count > 0)
                currentSectionPeak = currentStrain * strainDecay(offset - previous[0].BaseObject.StartTime);
        }

        /// <summary>
        /// Returns the calculated difficulty value representing all currently processed HitObjects.
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

        protected abstract double strainValue();

        private double strainDecay(double ms) => Math.Pow(strainDecayBase, ms / 1000);
    }
}
