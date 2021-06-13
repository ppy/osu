// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.Distributions;

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
    internal class PoissonBinomial
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
        /// Creates a Poisson binomial distribution based on N trials with the provided <paramref name="probabilities"/>.
        /// </summary>
        /// <param name="probabilities">The list of probabilities of success of each Bernoulli trial from the given lot.</param>
        public PoissonBinomial(IList<double> probabilities)
        {
            mu = probabilities.Sum();

            double variance = 0;
            double gamma = 0;

            foreach (double p in probabilities)
            {
                variance += p * (1 - p);
                gamma += p * (1 - p) * (1 - 2 * p);
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
            double result = Normal.CDF(0, 1, k) + v * (1 - k * k) * Normal.PDF(0, 1, k);

            if (result < 0) return 0;
            if (result > 1) return 1;

            return result;
        }
    }
}
