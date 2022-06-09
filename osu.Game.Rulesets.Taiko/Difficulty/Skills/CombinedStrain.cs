using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Taiko.Difficulty.Skills
{
    public class CombinedStrain : Skill
    {
        private const double final_multiplier = 0.04625;
        private const double rhythm_skill_multiplier = 0.32 * final_multiplier;
        private const double colour_skill_multiplier = 0.37 * final_multiplier;
        private const double stamina_skill_multiplier = 0.37 * final_multiplier;

        private Rhythm rhythm;
        private Colour colour;
        private Stamina stamina;

        public double ColourDifficultyValue => colour.DifficultyValue() * colour_skill_multiplier;
        public double RhythmDifficultyValue => rhythm.DifficultyValue() * rhythm_skill_multiplier;
        public double StaminaDifficultyValue => stamina.DifficultyValue() * stamina_skill_multiplier;

        public CombinedStrain(Mod[] mods) : base(mods)
        {
            rhythm = new Rhythm(mods);
            colour = new Colour(mods);
            stamina = new Stamina(mods);
        }

        /// <summary>
        /// Returns the <i>p</i>-norm of an <i>n</i>-dimensional vector.
        /// </summary>
        /// <param name="p">The value of <i>p</i> to calculate the norm for.</param>
        /// <param name="values">The coefficients of the vector.</param>
        private double norm(double p, params double[] values) => Math.Pow(values.Sum(x => Math.Pow(x, p)), 1 / p);

        public override void Process(DifficultyHitObject current)
        {
            rhythm.Process(current);
            colour.Process(current);
            stamina.Process(current);
        }

        /// <summary>
        /// Returns the combined star rating of the beatmap, calculated using peak strains from all sections of the map.
        /// </summary>
        /// <remarks>
        /// For each section, the peak strains of all separate skills are combined into a single peak strain for the section.
        /// The resulting partial rating of the beatmap is a weighted sum of the combined peaks (higher peaks are weighted more).
        /// </remarks>
        public override double DifficultyValue()
        {
            List<double> peaks = new List<double>();

            var colourPeaks = colour.GetCurrentStrainPeaks().ToList();
            var rhythmPeaks = rhythm.GetCurrentStrainPeaks().ToList();
            var staminaPeaks = stamina.GetCurrentStrainPeaks().ToList();

            for (int i = 0; i < colourPeaks.Count; i++)
            {
                double colourPeak = colourPeaks[i] * colour_skill_multiplier;
                double rhythmPeak = rhythmPeaks[i] * rhythm_skill_multiplier;
                double staminaPeak = staminaPeaks[i] * stamina_skill_multiplier;

                double peak = norm(1.5, colourPeak, staminaPeak);
                peak = norm(2, peak, rhythmPeak);

                // Sections with 0 strain are excluded to avoid worst-case time complexity of the following sort (e.g. /b/2351871).
                // These sections will not contribute to the difficulty.
                if (peak > 0)
                    peaks.Add(peak);
            }

            double difficulty = 0;
            double weight = 1;

            foreach (double strain in peaks.OrderByDescending(d => d))
            {
                difficulty += strain * weight;
                weight *= 0.9;
            }

            return difficulty;
        }
    }
}