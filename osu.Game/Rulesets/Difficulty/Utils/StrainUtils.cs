// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;

namespace osu.Game.Rulesets.Difficulty.Utils
{
    public class StrainUtils
    {
        /// <summary>
        /// Calculates the number of strains weighted against the top strain.
        /// </summary>
        public static double CountTopWeightedStrains(IReadOnlyCollection<double> strains, double difficultyValue)
        {
            if (strains.Count == 0)
                return 0.0;

            double consistentTopStrain = difficultyValue / 10; // What would the top strain be if all strain values were identical

            if (consistentTopStrain == 0)
                return strains.Count;

            // Use a weighted sum of all strains. Constants are arbitrary and give nice values
            return strains.Sum(s => DifficultyCalculationUtils.Logistic(s / consistentTopStrain, 0.88, 10, 1.1));
        }
    }
}
