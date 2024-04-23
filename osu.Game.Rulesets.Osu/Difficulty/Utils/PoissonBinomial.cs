// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;

namespace osu.Game.Rulesets.Osu.Difficulty.Utils
{
    /// <summary>
    /// Approximation of the Poisson binomial distribution:
    /// https://en.wikipedia.org/wiki/Poisson_binomial_distribution
    /// </summary>
    /// <remarks>
    /// <para>
    /// For the approximation method, see "Refined Normal Approximation (RNA)" from:
    /// Hong, Y. (2013). On computing the distribution function for the Poisson binomial distribution. Computational Statistics and Data Analysis, Vol. 59, pp. 41-51.
    /// (https://www.researchgate.net/publication/257017356_On_computing_the_distribution_function_for_the_Poisson_binomial_distribution)
    /// </para>
    /// <para>
    /// This has been verified against a reference implementation provided by the authors in the R package "poibin",
    /// which can be viewed here:
    /// https://rdrr.io/cran/poibin/man/poibin-package.html
    /// </para>
    /// </remarks>
    public class PoissonBinomial
    {
        /// <summary>
        /// The expected value of the distribution.
        /// </summary>
        private readonly double mu;

        /// <summary>
        /// The standard deviation of the distribution.
        /// </summary>
        private readonly double sigma;

        /// <summary>
        /// The gamma factor from equation (11) in the cited paper, pre-divided by 6 to save on re-computation.
        /// </summary>
        private readonly double v;

        /// <summary>
        /// Creates a Poisson binomial distribution based on N trials with the provided difficulties, skill, and method for getting the miss probabilities.
        /// </summary>
        /// <param name="difficulties">The list of difficulties in the map.</param>
        /// <param name="skill">The skill level to get the miss probabilities with.</param>
        /// <param name="hitProbability">Converts difficulties and skill to miss probabilities.</param>
        public PoissonBinomial(IList<double> difficulties, double skill, Func<double, double, double> hitProbability)
        {
            double variance = 0;
            double gamma = 0;

            foreach (double d in difficulties)
            {
                double p = 1 - hitProbability(skill, d);

                mu += p;
                variance += p * (1 - p);
                gamma += p * (1 - p) * (1 - 2 * p);
            }

            sigma = Math.Sqrt(variance);

            v = gamma / (6 * Math.Pow(sigma, 3));
        }

        /// <summary>
        /// Creates a Poisson binomial distribution based on N trials with the provided bins of difficulties, skill, and method for getting the miss probabilities.
        /// </summary>
        /// <param name="bins">The bins of difficulties in the map.</param>
        /// <param name="skill">The skill level to get the miss probabilities with.</param>
        /// /// <param name="hitProbability">Converts difficulties and skill to miss probabilities.</param>
        public PoissonBinomial(Bin[] bins, double skill, Func<double, double, double> hitProbability)
        {
            double variance = 0;
            double gamma = 0;

            foreach (Bin bin in bins)
            {
                double p = 1 - hitProbability(skill, bin.Difficulty);

                mu += p * bin.Count;
                variance += p * (1 - p) * bin.Count;
                gamma += p * (1 - p) * (1 - 2 * p) * bin.Count;
            }

            sigma = Math.Sqrt(variance);

            v = gamma / (6 * Math.Pow(sigma, 3));
        }

        /// <summary>
        /// Computes the value of the cumulative distribution function for this Poisson binomial distribution.
        /// </summary>
        /// <param name="count">
        /// The argument of the CDF to sample the distribution for.
        /// In the discrete case (when it is a whole number), this corresponds to the number
        /// of successful Bernoulli trials to query the CDF for.
        /// </param>
        /// <returns>
        /// The value of the CDF at <paramref name="count"/>.
        /// In the discrete case this corresponds to the probability that at most <paramref name="count"/>
        /// Bernoulli trials ended in a success.
        /// </returns>
        // ReSharper disable once InconsistentNaming
        public double CDF(double count)
        {
            if (sigma == 0)
                return 1;

            double k = (count + 0.5 - mu) / sigma;

            // see equation (14) of the cited paper
            double result = SpecialFunctions.NormalCdf(0, 1, k) + v * (1 - k * k) * SpecialFunctions.NormalPdf(0, 1, k);

            return Math.Clamp(result, 0, 1);
        }
    }
}
