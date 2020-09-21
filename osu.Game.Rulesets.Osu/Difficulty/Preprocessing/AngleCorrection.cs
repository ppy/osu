// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using MathNet.Numerics;
using osu.Game.Rulesets.Osu.Difficulty.Interp;

namespace osu.Game.Rulesets.Osu.Difficulty.Preprocessing
{
    public class AngleCorrection
    {
        public double Evaluate(double distance1, double x, double y)
        {
            double angle = Math.Abs(Math.Atan2(y, x));
            double distance2 = Math.Sqrt(x * x + y * y);

            return SpecialFunctions.Logistic(interp.Evaluate(distance1, distance2, angle));
        }

        /// <summary>
        /// Calculate a correction based on angle - specify a grid of values and interpolate between them
        /// </summary>
        /// <param name="d1">distance between previous note and target note</param>
        /// <param name="d2">other distance (either 2 previous to prev, or target note to next)</param>
        /// <param name="angles">angle between notes</param>
        /// <param name="values">Logistic(values[i,j,k]) is the correction value for  d1[i],d2[j],angle[k]</param>
        public AngleCorrection(double[] d1, double[] d2, double[] angles, double[,,] values)
        {
            interp = new TricubicInterp(d1, d2, angles, values);
        }

        public AngleCorrection(MultiL2NormCorrection correction, double[] d1, double[] d2, double[] angle, string print_name = "", bool print = false)
        {

            double[][][] values = new double[d1.Length][][];
            for (int i = 0; i < d1.Length; ++i)
            {
                values[i] = new double[d2.Length][];
                for (int j = 0; j < d2.Length; ++j)
                {
                    values[i][j] = new double[angle.Length];
                    for (int k = 0; k < angle.Length; ++k)
                    {
                        double x = d2[j] * Math.Cos(angle[k]);
                        double y = d2[j] * Math.Abs(Math.Sin(angle[k]));

                        double distance = d1[i];
                        values[i][j][k] = Math.Clamp(SpecialFunctions.Logit(Math.Clamp(correction.Evaluate(distance, x, y), -0.99999, 0.999999)), -100, 100);
                    }
                }
            }
            if (print_name.Length != 0 && print)
            {
                string indent = "    ";
                string indent2 = indent + indent;
                string indent3 = indent2 + indent;
                string indent4 = indent3 + indent;
                Console.WriteLine($"{indent2}public static readonly AngleCorrection {print_name} = new AngleCorrection(");

                Console.WriteLine($"{indent3}d1: new double[]{{ {string.Join(", ", d1)} }},");
                Console.WriteLine($"{indent3}d2: new double[]{{ {string.Join(", ", d2)} }},");
                Console.WriteLine($"{indent3}anges: new double[]{{ {string.Join(", ", angle)} }},");
                Console.WriteLine($"{indent3}values: new double[,,]{{");

                foreach (var xArray in values)
                {
                    Console.WriteLine($"{indent4}{{");

                    foreach (var yArray in xArray)
                    {
                        Console.WriteLine($"{indent4}{indent}{{ {string.Join(", ", yArray.Select(x => x.ToString("F2")))} }},");
                    }
                    Console.WriteLine($"{indent4}}},");
                }
                Console.WriteLine($"{indent3}}});\n");
            }

            interp = new TricubicInterp(d1, d2, angle, values);
        }

        private TricubicInterp interp;


        private static readonly double[] angles = { 0, 0.25 * Math.PI, 0.5 * Math.PI, 0.75 * Math.PI, Math.PI };

