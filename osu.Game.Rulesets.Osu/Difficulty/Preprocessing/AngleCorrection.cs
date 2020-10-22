// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Game.Rulesets.Osu.Difficulty.Interp;

namespace osu.Game.Rulesets.Osu.Difficulty.Preprocessing
{
    public class AngleCorrection
    {
        public double Evaluate(double distance1, double x, double y)
        {
            (x, y) = Transform.Apply(posTransform, distance1, x, y);

            double angle = Math.Abs(Math.Atan2(y, x));
            double distance2 = Math.Sqrt(x * x + y * y);
            double maxVal = max?.Evaluate(distance1) ?? 1;
            double minVal = min?.Evaluate(distance1) ?? 0;

            double scale = maxVal - minVal;
            return minVal + scale * Math.Clamp(interp.Evaluate(distance1, distance2, angle),0,1);
        }

        /// <summary>
        /// Calculate a correction based on angle - specify a grid of values and interpolate between them
        /// </summary>
        /// <param name="d1">distance between previous note and target note</param>
        /// <param name="d2">other distance (either 2 previous to prev, or target note to next)</param>
        /// <param name="angles">angle between notes</param>
        /// <param name="values">values[i,j,k] defines the correction value for  d1[i],d2[j],angle[k]. Values should be between 0 and 1. 0 is mapped to the minimum value, 1 to max</param>
        /// <param name="min">spline mapping d1 to minimum value (default 0)</param>
        /// <param name="max">spline mapping d1 to max value (default 1)</param>
        /// <param name="posTransform">transform for position of "other" note</param>
        public AngleCorrection(double[] d1, double[] d2, double[] angles, double[,,] values, CubicInterp min=null, CubicInterp max=null,
            Transform posTransform=null)
        {
            this.posTransform = posTransform;
            this.min = min;
            this.max = max;
            interp = new TricubicInterp(d1, d2, angles, values, dzLower: 0, dzUpper: 0);
        }

        /// <summary>
        /// Create a new AngleCorrection by resampling another correction
        /// </summary>
        public AngleCorrection(MultiL2NormCorrection correction, double[] d1, double[] d2, double[] angle, string print_name = "", bool print = true,
            CubicInterp min=null, CubicInterp max=null, Transform posTransform=null)
        {
            this.posTransform = posTransform;
            this.min = min;
            this.max = max;
            double[][][] values = new double[d1.Length][][];
            for (int i = 0; i < d1.Length; ++i)
            {
                values[i] = new double[d2.Length][];
                for (int j = 0; j < d2.Length; ++j)
                {
                    values[i][j] = new double[angle.Length];
                    for (int k = 0; k < angle.Length; ++k)
                    {
                        double distance = d1[i];
                        (double x, double y) = Transform.Inverse(posTransform, distance, d2[j] * Math.Cos(angle[k]), d2[j] * Math.Abs(Math.Sin(angle[k])));
                        double maxVal = max?.Evaluate(distance) ?? 1;
                        double minVal = min?.Evaluate(distance) ?? 0;
                        double scale = maxVal - minVal;

                        double corr = correction.Evaluate(distance, x, y);

                        values[i][j][k] = Math.Clamp((corr - minVal)/scale,0,1);
                    }
                }
            }
            if (print_name.Length != 0 && print)
            {
                string indent = "    ";
                string indent2 = indent + indent;
                string indent3 = indent2 + indent;
                string indent4 = indent3 + indent;
                string indent5 = indent4 + indent;

                Console.WriteLine($"{indent2}public static readonly AngleCorrection {print_name} = new AngleCorrection(");

                Console.WriteLine($"{indent3}d1: new double[]{{ {string.Join(", ", d1)} }},");
                Console.WriteLine($"{indent3}d2: new double[]{{ {string.Join(", ", d2)} }},");
                Console.WriteLine($"{indent3}angles: angles,");
                if (min != null) Console.WriteLine($"{indent3}min: min,");
                if (max != null) Console.WriteLine($"{indent3}max: max,");
                if (posTransform != null) Console.WriteLine($"{indent3}posTransform: posTransform,");
                Console.WriteLine($"{indent3}values: new double[,,]{{");

                for (int i=0; i<d1.Length; ++i)
                {
                    Console.WriteLine($"{indent4}{{ // d1={d1[i]}");
                    Console.WriteLine($"{indent5}// 0,   45,   90,   135,   180 degrees ");
                    for (int j = 0; j < d2.Length; ++j)
                    {
                        Console.WriteLine($"{indent4}{indent}{{ {string.Join(", ", values[i][j].Select(x => x.ToString("F2")))} }}, // d2={d2[j]}");
                    }
                    Console.WriteLine($"{indent4}}},");
                }
                Console.WriteLine($"{indent3}}});\n");
            }

            interp = new TricubicInterp(d1, d2, angle, values, dzLower: 0, dzUpper: 0);
        }


