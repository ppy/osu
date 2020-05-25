using System;
using System.Collections.Generic;

using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Interpolation;

using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Difficulty.MathUtil;


namespace osu.Game.Rulesets.Osu.Difficulty.Preprocessing
{
    public class OsuMovement
    {
        private static readonly LinearSpline correction0_moving_spline = LinearSpline.InterpolateSorted(
                                                                           new double[] { -1, 1 },
                                                                           new double[] { 1.1, 0 });

        // number of coefficients in the formula for correction0/3
        private const int num_coeffs = 4;

        // correction0 flow
        private static readonly double[] ds0f = { 0, 1, 1.35, 1.7, 2.3, 3 };
        private static readonly double[] ks0f = { -11.5, -5.9, -5.4, -5.6, -2, -2 };
        private static readonly double[] scales0f = { 1, 1, 1, 1, 1, 1 };
        private static readonly double[,,] coeffs0f = new double[,,]  {{{ 0   , -0.5 , -1.15 , -1.8 , -2   , -2   },
                                                                        { 0   ,  0   ,  0    ,  0   ,  0   ,  0   },
                                                                        { 1   ,  1   ,  1    ,  1   ,  1   ,  1   },
                                                                        { 6   ,  1   ,  1    ,  1   ,  1   ,  1   }},
                                                                       {{ 0   , -0.8 , -0.9  , -1   , -1   , -1   },
                                                                        { 0   ,  0.5 ,  0.75 ,  1   ,  2   ,  2   },
                                                                        { 1   ,  0.5 ,  0.4  ,  0.3 ,  0   ,  0   },
                                                                        { 3   ,  0.7 ,  0.7  ,  0.7 ,  1   ,  1   }},
                                                                       {{ 0   , -0.8 , -0.9  , -1   , -1   , -1   },
                                                                        { 0   , -0.5 , -0.75 , -1   , -2   , -2   },
                                                                        { 1   ,  0.5 ,  0.4  ,  0.3 ,  0   ,  0   },
                                                                        { 3   ,  0.7 ,  0.7  ,  0.7 ,  1   ,  1   }},
                                                                       {{ 0   ,  0   ,  0    ,  0   ,  0   ,  0   },
                                                                        { 0   ,  0.95,  0.975,  1   ,  0   ,  0   },
                                                                        { 0   ,  0   ,  0    ,  0   ,  0   ,  0   },
                                                                        { 0   ,  0.7 ,  0.55 ,  0.4 ,  0   ,  0   }},
                                                                       {{ 0   ,  0   ,  0    ,  0   ,  0   ,  0   },
                                                                        { 0   , -0.95, -0.975, -1   ,  0   ,  0   },
                                                                        { 0   ,  0   ,  0    ,  0   ,  0   ,  0   },
                                                                        { 0   ,  0.7 ,  0.55 ,  0.4 ,  0   ,  0   }}};

        // correction0 snap
        private static readonly double[] ds0s = { 0, 1.5, 2.5, 4, 6, 8 };
        private static readonly double[] ks0s = { -1, -5, -6.7, -6.5, -4.3, -4.3 };
        private static readonly double[] scales0s = { 1, 0.85, 0.6, 0.8, 1, 1 };
        private static readonly double[,,] coeffs0s = new double[,,]  {{{ 0.5 ,  2   ,  2.8 ,  5   ,  5   ,  5   },
                                                                        { 0   ,  0   ,  0   ,  0   ,  0   ,  0   },
                                                                        { 1   ,  1   ,  1   ,  0   ,  0   ,  0   },
                                                                        { 0.6 ,  1   ,  0.8 ,  0.6 ,  0.2 ,  0.2 }},
                                                                       {{ 0.25,  1   ,  0.7 ,  2   ,  2   ,  2   },
                                                                        { 0.5 ,  2   ,  2.8 ,  4   ,  6   ,  6   },
                                                                        { 1   ,  1   ,  1   ,  1   ,  1   ,  1   },
                                                                        { 0.6 ,  1   ,  0.8 ,  0.3 ,  0.2 ,  0.2 }},
                                                                       {{ 0.25,  1   ,  0.7 ,  2   ,  2   ,  2   },
                                                                        {-0.5 , -2   , -2.8 , -4   , -6   , -6   },
                                                                        { 1   ,  1   ,  1   ,  1   ,  1   ,  1   },
                                                                        { 0.6 ,  1   ,  0.8 ,  0.3 ,  0.2 ,  0.2 }},
                                                                       {{ 0   ,  0   , -0.5 , -2   , -3   , -3   },
                                                                        { 0   ,  0   ,  0   ,  0   ,  0   ,  0   },
                                                                        { 1   ,  1   ,  1   ,  1   ,  1   ,  1   },
                                                                        {-0.7 , -1   , -0.9 , -0.1 , -0.1 , -0.1 }}};

