using System;
using System.Collections.Generic;
using System.Text;

using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Interpolation;

using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Difficulty.MathUtil;


namespace osu.Game.Rulesets.Osu.Difficulty.Preprocessing
{
    public class OsuMovement
    {
        private static readonly CubicSpline correction0_moving_spline = CubicSpline.InterpolateHermiteSorted(
                                                                            new[] { -1, -0.6, 0.3, 0.5, 1 },
                                                                            new[] { 0.6, 1, 1, 0.6, 0 },
                                                                            new[] { 0.8, 0.8, -0.8, -2, -0.8 });
        // number of coefficients in the formula
        private const int num_coeffs = 4;


        private static readonly double[] ds0f = { 0, 0.5, 1, 1.5, 2, 2.5 };
        private static readonly double[] ks0f = { -6, -6, -8, -9.5, -8.2, -8.2 };
        private static readonly double[,,] coeffs0f = new double[,,]  {{{-0.5 , -0.5 , -1   , -1.5 , -2   , -2   },
                                                                        { 0   ,  0   ,  0   ,  0   ,  0   ,  0   },
                                                                        { 1   ,  1   ,  1   ,  1   ,  1   ,  1   },
                                                                        { 5   ,  5   ,  4   ,  2   ,  2   ,  2   }},
                                                                       {{-0.35, -0.35, -0.7 , -1   , -1.4 , -1.4 },
                                                                        { 0.35,  0.35,  0.7 ,  1   ,  1.4 ,  1.4 },
                                                                        { 1   ,  1   ,  1   ,  1   ,  1   ,  1   },
                                                                        { 0   ,  0   ,  1   ,  2   ,  2   ,  2   }},
                                                                       {{-0.35, -0.35, -0.7 , -1   , -1.4 , -1.4 },
                                                                        {-0.35, -0.35, -0.7 , -1   , -1.4 , -1.4 },
                                                                        { 1   ,  1   ,  1   ,  1   ,  1   ,  1   },
                                                                        { 0   ,  0   ,  1   ,  2   ,  2   ,  2   }}};


        private static readonly double[] ds0s = { 1, 1.5, 2.5, 4, 6, 8 };
        private static readonly double[] ks0s = { -1, -1, -5.9, -5.3, -2.4, -2.4 };
        private static readonly double[,,] coeffs0s = new double[,,]  {{{ 2   , 2   ,  3   ,  4   ,  5   ,  5   },
                                                                        { 0   , 0   ,  0   ,  0   ,  0   ,  0   },
                                                                        { 1   , 1   ,  1   ,  1   ,  1   ,  1   },
                                                                        { 1   , 1   ,  1   ,  0.6 ,  0.4 ,  0.4 }},
                                                                       {{ 1.6 , 1.6 ,  1.8 ,  2   ,  2.5 ,  2.5 },
                                                                        { 2   , 2   ,  2.4 ,  4   ,  4   ,  4   },
                                                                        { 1   , 1   ,  1   ,  1   ,  1   ,  1   },
                                                                        { 0   , 0   ,  0.3 ,  0.24,  0.16,  0.16}},
                                                                       {{ 1.6 , 1.6 ,  1.8 ,  2   ,  2.5 ,  2.5 },
                                                                        { 2   , 2   , -2.4 , -4   , -4   , -4   },
                                                                        { 1   , 1   ,  1   ,  1   ,  1   ,  1   },
                                                                        { 0   , 0   ,  0.3 ,  0.24,  0.16,  0.16}},
                                                                       {{ 0   , 0   ,  0   , -1   , -1.25, -1.25},
                                                                        { 0   , 0   ,  0   ,  0   ,  0   ,  0   },
                                                                        { 1   , 1   ,  1   ,  1   ,  1   ,  1   },
                                                                        { 0   , 0   , -0.3 , -0.3 , -0.32, -0.32}}};

        private static readonly double[] ds3f = { 0, 1, 2, 3, 4 };
        private static readonly double[] ks3f = { -4, -4, -4.5, -2.5, -2.5 };
        private static readonly double[,,] coeffs3f = new double[,,]  {{{0  , 1  , 2  , 4  , 4  },
                                                                        {0  , 0  , 0  , 0  , 0  },
                                                                        {0  , 0  , 0  , 0  , 0  },
                                                                        {1.5, 1.5, 1  , 0  , 0  }},
                                                                       {{0  , 0  , 0  , 0  , 0  },
                                                                        {0  , 0  , 0  , 0  , 0  },
                                                                        {0  , 0  , 0  , 0  , 0  },
                                                                        {2  , 2  , 2.5, 3.5, 3.5}}};