        private TricubicInterp interp;
        private CubicInterp min, max;
        private Transform posTransform;


        private const double angle = Math.PI / 4.0;
        private static readonly double[] angles = { 0, angle, 2 * angle, 3 * angle, 4 * angle };
        private static readonly Transform snap_scale_transform = new Transform { Scale = d => Math.Clamp(d, 2, 5) };
        private static readonly CubicInterp snap_0_max = new CubicInterp(new double[] { 0, 1.5, 2.5, 4, 6, 6.01 }, new double[] { 1, 0.85, 0.6, 0.8, 1, 1 });





        /*
        // code to resample original corrections
        private static readonly double[] flow_0_distances = { 0.2, 0.6, 1, 1.3, 1.7, 2.1 };
        private static readonly double[] flowdistances2 = { 0.1, 0.6, 1, 1.3, 1.8, 3.0 };


        private static readonly double[] snapdistances = { 0.6, 1.5, 2.4, 3.5, 5, 6.5, 9 };
        private static readonly double[] snapdistances2 = { 0, 0.5, 1, 1.5, 2.5 };

        private static readonly double[] flow_3_distances = { 0.2, 0.6, 1, 1.5, 2.8 };


        public static readonly AngleCorrection FLOW_0 = new AngleCorrection(MultiL2NormCorrection.FLOW_0_OLD, flow_0_distances, flowdistances2, angles, "FLOW_0");
        //public static readonly MultiL2NormCorrection FLOW_0 = MultiL2NormCorrection.FLOW_0_OLD;

        public static readonly AngleCorrection FLOW_3 = new AngleCorrection(MultiL2NormCorrection.FLOW_3_OLD, flow_3_distances, flowdistances2, angles, "FLOW_3");
        //public static readonly MultiL2NormCorrection FLOW_3 = MultiL2NormCorrection.FLOW_3_OLD;

        public static readonly AngleCorrection SNAP_0 = new AngleCorrection(MultiL2NormCorrection.SNAP_0_OLD, snapdistances, snapdistances2, angles, "SNAP_0", max: snap_0_max, posTransform: snap_scale_transform);
        //public static readonly MultiL2NormCorrection SNAP_0 = MultiL2NormCorrection.SNAP_0_OLD;

        public static readonly AngleCorrection SNAP_3 = new AngleCorrection(MultiL2NormCorrection.SNAP_3_OLD, snapdistances, snapdistances2, angles, "SNAP_3", posTransform: snap_scale_transform);
        //public static readonly MultiL2NormCorrection SNAP_3 = MultiL2NormCorrection.SNAP_3_OLD;
        */


