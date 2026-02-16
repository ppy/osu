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
        /// The amount of time with the highest strains, which the peak strain reductions will apply to.
        /// This is done in order to decrease their impact on the overall difficulty of the map for this skill.
        /// </summary>
        protected virtual int ReducedSectionTime => 4000;

        /// <summary>
        /// The baseline multiplier applied to the section with the biggest strain.
        /// </summary>
        protected virtual double ReducedStrainBaseline => 0.727;

        protected virtual double DifficultyMultiplier => 1.0588;

        protected OsuVariableLengthStrainSkill(Mod[] mods)
            : base(mods)
        {
        }

        public override double DifficultyValue()
        {
            double difficulty = 0;

            // Sections with 0 strain are excluded to avoid worst-case time complexity of the following sort (e.g. /b/2351871).
            // These sections will not contribute to the difficulty.
            var peaks = GetCurrentStrainPeaks().Where(p => p.Value > 0);

            List<StrainPeak> strains = peaks.OrderByDescending(p => p.Value).ToList();

            double time = 0; // Time is measured in units of strains
            const int chunk_size = 20;
            int strainsToRemove = 0;

            // We are reducing the highest strains first to account for extreme difficulty spikes
            // Split the strain into 20ms chunks to try to mitigate inconsistencies caused by reducing strains
            while (strains.Count > strainsToRemove && time < ReducedSectionTime)
            {
                StrainPeak strain = strains[strainsToRemove];
                double addedTime = 0;

                while (addedTime < strain.SectionLength)
                {
                    double scale = Math.Log10(Interpolation.Lerp(1, 10, Math.Clamp((time + addedTime) / ReducedSectionTime, 0, 1)));

                    strains.Add(new StrainPeak(strain.Value * Interpolation.Lerp(ReducedStrainBaseline, 1.0, scale), Math.Min(chunk_size, strain.SectionLength - addedTime)));
                    addedTime += chunk_size;
                }

                time += strain.SectionLength;
                strainsToRemove++;
            }

            strains.RemoveRange(0, strainsToRemove);

            // Reset time for summing
            time = 0;

            // Difficulty is a continuous weighted sum of the sorted strains
            foreach (StrainPeak strain in strains.OrderByDescending(s => s.Value))
            {
                /* Weighting function:
                        a+b
                        ∫ 0.9^x dx
                        a
                    where a = startTime and b = strain.SectionLength */
                double weight = Math.Pow(DecayWeight, time) * (DecayWeightIntegral - DecayWeightIntegral * Math.Pow(DecayWeight, strain.SectionLength / MaxSectionLength));
                difficulty += strain.Value * weight;
                time += strain.SectionLength / MaxSectionLength;
            }

            return difficulty * DifficultyMultiplier;
        }

        public static double DifficultyToPerformance(double difficulty) => 4.0 * Math.Pow(difficulty, 3.0);
    }
}