        private static readonly double[] ds3s = { 1, 1.5, 2.5, 4, 6, 8 };
        private static readonly double[] ks3s = { -1.8, -1.8, -3, -5.9, -6 ,-6 };
        private static readonly double[,,] coeffs3s = new double[,,]  {{{-2  , -2  , -3  , -4  , -5  , -5  },
                                                                        { 0  ,  0  ,  0  ,  0  ,  0  ,  0  },
                                                                        { 1  ,  1  ,  1  ,  1  ,  1  ,  1  },
                                                                        { 0.4,  0.4,  0.2,  0.4,  0.4,  0.4}},
                                                                       {{-1  , -1  , -1.5, -2  , -2.5, -2.5},
                                                                        { 1.4,  1.4,  2.1,  2.8,  3.5,  3.5},
                                                                        { 1  ,  1  ,  1  ,  1  ,  1  ,  1  },
                                                                        { 0  ,  0  ,  0.2,  0.4,  0.4,  0.4}},
                                                                       {{-1  , -1  , -1.5, -2  , -2.5, -2.5},
                                                                        {-1.4, -1.4, -2.1, -2.8, -3.5, -3.5},
                                                                        { 1  ,  1  ,  1  ,  1  ,  1  ,  1  },
                                                                        { 0  ,  0  ,  0.2,  0.4,  0.4,  0.4}},
                                                                       {{ 0  ,  0  ,  0  ,  0  ,  0  ,  0  },
                                                                        { 0  ,  0  ,  0  ,  0  ,  0  ,  0  },
                                                                        { 0  ,  0  ,  0  ,  0  ,  0  ,  0  },
                                                                        { 2  ,  2  ,  1  ,  0.4,  0.4,  0.4}},
                                                                       {{ 1  ,  1  ,  1.5,  2  ,  2.5,  2.5},
                                                                        { 0  ,  0  ,  0  ,  0  ,  0  ,  0  },
                                                                        { 1  ,  1  ,  1  ,  1  ,  1  ,  1  },
                                                                        {-1  , -1  , -0.6, -0.4, -0.4, -0.4}}};


        private static LinearSpline k0f_interp;
        private static LinearSpline[,] coeffs0f_interps;
        private static LinearSpline k0s_interp;
        private static LinearSpline[,] coeffs0s_interps;
        private static LinearSpline k3f_interp;
        private static LinearSpline[,] coeffs3f_interps;
        private static LinearSpline k3s_interp;
        private static LinearSpline[,] coeffs3s_interps;

        private const double t_ratio_threshold = 1.4;
        private const double correction0_still = 0.2;