        public static readonly AngleCorrection FLOW_0 = new AngleCorrection(
            d1: new double[] { 0.2, 0.6, 1, 1.3, 1.7, 2.1 },
            d2: new double[] { 0.1, 0.6, 1, 1.3, 1.8, 3 },
            angles: angles,
            values: new double[,,]{
                { // d1=0.2
                    // 0,   45,   90,   135,   180 degrees
                    { 0.45, 0.44, 0.42, 0.39, 0.39 }, // d2=0.1
                    { 0.89, 0.87, 0.80, 0.72, 0.67 }, // d2=0.6
                    { 0.99, 0.99, 0.98, 0.97, 0.96 }, // d2=1
                    { 1.00, 1.00, 1.00, 1.00, 1.00 }, // d2=1.3
                    { 1.00, 1.00, 1.00, 1.00, 1.00 }, // d2=1.8
                    { 1.00, 1.00, 1.00, 1.00, 1.00 }, // d2=3
                },
                { // d1=0.6
                    // 0,   45,   90,   135,   180 degrees
                    { 0.27, 0.26, 0.23, 0.20, 0.19 }, // d2=0.1
                    { 0.75, 0.68, 0.44, 0.26, 0.20 }, // d2=0.6
                    { 0.97, 0.94, 0.83, 0.59, 0.46 }, // d2=1
                    { 0.99, 0.99, 0.96, 0.86, 0.77 }, // d2=1.3
                    { 1.00, 1.00, 1.00, 0.99, 0.98 }, // d2=1.8
                    { 1.00, 1.00, 1.00, 1.00, 1.00 }, // d2=3
                },
                { // d1=1
                    // 0,   45,   90,   135,   180 degrees
                    { 0.16, 0.16, 0.14, 0.13, 0.13 }, // d2=0.1
                    { 0.37, 0.31, 0.19, 0.13, 0.11 }, // d2=0.6
                    { 0.65, 0.55, 0.29, 0.20, 0.17 }, // d2=1
                    { 0.83, 0.76, 0.51, 0.34, 0.28 }, // d2=1.3
                    { 0.96, 0.94, 0.84, 0.69, 0.61 }, // d2=1.8
                    { 1.00, 1.00, 1.00, 0.99, 0.99 }, // d2=3
                },
                { // d1=1.3
                    // 0,   45,   90,   135,   180 degrees
                    { 0.29, 0.28, 0.26, 0.23, 0.23 }, // d2=0.1
                    { 0.56, 0.48, 0.31, 0.19, 0.16 }, // d2=0.6
                    { 0.80, 0.71, 0.41, 0.24, 0.18 }, // d2=1
                    { 0.91, 0.85, 0.61, 0.34, 0.25 }, // d2=1.3
                    { 0.98, 0.96, 0.87, 0.63, 0.49 }, // d2=1.8
                    { 1.00, 1.00, 1.00, 0.99, 0.97 }, // d2=3
                },
                { // d1=1.7
                    // 0,   45,   90,   135,   180 degrees
                    { 0.39, 0.38, 0.35, 0.32, 0.31 }, // d2=0.1
                    { 0.66, 0.59, 0.39, 0.24, 0.20 }, // d2=0.6
                    { 0.85, 0.78, 0.47, 0.24, 0.17 }, // d2=1
                    { 0.93, 0.88, 0.62, 0.27, 0.18 }, // d2=1.3
                    { 0.98, 0.97, 0.84, 0.45, 0.27 }, // d2=1.8
                    { 1.00, 1.00, 0.99, 0.94, 0.85 }, // d2=3
                },
                { // d1=2.1
                    // 0,   45,   90,   135,   180 degrees
                    { 0.94, 0.94, 0.93, 0.92, 0.92 }, // d2=0.1
                    { 0.98, 0.97, 0.94, 0.89, 0.86 }, // d2=0.6
                    { 0.99, 0.99, 0.96, 0.87, 0.82 }, // d2=1
                    { 1.00, 0.99, 0.97, 0.87, 0.81 }, // d2=1.3
                    { 1.00, 1.00, 0.99, 0.90, 0.84 }, // d2=1.8
                    { 1.00, 1.00, 1.00, 0.99, 0.98 }, // d2=3
                },
    });

