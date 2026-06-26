// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Extensions;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Difficulty.Skills
{
    /// <summary>
    /// Similar to <see cref="StrainSkill"/>, but instead of strains having a fixed length, strains can be any length.
    /// A new <see cref="StrainPeak"/> is created for each <see cref="DifficultyHitObject"/>.
    /// </summary>
    /// <remarks>
    /// This class intends to replace <see cref="StrainSkill"/> eventually as it fixes bugs with that implementation.
    /// Has not yet been applied globally as it changes resultant PP values in ways which may require discretion.
    /// </remarks>
    public abstract class VariableLengthStrainSkill : Skill
    {
        /// <summary>
        /// The weight by which each strain value decays.
        /// </summary>
        protected readonly double DecayWeight;

        /// <summary>
        /// The maximum length of each strain section.
        /// </summary>
        protected readonly int MaxSectionLength;

        private double currentSectionPeak; // We also keep track of the peak strain in the current section.
        private double currentSectionBegin;
        private double currentSectionEnd;

        /// <summary>
        /// The number of `MaxSectionLength` sections calculated such that enough of the difficulty value is preserved.
        /// WARNING: This should be overridden if strains are ever used outside of <see cref="Skill.DifficultyValue"/>,
        /// or if <see cref="Skill.DifficultyValue"/> is overridden to not use the default geometric sum. This should be removed
        /// in the future when a better memory-saving technique is implemented.
        /// </summary>
        private readonly double maxStoredLength;

        private readonly List<StrainPeak> strainPeaks = new List<StrainPeak>();

        private double totalLength;

        /// <summary>
        /// Stores previous strains so that, if a high difficulty hit object is followed by a lower
        /// difficulty hit object, the high difficulty hit object gets a full strain instead of being cut short.
        /// </summary>
        private readonly List<(double StrainValue, double StartTime)> queuedStrains = new List<(double, double)>();

        /// <summary>
        /// Create a new <see cref="VariableLengthStrainSkill"/>.
        /// </summary>
        /// <param name="mods">The mods.</param>
        /// <param name="decayWeight">The weight by which each strain value decays.</param>
        /// <param name="maxSectionLength">The maximum length of each strain section.</param>
        protected VariableLengthStrainSkill(Mod[] mods, double decayWeight = 0.9, int maxSectionLength = 400)
            : base(mods)
        {
            DecayWeight = decayWeight;
            MaxSectionLength = maxSectionLength;

            maxStoredLength = 11 / (1 - DecayWeight);
        }

        /// <summary>
        /// Returns the strain value at <see cref="DifficultyHitObject"/>. This value is calculated with or without respect to previous objects.
        /// </summary>
        protected abstract double StrainValueAt(DifficultyHitObject current);

        /// <summary>
        /// Process a <see cref="DifficultyHitObject"/> and update current strain values accordingly.
        /// </summary>
        protected sealed override double ProcessInternal(DifficultyHitObject current)
        {
            // If we're on the first object, set up the first section to end `MaxSectionLength` after it.
            if (current.Index == 0)
            {
                currentSectionBegin = current.StartTime;
                currentSectionEnd = currentSectionBegin + MaxSectionLength;

                // No work is required for first object after calculating difficulty
                currentSectionPeak = StrainValueAt(current);
                return currentSectionPeak;
            }

            backfillPeaks(current);

            double currentStrain = StrainValueAt(current);

            // If the current strain is larger than the current peak, begin a new peak
            // Otherwise, add the current strain to the queue
            if (currentStrain > currentSectionPeak)
            {
                // Clear the queue since none of the strains inside of it will be contributing to the difficulty.
                queuedStrains.Clear();

                // End the current section with the new peak
                saveCurrentPeak(current.StartTime - currentSectionBegin);

                // Set up the new section to start at the current object with the current strain
                currentSectionBegin = current.StartTime;
                currentSectionEnd = currentSectionBegin + MaxSectionLength;
                currentSectionPeak = currentStrain;
            }
            else
            {
                // Empty the queue of smaller elements as they won't be relevant to difficulty
                while (queuedStrains.Count > 0 && queuedStrains[^1].StrainValue < currentStrain)
                    queuedStrains.RemoveAt(queuedStrains.Count - 1);

                queuedStrains.Add((currentStrain, current.StartTime));
            }

            return currentStrain;
        }

        /// <summary>
        /// Fills the space between the end of the current section and the current object, if there is any.
        /// </summary>
        /// <param name="current">The object who's <see cref="DifficultyHitObject.StartTime"/> is backfilled to.</param>
        private void backfillPeaks(DifficultyHitObject current)
        {
            // If the current object starts after the current section ends
            // then we want to start a new section without any harsh drop-off.
            // If we have previous strains that influence the current difficulty we will prioritise those first.
            // Otherwise, start with the current object's initial strain.
            while (current.StartTime > currentSectionEnd)
            {
                // Save the current peak, marking the end of the section.
                saveCurrentPeak(currentSectionEnd - currentSectionBegin);
                currentSectionBegin = currentSectionEnd;

                // If we have any strains queued, then we will use those until the object falls into the new section.
                if (queuedStrains.Count > 0)
                {
                    (double strain, double startTime) = queuedStrains[0];
                    queuedStrains.RemoveAt(0);

                    // We want the section to end `MaxSectionLength` after the strain we're using as an influence.
                    // This effectively means the queued strain will exist in its own section if the gap between the queued strain and current object is large enough.
                    // This is required to make sure there's no harsh difficulty difference between 2 sections if there was a large gap.
                    currentSectionEnd = startTime + MaxSectionLength;
                    startNewSectionFrom(currentSectionBegin, current);

                    // If the current object's peak was higher, we don't want to override it with a lower strain.
                    // Only use the queued strain if it contributes more difficulty.
                    currentSectionPeak = Math.Max(currentSectionPeak, strain);
                }
                // If the queue is empty then we should start the section from the current object instead.
                // The queue can be empty if we're starting off of the back of a new peak, or if we drained through all the queue
                // and the current object is still later than the section end.
                else
                {
                    // We don't have any prior strains to take as a reference, so end the new section `MaxSectionLength` after it starts.
                    currentSectionEnd = currentSectionBegin + MaxSectionLength;
                    startNewSectionFrom(currentSectionBegin, current);
                }
            }
        }

        /// <summary>
        /// Saves the current peak strain level to the list of strain peaks, which will be used to calculate an overall difficulty.
        /// </summary>
        private void saveCurrentPeak(double sectionLength)
        {
            strainPeaks.AddInPlace(new StrainPeak(currentSectionPeak, sectionLength));
            totalLength += sectionLength;

            // Remove from the back of our strain peaks if there's any which are too deep to contribute to difficulty.
            // `maxStoredLength` dictates for us how many sections will preserve at least 99.999% of the difficulty value.
            while (totalLength > maxStoredLength * MaxSectionLength)
            {
                totalLength -= strainPeaks[^1].SectionLength;
                strainPeaks.RemoveAt(strainPeaks.Count - 1);
            }
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

        private bool peaksFinalised;

        /// <summary>
        /// Returns a live enumerable of the peak strains for each <see cref="MaxSectionLength"/> section of the beatmap,
        /// including the peak of the current section.
        /// </summary>
        public IEnumerable<StrainPeak> GetCurrentStrainPeaks()
        {
            if (!peaksFinalised)
            {
                saveCurrentPeak(currentSectionEnd - currentSectionBegin);
                peaksFinalised = true;
            }

            return strainPeaks;
        }

        /// <summary>
        /// Calculates the number of strains weighted against the top strain.
        /// The result is scaled by clock rate as it affects the total number of strains.
        /// </summary>
        public virtual double CountTopWeightedStrains(double difficultyValue)
        {
            if (ObjectDifficulties.Count == 0)
                return 0.0;

            double consistentTopStrain = difficultyValue * (1 - DecayWeight); // What would the top strain be if all strain values were identical

            if (consistentTopStrain == 0)
                return ObjectDifficulties.Count;

            // Use a weighted sum of all strains. Constants are arbitrary and give nice values
            return ObjectDifficulties.Sum(s => 1.1 / (1 + Math.Exp(-10 * (s / consistentTopStrain - 0.88))));
        }

        /// <summary>
        /// Used to store the difficulty of a section of a map.
        /// </summary>
        public readonly struct StrainPeak : IComparable<StrainPeak>
        {
            public StrainPeak(double value, double sectionLength)
            {
                Value = value;
                SectionLength = Math.Round(sectionLength);
            }

            public double Value { get; }
            public double SectionLength { get; }

            // Reverse sort, highest is first.
            public int CompareTo(StrainPeak other) => other.Value.CompareTo(Value);
        }
    }
}