        public double D { get; private set; }
        public double MT { get; private set; }

        
        public OsuMovement(OsuHitObject obj0, OsuHitObject obj1, OsuHitObject obj2, OsuHitObject obj3, double clockRate)
        {
            var pos1 = Vector<double>.Build.Dense(new[] {(double)obj1.Position.X, (double)obj1.Position.Y});
            var pos2 = Vector<double>.Build.Dense(new[] {(double)obj2.Position.X, (double)obj2.Position.Y});
            var s12 = (pos2 - pos1) / (2 * obj2.Radius);
            double d12 = s12.L2Norm();
            double t12 = (obj2.StartTime - obj1.StartTime) / clockRate / 1000.0;

            // Correction #1 - The Previous Object
            // Estimate how obj0 affects the difficulty of hitting obj2
            double correction0 = 0;
            if (obj0 != null)
            {
                var pos0 = Vector<double>.Build.Dense(new[] {(double)obj0.Position.X, (double)obj0.Position.Y});
                var s01 = (pos1 - pos0) / (2 * obj2.Radius);

                if (d12 != 0)
                {
                    double d01 = s01.L2Norm();
                    double t01 = (obj1.StartTime - obj0.StartTime) / clockRate / 1000.0;
                    double t12_to_t01 = t12 / t01;

                    if (t12_to_t01 > t_ratio_threshold)
                    {
                        if (d01 == 0)
                        {
                            correction0 = correction0_still;
                        }
                        else
                        {
                            double cos012 = Math.Min(Math.Max(-s01.DotProduct(s12) / d01 / d12, -1), 1);
                            double correction0_moving = correction0_moving_spline.Interpolate(cos012);

                            double movingness = SpecialFunctions.Logistic(d01 * 2) * 2 - 1;
                            correction0 = (movingness * correction0_moving + (1 - movingness) * correction0_still) * 0.8;

                            //Console.Write(obj2.StartTime);
                            //Console.Write(" " + cos012);
                            //Console.WriteLine(" " + correction0);

                        }
                    }
                    else if (t12_to_t01 < 1 / t_ratio_threshold)
                    {
                        if (d01 == 0)
                        {
                            correction0 = 0;
                        }
                        else
                        {
                            double cos012 = Math.Min(Math.Max(-s01.DotProduct(s12) / d01 / d12, -1), 1);
                            correction0 = (1 - cos012) * SpecialFunctions.Logistic((d01 * t12_to_t01 - 1.5) * 4) * 0.3;
                        }
                    }
                    else
                    {
                        var normalized_pos0 = -s01 / t01 * t12;
                        double x0 = normalized_pos0.DotProduct(s12) / d12;
                        double y0 = (normalized_pos0 - x0 * s12 / d12).L2Norm();

                        double correction0_flow = calc_correction_0_or_3(d12, x0, y0, k0f_interp, coeffs0f_interps);
                        double correction0_snap = calc_correction_0_or_3(d12, x0, y0, k0s_interp, coeffs0s_interps);

                        correction0 = Mean.PowerMean(correction0_flow, correction0_snap, -10);
                    }
                }
            }

            // Correction #2 - The Next Object
            // Estimate how obj3 affects the difficulty of hitting obj2
            double correction3 = 0;

            if (obj3 != null)
            {
                var pos3 = Vector<double>.Build.Dense(new[] { (double)obj3.Position.X, (double)obj3.Position.Y });
                var s23 = (pos3 - pos2) / (2 * obj2.Radius);


                if (d12 != 0)
                {
                    double d23 = s23.L2Norm();
                    double t23 = (obj3.StartTime - obj2.StartTime) / clockRate / 1000.0;
                    double t12_to_t23 = t12 / t23;


                    if (t12_to_t23 > t_ratio_threshold)
                    {
                        if (d23 == 0)
                        {
                            correction3 = 0;
                        }
                        else
                        {
                            double cos123 = Math.Min(Math.Max(-s12.DotProduct(s23) / d12 / d23, -1), 1);
                            double correction3_moving = correction0_moving_spline.Interpolate(cos123);

                            double movingness = SpecialFunctions.Logistic(d23 * 6 - 5) - SpecialFunctions.Logistic(-5);
                            correction3 = (movingness * correction3_moving) * 0.5;

                        }
                    }
                    else if (t12_to_t23 < 1 / t_ratio_threshold)
                    {
                        if (d23 == 0)
                        {
                            correction3 = 0;
                        }
                        else
                        {
                            double cos123 = Math.Min(Math.Max(-s12.DotProduct(s23) / d12 / d23, -1), 1);
                            correction3 = (1 - cos123) * SpecialFunctions.Logistic((d23 * t12_to_t23 - 1.5) * 4) * 0.15;
                        }
                    }
                    else
                    {
                        var normalized_pos3 = s23 / t23 * t12;
                        double x3 = normalized_pos3.DotProduct(s12) / d12;
                        double y3 = (normalized_pos3 - x3 * s12 / d12).L2Norm();
                        double correction3_flow = calc_correction_0_or_3(d12, x3, y3, k3f_interp, coeffs3f_interps);
                        double correction3_snap = calc_correction_0_or_3(d12, x3, y3, k3s_interp, coeffs3s_interps);

                        correction3 = Math.Max(Mean.PowerMean(correction3_flow, correction3_snap, -10) - 0.1, 0) * 0.5;

                    }




                    //    normalized_pos3 = s23 / t23 * t12
                    //x0 = normalized_pos3.dot(s12) / d12
                    //y0 = norm(normalized_pos3 - x0 * s12 / d12)

                    //correction3_flow = calc_correction3_flow(d12, x0, y0)
                    //correction3_snap = calc_correction3_snap(d12, x0, y0)
                }
            }
            double d12_aim_correction = d12 * (1 + correction0 + correction3);
            this.D = d12_aim_correction;
            this.MT = t12;
        }

        public static void Initialize()
        {
            prepare_interp(ds0f, ks0f, coeffs0f, ref k0f_interp, ref coeffs0f_interps);
            prepare_interp(ds0s, ks0s, coeffs0s, ref k0s_interp, ref coeffs0s_interps);
            prepare_interp(ds3f, ks3f, coeffs3f, ref k3f_interp, ref coeffs3f_interps);
            prepare_interp(ds3s, ks3s, coeffs3s, ref k3s_interp, ref coeffs3s_interps);
        }


        private static void prepare_interp(double[] ds, double[] ks, double[,,] coeffs,
                                           ref LinearSpline k_interp, ref LinearSpline[,] coeffs_interps)
        {
            k_interp = LinearSpline.InterpolateSorted(ds, ks);

            coeffs_interps = new LinearSpline[coeffs.GetLength(0), num_coeffs];
            for (int i = 0; i < coeffs.GetLength(0); i++)
            {
                for (int j = 0; j < num_coeffs; j++)
                {
                    double[] coeff_ij = new double[coeffs.GetLength(2)];
                    for (int k = 0; k < coeffs.GetLength(2); k++)
                    {
                        coeff_ij[k] = coeffs[i, j, k];
                    }
                    coeffs_interps[i, j] = LinearSpline.InterpolateSorted(ds, coeff_ij);
                }
            }
        }

        private static double calc_correction_0_or_3(double d, double x, double y,
                                                     LinearSpline k_interp, LinearSpline[,] coeffs_interps)
        {
            double correction_raw = k_interp.Interpolate(d);
            for (int i = 0; i < coeffs_interps.GetLength(0); i++)
            {
                double[] cs = new double[num_coeffs];
                for (int j = 0; j < num_coeffs; j++)
                {
                    cs[j] = coeffs_interps[i, j].Interpolate(d);
                }
                correction_raw += cs[3] * Math.Sqrt(Math.Pow((x - cs[0]), 2) +
                                                    Math.Pow((y - cs[1]), 2) +
                                                    cs[2]);
            }
            return SpecialFunctions.Logistic(correction_raw);
        }

    }
}
