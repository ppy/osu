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
    public abstract class VariableLengthStrainSkill : Skill
    {
        /// <summary>
        /// The weight by which each strain value decays.
        /// </summary>
        protected virtual double DecayWeight => 0.9;

        /// <summary>
        /// The maximum length of each strain section.
        /// </summary>
        protected virtual int MaxSectionLength => 400;

        private double currentSectionPeak; // We also keep track of the peak strain in the current section.
        private double currentSectionBegin;
        private double currentSectionEnd;

        private static readonly Comparer<StrainPeak> strain_peak_comparer = Comparer<StrainPeak>.Create((p1, p2) => p2.Value.CompareTo(p1.Value));

        /// <summary>
        /// The number of `MaxSectionLength` sections calculated such that 99.999% of the difficulty value is preserved.
        /// </summary>
        private double cutOffTime => -5 * Math.Log(10) / Math.Log(DecayWeight);

        private readonly List<StrainPeak> strainPeaks = new List<StrainPeak>();

        private double totalLength;

        /// <summary>
        /// Stores previous strains so that, if a high difficulty hit object is followed by a lower
        /// difficulty hit object, the high difficulty hit object gets a full strain instead of being cut short.
        /// </summary>
        private readonly List<StrainObject> queuedStrains = new List<StrainObject>();

        protected VariableLengthStrainSkill(Mod[] mods)
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
                    StrainObject queuedStrain = queuedStrains[0];
                    queuedStrains.RemoveAt(0); // Could likely optimize this to remove all used strains at once

                    // We want the section to end `MaxSectionLength` after the strain we're using as an influence.
                    // This effectively means the queued strain will exist in its own section if the gap between the queued strain and current object is large enough.
                    // This is required to make sure there's no harsh difficulty difference between 2 sections if there was a large gap.
                    currentSectionEnd = queuedStrain.StartTime + MaxSectionLength;
                    startNewSectionFrom(currentSectionBegin, current);

                    // If the current object's peak was higher, we don't want to override it with a lower strain.
                    // Only use the queued strain if it contributes more difficulty.
                    currentSectionPeak = Math.Max(currentSectionPeak, queuedStrain.Value);
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

            double strain = StrainValueAt(current);

            // If the current strain is smaller than the current peak, add it to the queue
            if (strain < currentSectionPeak)
            {
                // Empty the queue of smaller elements as they won't be relevant to difficulty
                while (queuedStrains.Count > 0 && queuedStrains[^1].Value < strain)
                    queuedStrains.RemoveAt(queuedStrains.Count - 1);

                queuedStrains.Add(new StrainObject(strain, current.StartTime));
            }
            else
            {
                // Clear the queue since none of the strains inside of it will be contributing to the difficulty.
                queuedStrains.Clear();

                // End the current section with the new peak
                saveCurrentPeak(current.StartTime - currentSectionBegin);

                // Set up the new section to start at the current object with the current strain
                currentSectionBegin = current.StartTime;
                currentSectionEnd = currentSectionBegin + MaxSectionLength;
                currentSectionPeak = strain;
            }

            return strain;
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
        /// Saves the current peak strain level to the list of strain peaks, which will be used to calculate an overall difficulty.
        /// </summary>
        private void saveCurrentPeak(double sectionLength)
        {
            strainPeaks.AddInPlace(new StrainPeak(currentSectionPeak, sectionLength), strain_peak_comparer);
            totalLength += sectionLength;

            // Remove from the back of our strain peaks if there's any which are too deep to contribute to difficulty.
            // `cutOffTime` dictates for us how many sections will preserve at least 99.999% of the difficulty value.
            while (totalLength / MaxSectionLength > cutOffTime)
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

        /// <summary>
        /// Returns a live enumerable of the peak strains for each <see cref="MaxSectionLength"/> section of the beatmap,
        /// including the peak of the current section.
        /// </summary>
        public IEnumerable<StrainPeak> GetCurrentStrainPeaks() => strainPeaks.Append(new StrainPeak(currentSectionPeak, currentSectionEnd - currentSectionBegin));

        /// <summary>
        /// Returns the calculated difficulty value representing all <see cref="DifficultyHitObject"/>s that have been processed up to this point.
        /// </summary>
        public override double DifficultyValue()
        {
            double difficulty = 0;

            // Sections with 0 strain are excluded to avoid worst-case time complexity of the following sort (e.g. /b/2351871).
            // These sections will not contribute to the difficulty.
            var peaks = GetCurrentStrainPeaks().Where(p => p.Value > 0);

            List<StrainPeak> strains = peaks.OrderByDescending(p => (p.Value, p.SectionLength)).ToList();

            // Time is measured in units of strains
            double time = 0;

            // Difficulty is a continuous weighted sum of the sorted strains
            for (int i = 0; i < strains.Count; i++)
            {
                /* Weighting function can be thought of as:
                        b
                        ∫ DecayWeight^x dx
                        a
                    where a = startTime and b = endTime

                    Technically, the function below has been slightly modified from the equation above.
                    The real function would be (Math.Pow(DecayWeight, startTime) - Math.Pow(DecayWeight, endTime)) / Math.Log(1 / DecayWeight)

                    This is an intentional change to ensure the relation between individual chunk values and difficulty value remains the same as StrainSkill.
                    E.g. for a DecayWeight of 0.9, we're multiplying by 10 instead of 9.49122...
                */
                double startTime = time;
                double endTime = time + strains[i].SectionLength / MaxSectionLength;

                double weight = (Math.Pow(DecayWeight, startTime) - Math.Pow(DecayWeight, endTime)) / (1 - DecayWeight);

                difficulty += strains[i].Value * weight;
                time += strains[i].SectionLength / MaxSectionLength;
            }

            return difficulty;
        }

        /// <summary>
        /// Used to store the difficulty of a section of a map.
        /// <remarks>Not to be confused with <see cref="StrainObject"/></remarks>
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

            public int CompareTo(StrainPeak other)
            {
                return Value.CompareTo(other.Value);
            }
        }

        /// <summary>
        /// Used to store the difficulty and start time of an object in a map.
        /// <remarks>Not to be confused with <see cref="StrainPeak"/></remarks>
        /// </summary>
        private readonly struct StrainObject
        {
            public StrainObject(double value, double startTime)
            {
                Value = value;
                StartTime = startTime;
            }

            public double Value { get; }
            public double StartTime { get; }
        }
    }
}