        public static readonly AngleCorrection FLOW_0 = new AngleCorrection(
            d1: new double[] { 0, 1, 1.5, 2, 3 },
            d2: new double[] { 0, 1, 1.5, 2, 3 },
            angles: angles,
            values: new double[,,]{
                {
                    { 0.50, 0.50, 0.50, 0.50, 0.50 },
                    { 5.47, 5.47, 5.47, 5.47, 5.47 },
                    { 10.13, 10.13, 10.13, 10.13, 10.13 },
                    { 13.82, 13.82, 13.82, 13.82, 13.82 },
                    { 13.82, 13.82, 13.82, 13.82, 13.82 },
                },
                {
                    { -1.80, -1.80, -1.80, -1.80, -1.80 },
                    { 0.63, 0.22, -0.89, -1.37, -1.61 },
                    { 2.26, 1.79, 0.68, -0.13, -0.44 },
                    { 4.00, 3.51, 2.38, 1.44, 1.07 },
                    { 7.60, 7.11, 5.97, 4.92, 4.51 },
                },
                {
                    { -0.81, -0.81, -0.81, -0.81, -0.81 },
                    { 1.59, 1.11, -0.20, -1.16, -1.57 },
                    { 3.07, 2.49, 1.00, -0.49, -1.08 },
                    { 4.62, 3.99, 2.38, 0.65, -0.09 },
                    { 7.83, 7.14, 5.40, 3.52, 2.67 },
                },
                {
                    { 1.68, 1.68, 1.68, 1.68, 1.68 },
                    { 4.02, 3.54, 2.29, 1.05, 0.63 },
                    { 5.38, 4.75, 3.04, 1.20, 0.68 },
                    { 6.79, 6.07, 4.05, 1.82, 1.16 },
                    { 9.71, 8.87, 6.59, 4.14, 3.20 },
                },
                {
                    { 4.71, 4.71, 4.71, 4.71, 4.71 },
                    { 6.82, 6.31, 5.03, 3.83, 3.41 },
                    { 8.04, 7.34, 5.45, 3.74, 3.24 },
                    { 9.33, 8.49, 6.12, 3.99, 3.47 },
                    { 12.04, 11.04, 8.25, 5.75, 5.07 },
                },
        });

        public static readonly AngleCorrection SNAP_0 = new AngleCorrection(
            d1: new double[] { 0, 2, 4, 6, 10 },
            d2: new double[] { 0, 2, 4, 6, 10 },
            angles: angles,
            values: new double[,,]{
                {
                    { 0.35, 0.35, 0.35, 0.35, 0.35 },
                    { 1.01, 1.15, 1.52, 1.91, 2.07 },
                    { 2.99, 3.16, 3.56, 3.98, 4.15 },
                    { 5.13, 5.29, 5.71, 6.13, 6.31 },
                    { 9.47, 9.65, 10.07, 10.49, 10.67 },
                },
                {
                    { -0.24, -0.24, -0.24, -0.24, -0.24 },
                    { -2.46, -1.78, -0.31, 0.69, 0.84 },
                    { -1.37, -0.61, 0.65, 0.95, 0.96 },
                    { 0.53, 0.77, 0.95, 0.97, 0.97 },
                    { 0.97, 0.97, 0.97, 0.97, 0.97 },
                },
                {
                    { -1.27, -1.27, -1.27, -1.27, -1.27 },
                    { -2.88, -2.31, -1.06, 0.01, 0.37 },
                    { -3.99, -2.56, -0.37, 0.96, 1.17 },
                    { -3.49, -1.63, 0.65, 1.31, 1.36 },
                    { 0.14, 0.96, 1.36, 1.38, 1.39 },
                },
                {
                    { -1.05, -1.05, -1.05, -1.05, -1.05 },
                    { -1.78, -1.56, -1.02, -0.42, -0.13 },
                    { -2.25, -1.77, -0.88, 0.26, 0.78 },
                    { -2.09, -1.57, -0.53, 1.00, 1.60 },
                    { -0.58, -0.19, 1.03, 2.70, 3.37 },
                },
                {
                    { -1.05, -1.05, -1.05, -1.05, -1.05 },
                    { -1.78, -1.56, -1.02, -0.42, -0.13 },
                    { -2.25, -1.77, -0.88, 0.26, 0.78 },
                    { -2.09, -1.57, -0.53, 1.00, 1.60 },
                    { -0.58, -0.19, 1.03, 2.70, 3.37 },
                },
            });