        // correction3 flow
        private static readonly double[] ds3f = { 0, 1, 2, 3, 4 };
        private static readonly double[] ks3f = { -4, -5.3, -5.2, -2.5, -2.5 };
        private static readonly double[] scales3f = { 1, 1, 1, 1, 1 };
        private static readonly double[,,] coeffs3f = new double[,,]  {{{0   ,  1.2 ,  2   ,  2   ,  2  },
                                                                        {0   ,  0   ,  0   ,  0   ,  0  },
                                                                        {0   ,  0   ,  0   ,  0   ,  0  },
                                                                        {1.5 ,  1   ,  0.4 ,  0   ,  0  }},
                                                                       {{0   ,  0   ,  0   ,  0   ,  0  },
                                                                        {0   ,  0   ,  0   ,  0   ,  0  },
                                                                        {0   ,  0   ,  0   ,  0   ,  0  },
                                                                        {2   ,  1.5 ,  2.5 ,  3.5 ,  3.5}},
                                                                       {{0   ,  0.3 ,  0.6 ,  0.6 ,  0.6},
                                                                        {0   ,  1   ,  2.4 ,  2.4 ,  2.4},
                                                                        {0   ,  0   ,  0   ,  0   ,  0  },
                                                                        {0   ,  0.4 ,  0.4 ,  0   ,  0  }},
                                                                       {{0   ,  0.3 ,  0.6 ,  0.6 ,  0.6},
                                                                        {0   , -1   , -2.4 , -2.4 , -2.4},
                                                                        {0   ,  0   ,  0   ,  0   ,  0  },
                                                                        {0   ,  0.4 ,  0.4 ,  0   ,  0  }}};

        // correction3 snap
        private static readonly double[] ds3s = { 1, 1.5, 2.5, 4, 6, 8 };
        private static readonly double[] ks3s = { -2, -2, -3, -5.4, -4.9, -4.9 };
        private static readonly double[] scales3s = { 1, 1, 1, 1, 1, 1 };
        private static readonly double[,,] coeffs3s = new double[,,]  {{{-2  , -2  , -3  , -4  , -6  , -6  },
                                                                        { 0  ,  0  ,  0  ,  0  ,  0  ,  0  },
                                                                        { 1  ,  1  ,  1  ,  0  ,  0  ,  0  },
                                                                        { 0.4,  0.4,  0.2,  0.4,  0.3,  0.3}},
                                                                       {{-1  , -1  , -1.5, -2  , -3  , -3  },
                                                                        { 1.4,  1.4,  2.1,  2  ,  3  ,  3  },
                                                                        { 1  ,  1  ,  1  ,  1  ,  1  ,  1  },
                                                                        { 0.4,  0.4,  0.2,  0.4,  0.2,  0.2}},
                                                                       {{-1  , -1  , -1.5, -2  , -3  , -3  },
                                                                        {-1.4, -1.4, -2.1, -2  , -3  , -3  },
                                                                        { 1  ,  1  ,  1  ,  1  ,  1  ,  1  },
                                                                        { 0.4,  0.4,  0.2,  0.4,  0.2,  0.2}},
                                                                       {{ 0  ,  0  ,  0  ,  0  ,  0  ,  0  },
                                                                        { 0  ,  0  ,  0  ,  0  ,  0  ,  0  },
                                                                        { 0  ,  0  ,  0  ,  0  ,  0  ,  0  },
                                                                        { 0  ,  0  ,  1  ,  0.6,  0.6,  0.6}},
                                                                       {{ 1  ,  1  ,  1.5,  2  ,  3  ,  3  },
                                                                        { 0  ,  0  ,  0  ,  0  ,  0  ,  0  },
                                                                        { 1  ,  1  ,  1  ,  1  ,  1  ,  1  },
                                                                        { 0  ,  0  , -0.6, -0.4, -0.3, -0.3}}};

