using System;
using System.Collections.Generic;
using System.Linq;

namespace osu.Game.Rulesets.Difficulty.Utils
{
    public class StrainSections
    {
        /// <summary>
        /// The length of each strain section.
        /// </summary>
        private readonly int sectionLength;

        private Func<double, double> strainValueAtTime;

        private double currentSectionPeak = 1; // Why 1?

        private double currentSectionEnd;

        private readonly List<double> strainPeaks = new List<double>();

        public StrainSections(Func<double, double> strainValueAtTime, int sectionLength = 400)
        {
            this.sectionLength = sectionLength;
            this.strainValueAtTime = strainValueAtTime;
        }

        /// <summary>
        /// Save strain peaks up to given time
        /// </summary>
        public void UpdateTime(double time)
        {
            // The first object doesn't generate a strain, so we begin with an incremented section end
            if (currentSectionEnd == 0)
                currentSectionEnd = Math.Ceiling(time / sectionLength) * sectionLength;

            while (time > currentSectionEnd)
            {
                saveCurrentPeak();
                currentSectionPeak = strainValueAtTime(currentSectionEnd);
                currentSectionEnd += sectionLength;
            }

        }

        /// <summary>
        /// Update current strain value, setting currentSectionPeak if necessary
        /// </summary>
        public void UpdateStrainPeak(double strain)
        {
            currentSectionPeak = Math.Max(strain, currentSectionPeak);
        }

        /// <summary>
        /// Saves the current peak strain level to the list of strain peaks, which will be used to calculate an overall difficulty.
        /// </summary>
        private void saveCurrentPeak()
        {
            strainPeaks.Add(currentSectionPeak);
        }

        /// <summary>
        /// Returns a live enumerable of the peak strains for each <see cref="sectionLength"/> section of the beatmap,
        /// including the peak of the current section.
        /// </summary>
        public IEnumerable<double> GetCurrentStrainPeaks() => strainPeaks.Append(currentSectionPeak);

        /// <summary>
        /// Returns the calculated difficulty value representing all strains that have been processed up to this point.
        /// </summary>
        public double ExponentialWeightedSum(double decayWeight=0.9)
        {
            double difficulty = 0;
            double weight = 1;

            // Difficulty is the weighted sum of the highest strains from every section.
            // We're sorting from highest to lowest strain.
            foreach (double strain in GetCurrentStrainPeaks().OrderByDescending(d => d))
            {
                difficulty += strain * weight;
                weight *= decayWeight;
            }

            return difficulty;
        }
    }
}