        public static readonly AngleCorrection FLOW_3 = new AngleCorrection(
            d1: new double[] { 0.2, 0.6, 1, 1.5, 2.8 },
            d2: new double[] { 0.1, 0.6, 1, 1.3, 1.8, 3 },
            angles: angles,
            values: new double[,,]{
                { // d1=0.2
                    // 0,   45,   90,   135,   180 degrees
                    { 0.02, 0.02, 0.02, 0.03, 0.03 }, // d2=0.1
                    { 0.07, 0.08, 0.11, 0.13, 0.14 }, // d2=0.6
                    { 0.24, 0.27, 0.32, 0.37, 0.39 }, // d2=1
                    { 0.47, 0.50, 0.57, 0.62, 0.64 }, // d2=1.3
                    { 0.84, 0.85, 0.88, 0.90, 0.91 }, // d2=1.8
                    { 1.00, 1.00, 1.00, 1.00, 1.00 }, // d2=3
                },
                { // d1=0.6
                    // 0,   45,   90,   135,   180 degrees
                    { 0.03, 0.03, 0.03, 0.03, 0.04 }, // d2=0.1
                    { 0.04, 0.06, 0.09, 0.13, 0.15 }, // d2=0.6
                    { 0.09, 0.15, 0.25, 0.36, 0.41 }, // d2=1
                    { 0.22, 0.30, 0.46, 0.60, 0.65 }, // d2=1.3
                    { 0.60, 0.68, 0.81, 0.89, 0.91 }, // d2=1.8
                    { 0.99, 0.99, 1.00, 1.00, 1.00 }, // d2=3
                },
                { // d1=1
                    // 0,   45,   90,   135,   180 degrees
                    { 0.04, 0.04, 0.04, 0.05, 0.05 }, // d2=0.1
                    { 0.05, 0.06, 0.10, 0.15, 0.18 }, // d2=0.6
                    { 0.07, 0.12, 0.21, 0.37, 0.43 }, // d2=1
                    { 0.11, 0.21, 0.38, 0.59, 0.66 }, // d2=1.3
                    { 0.36, 0.52, 0.74, 0.87, 0.91 }, // d2=1.8
                    { 0.96, 0.98, 0.99, 1.00, 1.00 }, // d2=3
                },
                { // d1=1.5
                    // 0,   45,   90,   135,   180 degrees
                    { 0.07, 0.07, 0.07, 0.08, 0.08 }, // d2=0.1
                    { 0.12, 0.14, 0.19, 0.26, 0.29 }, // d2=0.6
                    { 0.20, 0.25, 0.38, 0.53, 0.59 }, // d2=1
                    { 0.29, 0.40, 0.56, 0.74, 0.79 }, // d2=1.3
                    { 0.56, 0.71, 0.84, 0.93, 0.95 }, // d2=1.8
                    { 0.98, 0.99, 1.00, 1.00, 1.00 }, // d2=3
                },
                { // d1=2.8
                    // 0,   45,   90,   135,   180 degrees
                    { 0.10, 0.10, 0.10, 0.10, 0.11 }, // d2=0.1
                    { 0.36, 0.37, 0.38, 0.39, 0.40 }, // d2=0.6
                    { 0.67, 0.68, 0.70, 0.72, 0.72 }, // d2=1
                    { 0.85, 0.85, 0.86, 0.88, 0.88 }, // d2=1.3
                    { 0.97, 0.97, 0.97, 0.98, 0.98 }, // d2=1.8
                    { 1.00, 1.00, 1.00, 1.00, 1.00 }, // d2=3
                },
            });