        private static LinearSpline k0fInterp;
        private static LinearSpline scale0fInterp;
        private static LinearSpline[,] coeffs0fInterps;
        private static LinearSpline k0sInterp;
        private static LinearSpline scale0sInterp;
        private static LinearSpline[,] coeffs0sInterps;
        private static LinearSpline k3fInterp;
        private static LinearSpline scale3fInterp;
        private static LinearSpline[,] coeffs3fInterps;
        private static LinearSpline k3sInterp;
        private static LinearSpline scale3sInterp;
        private static LinearSpline[,] coeffs3sInterps;

        private const double t_ratio_threshold = 1.4;
        private const double correction0_still = 0;

        public double RawMT { get; private set; }
        public double D { get; private set; }
        public double MT { get; private set; }
        public double IP12 { get; private set; }
        public double Cheesablility { get; private set; }
        public double CheesableRatio { get; private set; }
        public double Time { get; private set; }
        public bool EndsOnSlider { get; private set; }

        /// <summary>
        /// Extracts movement (only for the first object in a beatmap).
        /// </summary>
        public static List<OsuMovement> ExtractMovement(OsuHitObject obj)
        {
            var movement = GetEmptyMovement(obj.StartTime / 1000.0);

            var movementWithNested = new List<OsuMovement>() { movement };
            // add zero difficulty movements corresponding to slider ticks/slider ends so combo is reflected properly
            int extraNestedCount = obj.NestedHitObjects.Count - 1;

            for (int i = 0; i < extraNestedCount; i++)
            {
                movementWithNested.Add(GetEmptyMovement(movement.Time));
            }

            return movementWithNested;
        }

