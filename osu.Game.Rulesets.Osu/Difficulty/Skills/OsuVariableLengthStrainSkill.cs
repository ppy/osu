// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Mods;
using System.Linq;
using osu.Framework.Utils;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills
{
    public abstract class OsuVariableLengthStrainSkill : VariableLengthStrainSkill
    {
        /// <summary>
        /// The number of sections with the highest strains, which the peak strain reductions will apply to.
        /// This is done in order to decrease their impact on the overall difficulty of the map for this skill.
        /// </summary>
        protected virtual int ReducedSectionCount => 10;

        /// <summary>
        /// The baseline multiplier applied to the section with the biggest strain.
        /// </summary>
        protected virtual double ReducedStrainBaseline => 0.727;

        protected OsuVariableLengthStrainSkill(Mod[] mods)
            : base(mods)
        {
        }

        public override double DifficultyValue()
        {
            double difficulty = 0;
            double decayWeightIntegral = (DecayWeight - 1) / Math.Log(DecayWeight) * (1.0 / (1 - DecayWeight));

            // Sections with 0 strain are excluded to avoid worst-case time complexity of the following sort (e.g. /b/2351871).
            // These sections will not contribute to the difficulty.
            var peaks = GetCurrentStrainPeaks().Where(p => p.Value > 0);

            List<StrainPeak> strains = peaks.OrderByDescending(p => p.Value).ToList();

            double time = 0; // Time is measured in units of strains
            const int chunk_size = 20;
            int strainsToRemove = 0;

            // We are reducing the highest strains first to account for extreme difficulty spikes
            // Split the strain into 20ms chunks to try to mitigate inconsistencies caused by reducing strains
            while (strains.Count > strainsToRemove && time / MaxSectionLength < ReducedSectionCount)
            {
                StrainPeak strain = strains[strainsToRemove];
                double addedTime = 0;

                while (addedTime < strain.SectionLength)
                {
                    double scale = Math.Log10(Interpolation.Lerp(1, 10, Math.Clamp((time + addedTime) / MaxSectionLength / ReducedSectionCount, 0, 1)));

                    strains.Add(new StrainPeak(strain.Value * Interpolation.Lerp(ReducedStrainBaseline, 1.0, scale), Math.Min(chunk_size, strain.SectionLength - addedTime)));
                    addedTime += chunk_size;
                }

                time += strain.SectionLength;
                strainsToRemove++;
            }

            strains.RemoveRange(0, strainsToRemove);

            // Reset time for summing
            time = WeightedSumTimeOffset;

            // Difficulty is a continuous weighted sum of the sorted strains
            foreach (StrainPeak strain in strains.OrderByDescending(s => s.Value))
            {
                double weight = Math.Pow(DecayWeight, time) * (decayWeightIntegral - decayWeightIntegral * Math.Pow(DecayWeight, strain.SectionLength / MaxSectionLength));
                difficulty += strain.Value * weight;
                time += strain.SectionLength / MaxSectionLength;
            }

            return difficulty;
        }

        public static double DifficultyToPerformance(double difficulty) => Math.Pow(5.0 * Math.Max(1.0, difficulty / 0.0675) - 4.0, 3.0) / 100000.0;
    }
}