        public static readonly AngleCorrection SNAP_0 = new AngleCorrection(
            d1: new double[] { 0.6, 1.5, 2.4, 3.5, 5, 6.5, 9 },
            d2: new double[] { 0, 0.5, 1, 1.5, 2.5 },
            angles: angles,
            max: snap_0_max,
            posTransform: snap_scale_transform,
            values: new double[,,]{
                { // d1=0.6
                    // 0,   45,   90,   135,   180 degrees
                    { 0.52, 0.52, 0.52, 0.52, 0.52 }, // d2=0
                    { 0.34, 0.40, 0.56, 0.72, 0.77 }, // d2=0.5
                    { 0.43, 0.52, 0.74, 0.88, 0.91 }, // d2=1
                    { 0.68, 0.76, 0.89, 0.95, 0.97 }, // d2=1.5
                    { 0.95, 0.97, 0.98, 0.98, 0.99 }, // d2=2.5
                },
                { // d1=1.5
                    // 0,   45,   90,   135,   180 degrees
                    { 0.76, 0.76, 0.76, 0.76, 0.76 }, // d2=0
                    { 0.37, 0.48, 0.75, 0.91, 0.94 }, // d2=0.5
                    { 0.21, 0.36, 0.81, 0.97, 0.99 }, // d2=1
                    { 0.32, 0.52, 0.92, 0.99, 1.00 }, // d2=1.5
                    { 0.90, 0.96, 1.00, 1.00, 1.00 }, // d2=2.5
                },
                { // d1=2.4
                    // 0,   45,   90,   135,   180 degrees
                    { 0.45, 0.45, 0.45, 0.45, 0.45 }, // d2=0
                    { 0.12, 0.18, 0.39, 0.71, 0.81 }, // d2=0.5
                    { 0.05, 0.11, 0.39, 0.88, 0.96 }, // d2=1
                    { 0.07, 0.17, 0.60, 0.98, 1.00 }, // d2=1.5
                    { 0.56, 0.77, 0.99, 1.00, 1.00 }, // d2=2.5
                },
                { // d1=3.5
                    // 0,   45,   90,   135,   180 degrees
                    { 0.37, 0.37, 0.37, 0.37, 0.37 }, // d2=0
                    { 0.07, 0.12, 0.38, 0.76, 0.88 }, // d2=0.5
                    { 0.02, 0.08, 0.51, 0.96, 1.00 }, // d2=1
                    { 0.03, 0.16, 0.81, 1.00, 1.00 }, // d2=1.5
                    { 0.57, 0.87, 1.00, 1.00, 1.00 }, // d2=2.5
                },
                { // d1=5
                    // 0,   45,   90,   135,   180 degrees
                    { 0.27, 0.27, 0.27, 0.27, 0.27 }, // d2=0
                    { 0.08, 0.13, 0.31, 0.58, 0.69 }, // d2=0.5
                    { 0.04, 0.14, 0.48, 0.84, 0.90 }, // d2=1
                    { 0.16, 0.33, 0.78, 0.94, 0.96 }, // d2=1.5
                    { 0.85, 0.92, 0.96, 0.97, 0.97 }, // d2=2.5
                },
                { // d1=6.5
                    // 0,   45,   90,   135,   180 degrees
                    { 0.26, 0.26, 0.26, 0.26, 0.26 }, // d2=0
                    { 0.13, 0.16, 0.27, 0.44, 0.53 }, // d2=0.5
                    { 0.08, 0.15, 0.32, 0.65, 0.77 }, // d2=1
                    { 0.17, 0.24, 0.49, 0.83, 0.90 }, // d2=1.5
                    { 0.62, 0.71, 0.90, 0.98, 0.99 }, // d2=2.5
                },
                { // d1=9
                    // 0,   45,   90,   135,   180 degrees
                    { 0.26, 0.26, 0.26, 0.26, 0.26 }, // d2=0
                    { 0.13, 0.16, 0.27, 0.44, 0.53 }, // d2=0.5
                    { 0.08, 0.15, 0.32, 0.65, 0.77 }, // d2=1
                    { 0.17, 0.24, 0.49, 0.83, 0.90 }, // d2=1.5
                    { 0.62, 0.71, 0.90, 0.98, 0.99 }, // d2=2.5
                },
            });