        /// <summary>
        /// Calculates the movement time, effective distance and other details for the movement from obj1 to obj2.
        /// </summary>
        public static List<OsuMovement> ExtractMovement(OsuHitObject obj0, OsuHitObject obj1, OsuHitObject obj2, OsuHitObject obj3,
                                                        Vector<double> tapStrain, double clockRate,
                                                        bool hidden = false, double noteDensity = 0, OsuHitObject objMinus2 = null)
        {
            
            var movement = new OsuMovement();

            double t12 = (obj2.StartTime - obj1.StartTime) / clockRate / 1000.0;
            movement.RawMT = t12;
            movement.Time = obj2.StartTime / 1000.0;

            if (obj2 is Spinner || obj1 is Spinner)
            {
                movement.IP12 = 0;
                movement.D = 0;
                movement.MT = 1;
                movement.Cheesablility = 0;
                movement.CheesableRatio = 0;
                return new List<OsuMovement>() { movement };
            }

            if (obj0 is Spinner)
                obj0 = null;

            if (obj3 is Spinner)
                obj3 = null;

            if (obj2 is Slider)
                movement.EndsOnSlider = true;

            // calculate basic info (position, displacement, distance...)
            // explanation of abbreviations:
            // posx: position of obj x
            // sxy : displacement (normalized) from obj x to obj y
            // txy : time difference of obj x and obj y
            // dxy : distance (normalized) from obj x to obj y
            // ipxy: index of performance of the movement from obj x to obj y
            var pos1 = Vector<double>.Build.Dense(new[] {(double)obj1.Position.X, (double)obj1.Position.Y});
            var pos2 = Vector<double>.Build.Dense(new[] {(double)obj2.Position.X, (double)obj2.Position.Y});
            var s12 = (pos2 - pos1) / (2 * obj2.Radius);
            double d12 = s12.L2Norm();
            double ip12 = FittsLaw.CalculateIP(d12, t12);

            movement.IP12 = ip12;

            var pos0 = Vector<double>.Build.Dense(2);
            var pos3 = Vector<double>.Build.Dense(2);
            var s01 = Vector<double>.Build.Dense(2);
            var s23 = Vector<double>.Build.Dense(2);
            double d01 = 0;
            double d02 = 0;
            double d23 = 0;
            double t01 = 0;
            double t23 = 0;

            double flowiness012 = 0;
            double flowiness123 = 0;
            bool obj1InTheMiddle = false;
            bool obj2InTheMiddle = false;

            double dMinus22 = 0;
            if (objMinus2 != null)
            {
                var posMinus2 = Vector<double>.Build.Dense(new[] { (double)objMinus2.Position.X, (double)objMinus2.Position.Y });
                dMinus22 = ((pos2 - posMinus2) / (2 * obj2.Radius)).L2Norm();
            }

            if (obj0 != null)
            {
                pos0 = Vector<double>.Build.Dense(new[] { (double)obj0.Position.X, (double)obj0.Position.Y });
                s01 = (pos1 - pos0) / (2 * obj2.Radius);
                d01 = s01.L2Norm();
                t01 = (obj1.StartTime - obj0.StartTime) / clockRate / 1000.0;
                d02 = ((pos2 - pos0) / (2 * obj2.Radius)).L2Norm();
            }

            if (obj3 != null)
            {
                pos3 = Vector<double>.Build.Dense(new[] { (double)obj3.Position.X, (double)obj3.Position.Y });
                s23 = (pos3 - pos2) / (2 * obj2.Radius);
                d23 = s23.L2Norm();
                t23 = (obj3.StartTime - obj2.StartTime) / clockRate / 1000.0;
            }

            // Correction #1 - The Previous Object
            // Estimate how obj0 affects the difficulty of hitting obj2
            double correction0 = 0;

            if (obj0 != null && d12 != 0)
            {
                double tRatio0 = t12 / t01;

                if (tRatio0 > t_ratio_threshold)
                {
                    if (d01 == 0)
                    {
                        correction0 = correction0_still;
                    }
                    else
                    {
                        double cos012 = Math.Min(Math.Max(-s01.DotProduct(s12) / d01 / d12, -1), 1);
                        double correction0_moving = correction0_moving_spline.Interpolate(cos012);

                        double movingness = SpecialFunctions.Logistic(d01 * 6 - 5) - SpecialFunctions.Logistic(-5);
                        correction0 = (movingness * correction0_moving + (1 - movingness) * correction0_still) * 1.5;
                    }
                }
                else if (tRatio0 < 1 / t_ratio_threshold)
                {
                    if (d01 == 0)
                    {
                        correction0 = 0;
                    }
                    else
                    {
                        double cos012 = Math.Min(Math.Max(-s01.DotProduct(s12) / d01 / d12, -1), 1);
                        correction0 = (1 - cos012) * SpecialFunctions.Logistic((d01 * tRatio0 - 1.5) * 4) * 0.3;
                    }
                }
                else
                {
                    obj1InTheMiddle = true;

                    var normalized_pos0 = -s01 / t01 * t12;
                    double x0 = normalized_pos0.DotProduct(s12) / d12;
                    double y0 = (normalized_pos0 - x0 * s12 / d12).L2Norm();

                    double correction0Flow = calcCorrection0Or3(d12, x0, y0, k0fInterp, scale0fInterp, coeffs0fInterps);
                    double correction0Snap = calcCorrection0Or3(d12, x0, y0, k0sInterp, scale0sInterp, coeffs0sInterps);
                    double correction0Stop = calcCorrection0Stop(d12, x0, y0);

                    flowiness012 = SpecialFunctions.Logistic((correction0Snap - correction0Flow - 0.05) * 20);

                    correction0 = Mean.PowerMean(new double[] { correction0Flow, correction0Snap, correction0Stop }, -10) * 1.3;
                }
            }

            // Correction #2 - The Next Object
            // Estimate how obj3 affects the difficulty of hitting obj2
            double correction3 = 0;

            if (obj3 != null && d12 != 0)
            {
                double tRatio3 = t12 / t23;

                if (tRatio3 > t_ratio_threshold)
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
                else if (tRatio3 < 1 / t_ratio_threshold)
                {
                    if (d23 == 0)
                    {
                        correction3 = 0;
                    }
                    else
                    {
                        double cos123 = Math.Min(Math.Max(-s12.DotProduct(s23) / d12 / d23, -1), 1);
                        correction3 = (1 - cos123) * SpecialFunctions.Logistic((d23 * tRatio3 - 1.5) * 4) * 0.15;
                    }
                }
                else
                {
                    obj2InTheMiddle = true;

                    var normalizedPos3 = s23 / t23 * t12;
                    double x3 = normalizedPos3.DotProduct(s12) / d12;
                    double y3 = (normalizedPos3 - x3 * s12 / d12).L2Norm();

                    double correction3Flow = calcCorrection0Or3(d12, x3, y3, k3fInterp, scale3fInterp, coeffs3fInterps);
                    double correction3Snap = calcCorrection0Or3(d12, x3, y3, k3sInterp, scale3sInterp, coeffs3sInterps);

                    flowiness123 = SpecialFunctions.Logistic((correction3Snap - correction3Flow - 0.05) * 20);

                    correction3 = Math.Max(Mean.PowerMean(correction3Flow, correction3Snap, -10) - 0.1, 0) * 0.5;
                }
            }

            // Correction #3 - 4-object pattern
            // Estimate how the whole pattern consisting of obj0 to obj3 affects 
            // the difficulty of hitting obj2. This only takes effect when the pattern
            // is not so spaced (i.e. does not contain jumps)
            double patternCorrection = 0;

            if (obj1InTheMiddle && obj2InTheMiddle)
            {
                double gap = (s12 - s23 / 2 - s01 / 2).L2Norm() / (d12 + 0.1);
                patternCorrection = (SpecialFunctions.Logistic((gap - 1) * 8) - SpecialFunctions.Logistic(-6)) *
                                    SpecialFunctions.Logistic((d01 - 0.7) * 10) * SpecialFunctions.Logistic((d23 - 0.7) * 10) *
                                    Mean.PowerMean(flowiness012, flowiness123, 2) * 0.6;
            }

            // Correction #4 - Tap Strain
            // Estimate how tap strain affects difficulty
            double tapCorrection = 0;

            if (d12 > 0 && tapStrain != null)
            {
                tapCorrection = SpecialFunctions.Logistic((Mean.PowerMean(tapStrain, 2) / ip12 - 1.34) / 0.1) * 0.3;
            }

            // Correction #5 - Cheesing
            // The player might make the movement of obj1 -> obj2 easier by 
            // hitting obj1 early and obj2 late. Here we estimate the amount of 
            // cheesing and update MT accordingly.
            double timeEarly = 0;
            double timeLate = 0;
            double cheesabilityEarly = 0;
            double cheesabilityLate = 0;

            if (d12 > 0)
            {
                double t01Reciprocal;
                double ip01;
                if (obj0 != null)
                {
                    t01Reciprocal = 1 / (t01 + 1e-10);
                    ip01 = FittsLaw.CalculateIP(d01, t01);
                }
                else
                {
                    t01Reciprocal = 0;
                    ip01 = 0;
                }
                cheesabilityEarly = SpecialFunctions.Logistic((ip01 / ip12 - 0.6) * (-15)) * 0.5;
                timeEarly = cheesabilityEarly * (1 / (1 / (t12 + 0.07) + t01Reciprocal)) ;

                double t23Reciprocal;
                double ip23;
                if (obj3 != null)
                {
                    t23Reciprocal = 1 / (t23 + 1e-10);
                    ip23 = FittsLaw.CalculateIP(d23, t23);
                }
                else
                {
                    t23Reciprocal = 0;
                    ip23 = 0;
                }
                cheesabilityLate = SpecialFunctions.Logistic((ip23 / ip12 - 0.6) * (-15)) * 0.5;
                timeLate = cheesabilityLate * (1 / (1 / (t12 + 0.07) + t23Reciprocal));
            }

            // Correction #6 - High bpm jump buff (alt buff)
            double effectiveBpm = 30 / (t12 + 1e-10);
            double highBpmJumpBuff = SpecialFunctions.Logistic((effectiveBpm - 354) / 16) *
                                     SpecialFunctions.Logistic((d12 - 1.9) / 0.15) * 0.23;


            // Correction #7 - Small circle bonus
            double smallCircleBonus = SpecialFunctions.Logistic((55 - 2 * obj2.Radius) / 3.0) * 0.3;

            // Correction #8 - Stacked notes nerf
            double d12StackedNerf = Math.Max(0, Math.Min(d12, Math.Min(1.2 * d12 - 0.185, 1.4 * d12 - 0.32)));

            // Correction #9 - Slow small jump nerf
            double smallJumpNerfFactor = 1 - 0.17 * Math.Exp(-Math.Pow((d12 - 2.2) / 0.7, 2)) *
                                             SpecialFunctions.Logistic((255 - effectiveBpm) / 10);

            // Correction #10 - Slow big jump buff
            double bigJumpBuffFactor = 1 + 0.15 * SpecialFunctions.Logistic((d12 - 6) / 0.5) *
                                           SpecialFunctions.Logistic((210 - effectiveBpm) / 8);

            // Correction #11 - Hidden Mod
            double correctionHidden = 0;
            if (hidden)
            {
                correctionHidden = 0.05 + 0.008 * noteDensity;
            }


            // Correction #12 - Stacked wiggle fix
            if (obj0 != null && obj3 != null)
            {
                var d13 = ((pos3 - pos1) / (2 * obj2.Radius)).L2Norm();
                var d03 = ((pos3 - pos0) / (2 * obj2.Radius)).L2Norm();

                if (d01 < 1 && d02 < 1 && d03 < 1 && d12 < 1 && d13 < 1 && d23 < 1)
                {
                    correction0 = 0;
                    correction3 = 0;
                    patternCorrection = 0;
                    tapCorrection = 0;
                }
            }

            // Correction #13 - Repetitive jump nerf
            // Nerf big jumps where obj0 and obj2 are close or where objMinus2 and obj2 are close
            double jumpOverlapCorrection = 1 - (Math.Max(0.15 - 0.1 * d02, 0) + Math.Max(0.1125 - 0.075 * dMinus22, 0)) *
                                               SpecialFunctions.Logistic((d12 - 3.3) / 0.25);


            // Correction #14 - Sudden distance increase buff
            double distanceIncreaseBuff = 1;
            if (obj0 != null)
            {
                double d01OverlapNerf = Math.Min(1, Math.Pow(d01, 3));
                double timeDifferenceNerf = Math.Exp(-4 * Math.Pow(1 - Math.Max(t12 / t01, t01 / t12), 2));
                double distanceRatio = d12 / Math.Max(1, d01);
                double bpmScaling = Math.Max(1, -16 * t12 + 3.4);
                distanceIncreaseBuff = 1 + 0.225 * bpmScaling * timeDifferenceNerf * d01OverlapNerf * Math.Max(0, distanceRatio - 2);
            }

            // Apply the corrections
            double d12WithCorrection = d12StackedNerf * (1 + smallCircleBonus) * (1 + correction0 + correction3 + patternCorrection) *
                                       (1 + highBpmJumpBuff) * (1 + tapCorrection) * smallJumpNerfFactor * bigJumpBuffFactor * (1 + correctionHidden) *
                                       jumpOverlapCorrection * distanceIncreaseBuff;

            movement.D = d12WithCorrection;
            movement.MT = t12;
            movement.Cheesablility = cheesabilityEarly + cheesabilityLate;
            movement.CheesableRatio = (timeEarly + timeLate) / (t12 + 1e-10);

            var movementWithNested = new List<OsuMovement>() { movement };

            // add zero difficulty movements corresponding to slider ticks/slider ends so combo is reflected properly
            int extraNestedCount = obj2.NestedHitObjects.Count - 1;

            for (int i = 0; i < extraNestedCount; i++)
            {
                movementWithNested.Add(GetEmptyMovement(movement.Time));
            }

            return movementWithNested;
        }

