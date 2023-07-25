// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace osu.Game.Rulesets.Difficulty.Utils
{
    /// <summary>
    /// Container for storing the peak value for consecutive intervals of a time series
    /// Used to store peak strain values for difficulty calculation
    /// </summary>
    public class SectionPeaks : IEnumerable<double>
    {
        /// <summary>
        /// The duration of each interval.
        /// </summary>
        private readonly int sectionLength;

        /// <summary>
        /// Calculate the value at a time point in the future, assuming nothing else happens
        /// Used to calculate the peak at the beginning of each section
        /// </summary>
        private readonly Func<double, double> valueAtTime;

        private double currentSectionPeak;
        private double currentSectionEnd;
        private readonly List<double> strainPeaks = new List<double>();

        public SectionPeaks(Func<double, double> valueAtTime, int sectionLength = 400)
        {
            this.sectionLength = sectionLength;
            this.valueAtTime = valueAtTime;
        }

        /// <summary>
        /// Save strain peaks up to given time
        /// </summary>
        public void AdvanceTime(double time)
        {
            // On the first call, start with incremented currentSectionEnd. This avoids storing peaks before the first section has begun.
            if (currentSectionEnd == 0)
                currentSectionEnd = Math.Ceiling(time / sectionLength) * sectionLength;

            while (time > currentSectionEnd)
            {
                saveCurrentPeak();
                currentSectionPeak = valueAtTime(currentSectionEnd);
                currentSectionEnd += sectionLength;
            }
        }

        /// <summary>
        /// Update current value, setting <see cref="currentSectionPeak"/> if necessary
        /// </summary>
        public void SetValueAtCurrentTime(double value)
        {
            currentSectionPeak = Math.Max(value, currentSectionPeak);
        }

        /// <summary>
        /// Saves the current peak value to the list of peaks.
        /// </summary>
        private void saveCurrentPeak()
        {
            strainPeaks.Add(currentSectionPeak);
        }

        /// <summary>
        /// Enumerating yields the peak strains for each <see cref="sectionLength"/> section of the beatmap,
        /// including the peak of the current section.
        /// </summary>
        public IEnumerator<double> GetEnumerator() => strainPeaks.Append(currentSectionPeak).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
