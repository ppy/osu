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
        protected override double RawDifficultyMultiplier => 1.058;

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
            double weight = 1;
            double decayWeightIntegral = (DecayWeight - 1) / Math.Log(DecayWeight) * (1.0 / (1 - DecayWeight));

            // Sections with 0 strain are excluded to avoid worst-case time complexity of the following sort (e.g. /b/2351871).
            // These sections will not contribute to the difficulty.
            var peaks = GetCurrentStrainPeaks().Where(p => p.Value > 0);

            List<StrainPeak> strains = peaks.OrderByDescending(p => p.Value).ToList();

            // Create list of strains to nerf
            List<StrainPeak> strainsToReduce = new List<StrainPeak>();
            int indexToRemove = 0;
            const int chunkSize = 20;

            double time = 0; // Time is measured in units of strains

            // We are reducing the highest strains first to account for extreme difficulty spikes
            // Split the strain into 20ms chunks to try to mitigate inconsistencies caused by reducing strains
            foreach (StrainPeak strain in strains)
            {
                double addedTime = 0;

                while (addedTime < strain.SectionLength)
                {
                    double scale = Math.Log10(Interpolation.Lerp(1, 10, Math.Clamp((time + addedTime) / MaxSectionLength / ReducedSectionCount, 0, 1)));

                    strainsToReduce.Add(new StrainPeak(strain.Value * Interpolation.Lerp(ReducedStrainBaseline, 1.0, scale), Math.Min(chunkSize, strain.SectionLength - addedTime)));
                    addedTime += chunkSize;
                }

                time += strain.SectionLength;
                indexToRemove++;
                if (time / MaxSectionLength > ReducedSectionCount) break;
            }

            strains.RemoveRange(0, indexToRemove);

            time = 0;

            strains = strains.Concat(strainsToReduce).OrderByDescending(s => s.Value).ToList();
            time = 0;

            // Difficulty is a continuous weighted sum of the sorted strains
            for (int i = 0; i < strains.Count; i++)
            {
                weight = Math.Pow(DecayWeight, time) * (decayWeightIntegral - decayWeightIntegral * Math.Pow(DecayWeight, strains[i].SectionLength / MaxSectionLength)); // f(a,b)=Integrate[Power[0.9,x],{x,a,a+b}]
                difficulty += strains[i].Value * weight;
                time += strains[i].SectionLength / MaxSectionLength;
            }

            return difficulty * RawDifficultyMultiplier;
        }

        public static double DifficultyToPerformance(double difficulty) => Math.Pow(5.0 * Math.Max(1.0, difficulty / 0.0675) - 4.0, 3.0) / 100000.0;
    }
}