        public static OsuMovement GetEmptyMovement(double time)
        {
            return new OsuMovement()
            {
                D = 0,
                MT = 1,
                CheesableRatio = 0,
                Cheesablility = 0,
                RawMT = 0,
                IP12 = 0,
                Time = time
            };
        }

        /// <summary>
        /// Gets the interpolations for correction0/3 ready for use.
        /// This needs to be called before any ExtractMovement calls.
        /// </summary>
        public static void Initialize()
        {
            prepareInterp(ds0f, ks0f, scales0f, coeffs0f, ref k0fInterp, ref scale0fInterp, ref coeffs0fInterps);
            prepareInterp(ds0s, ks0s, scales0s, coeffs0s, ref k0sInterp, ref scale0sInterp, ref coeffs0sInterps);
            prepareInterp(ds3f, ks3f, scales3f, coeffs3f, ref k3fInterp, ref scale3fInterp, ref coeffs3fInterps);
            prepareInterp(ds3s, ks3s, scales3s, coeffs3s, ref k3sInterp, ref scale3sInterp, ref coeffs3sInterps);
        }


        private static void prepareInterp(double[] ds, double[] ks, double[] scales, double[,,] coeffs,
                                           ref LinearSpline kInterp, ref LinearSpline scaleInterp, ref LinearSpline[,] coeffsInterps)
        {
            kInterp = LinearSpline.InterpolateSorted(ds, ks);
            scaleInterp = LinearSpline.InterpolateSorted(ds, scales);

            coeffsInterps = new LinearSpline[coeffs.GetLength(0), num_coeffs];
            for (int i = 0; i < coeffs.GetLength(0); i++)
            {
                for (int j = 0; j < num_coeffs; j++)
                {
                    double[] coeff_ij = new double[coeffs.GetLength(2)];
                    for (int k = 0; k < coeffs.GetLength(2); k++)
                    {
                        coeff_ij[k] = coeffs[i, j, k];
                    }
                    coeffsInterps[i, j] = LinearSpline.InterpolateSorted(ds, coeff_ij);
                }
            }
        }

        private static double calcCorrection0Or3(double d, double x, double y,
                                                 LinearSpline kInterp, LinearSpline scaleInterp, LinearSpline[,] coeffsInterps)
        {
            double correction_raw = kInterp.Interpolate(d);
            for (int i = 0; i < coeffsInterps.GetLength(0); i++)
            {
                double[] cs = new double[num_coeffs];
                for (int j = 0; j < num_coeffs; j++)
                {
                    cs[j] = coeffsInterps[i, j].Interpolate(d);
                }
                correction_raw += cs[3] * Math.Sqrt(Math.Pow((x - cs[0]), 2) +
                                                    Math.Pow((y - cs[1]), 2) +
                                                    cs[2]);
            }
            return SpecialFunctions.Logistic(correction_raw) * scaleInterp.Interpolate(d);
        }

        private static double calcCorrection0Stop(double d, double x, double y)
        {
            return SpecialFunctions.Logistic(10 * Math.Sqrt(x * x + y * y + 1) - 12);
        }
    }
}