        public static readonly AngleCorrection FLOW_3 = new AngleCorrection(
            d1: new double[] { 0, 1, 1.5, 2, 3 },
            d2: new double[] { 0, 1, 1.5, 2, 3 },
            angles: angles,
            values: new double[,,]{
                {
                    { -4.00, -4.00, -4.00, -4.00, -4.00 },
                    { -0.50, -0.50, -0.50, -0.50, -0.50 },
                    { 1.25, 1.25, 1.25, 1.25, 1.25 },
                    { 3.00, 3.00, 3.00, 3.00, 3.00 },
                    { 6.50, 6.50, 6.50, 6.50, 6.50 },
                },
                {
                    { -3.26, -3.26, -3.26, -3.26, -3.26 },
                    { -2.62, -2.04, -1.31, -0.55, -0.29 },
                    { -1.50, -0.80, 0.11, 0.98, 1.30 },
                    { 0.08, 0.67, 1.66, 2.56, 2.91 },
                    { 3.30, 3.81, 4.84, 5.79, 6.16 },
                },
                {
                    { -2.72, -2.72, -2.72, -2.72, -2.72 },
                    { -1.40, -1.07, -0.50, 0.12, 0.36 },
                    { -0.58, 0.07, 0.77, 1.67, 1.99 },
                    { 0.87, 1.45, 2.25, 3.29, 3.66 },
                    { 4.18, 4.64, 5.57, 6.63, 7.05 },
                },
                {
                    { -2.42, -2.42, -2.42, -2.42, -2.42 },
                    { -0.35, -0.19, 0.18, 0.62, 0.81 },
                    { 0.80, 1.08, 1.56, 2.23, 2.50 },
                    { 2.02, 2.48, 3.00, 3.90, 4.23 },
                    { 5.42, 5.68, 6.25, 7.36, 7.76 },
                },
                {
                    { -2.50, -2.50, -2.50, -2.50, -2.50 },
                    { 1.00, 1.00, 1.00, 1.00, 1.00 },
                    { 2.75, 2.75, 2.75, 2.75, 2.75 },
                    { 4.50, 4.50, 4.50, 4.50, 4.50 },
                    { 8.00, 8.00, 8.00, 8.00, 8.00 },
                },
            });

        public static readonly AngleCorrection SNAP_3 = new AngleCorrection(
            d1: new double[] { 0, 2, 4, 6, 10 },
            d2: new double[] { 0, 2, 4, 6, 10 },
            angles: angles,
            values: new double[,,]{
                {
                    { 0.49, 0.49, 0.49, 0.49, 0.49 },
                    { 2.42, 2.11, 1.29, 0.37, -0.01 },
                    { 4.66, 4.27, 3.25, 2.14, 1.66 },
                    { 6.99, 6.57, 5.50, 4.37, 3.88 },
                    { 11.72, 11.28, 10.18, 9.04, 8.56 },
                },
                {
                    { -0.75, -0.75, -0.75, -0.75, -0.75 },
                    { 1.80, 1.40, 0.45, -0.52, -0.89 },
                    { 3.97, 3.48, 2.27, 1.02, 0.48 },
                    { 6.13, 5.61, 4.35, 3.04, 2.49 },
                    { 10.48, 9.95, 8.65, 7.32, 6.77 },
                },
                {
                    { -2.29, -2.29, -2.29, -2.29, -2.29 },
                    { 1.47, 0.72, -0.88, -2.59, -3.26 },
                    { 4.43, 3.47, 1.19, -1.43, -3.03 },
                    { 7.20, 6.18, 3.68, 0.87, -0.56 },
                    { 12.74, 11.67, 9.01, 6.14, 4.83 },
                },
                {
                    { -2.31, -2.31, -2.31, -2.31, -2.31 },
                    { 0.64, 0.08, -1.08, -2.23, -2.70 },
                    { 3.15, 2.22, 0.33, -1.80, -2.69 },
                    { 5.17, 4.21, 1.99, -0.57, -2.27 },
                    { 9.13, 8.13, 5.66, 2.89, 1.46 },
                },
                {
                    { -2.31, -2.31, -2.31, -2.31, -2.31 },
                    { 0.64, 0.08, -1.08, -2.23, -2.70 },
                    { 3.15, 2.22, 0.33, -1.80, -2.69 },
                    { 5.17, 4.21, 1.99, -0.57, -2.27 },
                    { 9.13, 8.13, 5.66, 2.89, 1.46 },
                },
            });

    }
}
