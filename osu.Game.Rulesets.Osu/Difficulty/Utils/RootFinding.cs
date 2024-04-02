// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

namespace osu.Game.Rulesets.Osu.Difficulty.Utils
{
    public static class RootFinding
    {
        /// <summary>
        /// Finds the root of a <paramref name="function"/> using the Chandrupatla method, expanding the bounds if the root is not located within.
        /// Expansion only occurs for the upward bound, as this function is optimized for functions of range [0, x),
        /// which is useful for finding skill level (skill can never be below 0).
        /// </summary>
        /// <param name="function">The function of which to find the root.</param>
        /// <param name="guessLowerBound">The lower bound of the function inputs.</param>
        /// <param name="guessUpperBound">The upper bound of the function inputs.</param>
        /// <param name="maxIterations">The maximum number of iterations before the function throws an error.</param>
        /// <param name="accuracy">The desired precision in which the root is returned.</param>
        /// <param name="expansionFactor">The multiplier on the upper bound when no root is found within the provided bounds.</param>
        public static double FindRootExpand(Func<double, double> function, double guessLowerBound, double guessUpperBound, int maxIterations = 25, double accuracy = 1e-6D, double expansionFactor = 2)
        {
            double a = guessLowerBound;
            double b = guessUpperBound;
            double fa = function(a);
            double fb = function(b);

            while (fa * fb > 0)
            {
                a = b;
                b *= expansionFactor;
                fa = function(a);
                fb = function(b);
            }

            double t = 0.5;

            for (int i = 0; i < maxIterations; i++)
            {
                double xt = a + t * (b - a);
                double ft = function(xt);

                double c;
                double fc;

                if (Math.Sign(ft) == Math.Sign(fa))
                {
                    c = a;
                    fc = fa;
                }
                else
                {
                    c = b;
                    b = a;
                    fc = fb;
                    fb = fa;
                }

                a = xt;
                fa = ft;

                double xm, fm;

                if (Math.Abs(fa) < Math.Abs(fb))
                {
                    xm = a;
                    fm = fa;
                }
                else
                {
                    xm = b;
                    fm = fb;
                }

                if (fm == 0)
                    return xm;

                double tol = 2 * accuracy * Math.Abs(xm) + 2 * accuracy;
                double tlim = tol / Math.Abs(b - c);

                if (tlim > 0.5)
                {
                    return xm;
                }

                double chi = (a - b) / (c - b);
                double phi = (fa - fb) / (fc - fb);
                bool iqi = phi * phi < chi && (1 - phi) * (1 - phi) < chi;

                if (iqi)
                    t = fa / (fb - fa) * fc / (fb - fc) + (c - a) / (b - a) * fa / (fc - fa) * fb / (fc - fb);
                else
                    t = 0.5;

                t = Math.Min(1 - tlim, Math.Max(tlim, t));
            }

            return 0;
        }
    }
}