        public static readonly AngleCorrection SNAP_3 = new AngleCorrection(
            d1: new double[] { 0.6, 1.5, 2.4, 3.5, 5, 6.5, 9 },
            d2: new double[] { 0, 0.5, 1, 1.5, 2.5 },
            angles: angles,
            posTransform: snap_scale_transform,
            values: new double[,,]{
                { // d1=0.6
                    // 0,   45,   90,   135,   180 degrees
                    { 0.62, 0.62, 0.62, 0.62, 0.62 }, // d2=0
                    { 0.80, 0.77, 0.66, 0.54, 0.49 }, // d2=0.5
                    { 0.92, 0.89, 0.78, 0.59, 0.50 }, // d2=1
                    { 0.97, 0.96, 0.90, 0.76, 0.66 }, // d2=1.5
                    { 1.00, 1.00, 0.99, 0.96, 0.94 }, // d2=2.5
                },
                { // d1=1.5
                    // 0,   45,   90,   135,   180 degrees
                    { 0.62, 0.62, 0.62, 0.62, 0.62 }, // d2=0
                    { 0.80, 0.77, 0.66, 0.54, 0.49 }, // d2=0.5
                    { 0.92, 0.89, 0.78, 0.59, 0.50 }, // d2=1
                    { 0.97, 0.96, 0.90, 0.76, 0.66 }, // d2=1.5
                    { 1.00, 1.00, 0.99, 0.96, 0.94 }, // d2=2.5
                },
                { // d1=2.4
                    // 0,   45,   90,   135,   180 degrees
                    { 0.12, 0.12, 0.12, 0.12, 0.12 }, // d2=0
                    { 0.52, 0.43, 0.27, 0.16, 0.13 }, // d2=0.5
                    { 0.84, 0.75, 0.49, 0.24, 0.17 }, // d2=1
                    { 0.95, 0.91, 0.74, 0.43, 0.31 }, // d2=1.5
                    { 1.00, 0.99, 0.97, 0.88, 0.80 }, // d2=2.5
                },
                { // d1=3.5
                    // 0,   45,   90,   135,   180 degrees
                    { 0.08, 0.08, 0.08, 0.08, 0.08 }, // d2=0
                    { 0.70, 0.56, 0.25, 0.09, 0.05 }, // d2=0.5
                    { 0.97, 0.92, 0.64, 0.18, 0.08 }, // d2=1
                    { 1.00, 0.99, 0.93, 0.57, 0.31 }, // d2=1.5
                    { 1.00, 1.00, 1.00, 0.99, 0.97 }, // d2=2.5
                },
                { // d1=5
                    // 0,   45,   90,   135,   180 degrees
                    { 0.11, 0.11, 0.11, 0.11, 0.11 }, // d2=0
                    { 0.88, 0.77, 0.39, 0.10, 0.05 }, // d2=0.5
                    { 0.99, 0.99, 0.86, 0.29, 0.07 }, // d2=1
                    { 1.00, 1.00, 0.99, 0.83, 0.53 }, // d2=1.5
                    { 1.00, 1.00, 1.00, 1.00, 1.00 }, // d2=2.5
                },
                { // d1=6.5
                    // 0,   45,   90,   135,   180 degrees
                    { 0.09, 0.09, 0.09, 0.09, 0.09 }, // d2=0
                    { 0.79, 0.66, 0.32, 0.10, 0.06 }, // d2=0.5
                    { 0.98, 0.96, 0.76, 0.22, 0.07 }, // d2=1
                    { 1.00, 1.00, 0.97, 0.66, 0.29 }, // d2=1.5
                    { 1.00, 1.00, 1.00, 0.99, 0.98 }, // d2=2.5
                },
                { // d1=9
                    // 0,   45,   90,   135,   180 degrees
                    { 0.09, 0.09, 0.09, 0.09, 0.09 }, // d2=0
                    { 0.79, 0.66, 0.32, 0.10, 0.06 }, // d2=0.5
                    { 0.98, 0.96, 0.76, 0.22, 0.07 }, // d2=1
                    { 1.00, 1.00, 0.97, 0.66, 0.29 }, // d2=1.5
                    { 1.00, 1.00, 1.00, 0.99, 0.98 }, // d2=2.5
                },
            });

    }
}
