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
    /// Similar to <see cref="StrainSkill"/>, but instead of strains having a fixed length, strains can be any length.
    /// A new <see cref="Strain"/> is created for each <see cref="DifficultyHitObject"/>.
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

        private double currentSectionPeak; // We also keep track of the peak strain level in the current section.
        private double currentSectionBegin;
        private double currentSectionEnd;

        /// <summary>
        /// Used to store the difficulty of a section of a map.
        /// </summary>
        public struct Strain : IComparable<Strain>
        {
            public Strain(double value, double sectionLength)
            {
                Value = value;
                SectionLength = sectionLength;
            }

            public double Value { get; set; }
            public double SectionLength { get; }

            public int CompareTo(Strain other)
            {
                return Value.CompareTo(other.Value);
            }
        }

        private readonly List<Strain> strainPeaks = new List<Strain>();
        protected readonly List<double> ObjectStrains = new List<double>(); // Store individual strains

        /// <summary>
        /// Stores previous strains so that, if a high difficulty hit object is followed by a lower
        /// difficulty hit object, the high difficulty hit object gets a full strain instead of being cut short.
        /// <typeparam name="double">Strain difficulty</typeparam>
        /// <typeparam name="double">Strain start time</typeparam>
        /// <remarks>In the case that continuous strains is implemented, please remove this</remarks>
        /// </summary>
        private readonly PriorityQueue<double, double> strainPeakQueue = new PriorityQueue<double, double>();

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
        public sealed override void Process(DifficultyHitObject current)
        {
            // The first object doesn't generate a strain, so we begin with an incremented section end
            if (current.Index == 0)
            {
                currentSectionBegin = current.StartTime;
                currentSectionEnd = currentSectionBegin + MaxSectionLength;
            }

            double deltaTime = current.DeltaTime;

            while (current.StartTime > currentSectionEnd)
            {
                // Pull from queue if possible
                if (strainPeakQueue.Count > 0)
                {
                    saveCurrentPeak(currentSectionEnd - currentSectionBegin);
                    strainPeakQueue.TryDequeue(out double storedStrain, out double storedStrainStartTime);
                    deltaTime -= currentSectionEnd - currentSectionBegin;
                    currentSectionBegin = currentSectionEnd;
                    currentSectionEnd = storedStrainStartTime + MaxSectionLength;
                    startNewSectionFrom(currentSectionBegin, current);
                    currentSectionPeak = Math.Max(currentSectionPeak, storedStrain);
                }
                else
                {
                    // Create new strains if queue is empty
                    saveCurrentPeak(currentSectionEnd - currentSectionBegin);
                    deltaTime -= currentSectionEnd - currentSectionBegin;
                    currentSectionBegin = currentSectionEnd;
                    currentSectionEnd = currentSectionBegin + MaxSectionLength;
                    startNewSectionFrom(currentSectionBegin, current);
                }
            }

            double strain = StrainValueAt(current);

            // Store the strain value for the object
            ObjectStrains.Add(strain);

            if (current.Index == 0)
            {
                currentSectionPeak = strain;
            }
            else if (strain > currentSectionPeak)
            {
                // Clear the queue
                strainPeakQueue.Clear();

                currentSectionPeak = strain;

                // End the current strain, and create a new strain starting at the current hitobject
                saveCurrentPeak(deltaTime);
                currentSectionBegin += deltaTime;
                currentSectionEnd = currentSectionBegin + MaxSectionLength;
                startNewSectionFrom(currentSectionBegin, current);
            }
            else
            {
                // If the current strain is smaller than the current peak
                // Empty the queue of smaller elements
                while (strainPeakQueue.Count > 0 && strainPeakQueue.Peek() < strain)
                {
                    strainPeakQueue.Dequeue();
                }

                // Add current strain to queue since it's less than the current peak
                strainPeakQueue.Enqueue(strain, current.StartTime);

                // Make a new strain with the same value and end time as the old strain
                // Yes, this is a little stupid
                saveCurrentPeak(deltaTime);
                currentSectionBegin += deltaTime;
            }
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
        private void saveCurrentPeak(double sectionLength)
        {
            strainPeaks.Add(new Strain(currentSectionPeak, sectionLength));
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
        public IEnumerable<Strain> GetCurrentStrainPeaks() => strainPeaks.Append(new Strain(currentSectionPeak, MaxSectionLength));

        /// <summary>
        /// Returns the calculated difficulty value representing all <see cref="DifficultyHitObject"/>s that have been processed up to this point.
        /// </summary>
        public override double DifficultyValue()
        {
            double difficulty = 0;
            double weight = 1;

            // Sections with 0 strain are excluded to avoid worst-case time complexity of the following sort (e.g. /b/2351871).
            // These sections will not contribute to the difficulty.
            var peaks = GetCurrentStrainPeaks().Where(p => p.Value > 0);

            List<Strain> strains = peaks.OrderByDescending(p => (p.Value, p.SectionLength)).ToList();

            // Time is measured in units of strains
            double time = 0;

            // Difficulty is the weighted sum of the highest strains from every section.
            // We're sorting from highest to lowest strain.
            for (int i = 0; i < strains.Count; i++)
            {
                difficulty += strains[i].Value * weight * strains[i].SectionLength / MaxSectionLength;
                time += strains[i].SectionLength / MaxSectionLength;
                weight = Math.Pow(DecayWeight, time);
            }

            return difficulty;
        }
    }
}
