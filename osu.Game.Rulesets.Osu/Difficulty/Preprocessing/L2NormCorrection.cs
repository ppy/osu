// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using MathNet.Numerics;
using MathNet.Numerics.Interpolation;

namespace osu.Game.Rulesets.Osu.Difficulty.Preprocessing
{
    public class L2NormCorrection
    {
        public L2NormCorrection(double[] distance, double[] x_offset, double[] y_offset, double[] offset, double[] scale)
        {
            this.x_offset = LinearSpline.InterpolateSorted(distance, x_offset);
            this.y_offset = LinearSpline.InterpolateSorted(distance, y_offset);
            this.offset = LinearSpline.InterpolateSorted(distance, offset);
            this.scale = LinearSpline.InterpolateSorted(distance, scale);
        }

        public double Evaluate(double distance, double x, double y)
        {
            x -= x_offset.Interpolate(distance);
            y -= y_offset.Interpolate(distance);
            double z = offset.Interpolate(distance);
            double c = scale.Interpolate(distance);
            return c * Math.Sqrt(x * x + y * y + z);
        }

        private LinearSpline x_offset, y_offset, offset, scale;
    }

    public class MultiL2NormCorrection
    {
        public MultiL2NormCorrection(L2NormCorrection[] components, double[] distance, double[] offset, double[] scale)
        {
            this.offset = LinearSpline.InterpolateSorted(distance, offset);
            this.scale = LinearSpline.InterpolateSorted(distance, scale);
            this.components = components;
        }

        public double Evaluate(double distance, double x, double y)
        {
            double result = components.Select(component => component.Evaluate(distance, x, y)).Sum();
            result += offset.Interpolate(distance);
            return SpecialFunctions.Logistic(result) * scale.Interpolate(distance);
        }

        private L2NormCorrection[] components;
        private LinearSpline offset, scale;

        private static readonly double[] angles = { 0, 0.25 * Math.PI, 0.5 * Math.PI, 0.75 * Math.PI, Math.PI};
        private static readonly double[] distances = { 0, 0.5, 1, 1.5, 2, 3, 6,10 };

        private static readonly double[] flowdistances = { 0, 0.7, 1, 1.3, 1.7, 3};
        private static readonly double[] snapdistances = { 0, 1, 2, 3, 5, 9};


        private static readonly double[] flow0distances = { 0.0, 1.0, 1.35, 1.7, 2.3, 3 };
        public static readonly MultiL2NormCorrection FLOW_0_OLD = new MultiL2NormCorrection(
            distance: flow0distances,
            offset: new double[] { -11.5, -5.9, -5.4, -5.6, -2, -2 },
            scale: new double[] { 1.0, 1.0, 1.0, 1.0, 1.0, 1.0 },
            components: new[] {
                new L2NormCorrection(
                    distance: flow0distances,
                    x_offset: new double[] { 0, -0.5, -1.15, -1.8, -2, -2 },
                    y_offset: new double[] { 0, 0, 0, 0, 0, 0 },
                    offset:   new double[] { 1, 1, 1, 1, 1, 1 },
                    scale:    new double[] { 6, 1, 1, 1, 1, 1 }
                ),
                new L2NormCorrection(
                    distance: flow0distances,
                    x_offset: new double[] { 0, -0.8, -0.9, -1, -1, -1 },
                    y_offset: new double[] { 0, 0.5, 0.75, 1, 2, 2 },
                    offset:   new double[] { 1, 0.5, 0.4, 0.3, 0, 0 },
                    scale:    new double[] { 3, 0.7, 0.7, 0.7, 1, 1 }
                ),
                new L2NormCorrection(
                    distance: flow0distances,
                    x_offset: new double[] { 0, -0.8, -0.9, -1, -1, -1 },
                    y_offset: new double[] { 0, -0.5, -0.75, -1, -2, -2 },
                    offset:   new double[] { 1, 0.5, 0.4, 0.3, 0, 0 },
                    scale:    new double[] { 3, 0.7, 0.7, 0.7, 1, 1 }
                ),
                new L2NormCorrection(
                    distance: flow0distances,
                    x_offset: new double[] { 0, 0, 0, 0, 0, 0 },
                    y_offset: new double[] { 0, 0.95, 0.975, 1, 0, 0 },
                    offset:   new double[] { 0, 0, 0, 0, 0, 0 },
                    scale:    new double[] { 0, 0.7, 0.55, 0.4, 0, 0 }
                ),
                new L2NormCorrection(
                    distance: flow0distances,
                    x_offset: new double[] { 0, 0, 0, 0, 0, 0 },
                    y_offset: new double[] { 0, -0.95, -0.975, -1, 0, 0 },
                    offset:   new double[] { 0, 0, 0, 0, 0, 0 },
                    scale:    new double[] { 0, 0.7, 0.55, 0.4, 0, 0 }
                )
            });
        public static readonly AngleCorrection FLOW_0_REPLACEMENT = new AngleCorrection(FLOW_0_OLD, flowdistances, flowdistances, angles, "FLOW_0");

        private static readonly double[] snap0distances = { 0, 1.5, 2.5, 4, 6, 8 };
        public static readonly MultiL2NormCorrection SNAP_0_OLD = new MultiL2NormCorrection(
            distance: snap0distances,
            offset: new double[] { -1, -5, -6.7, -6.5, -4.3, -4.3 },
            scale: new double[] { 1, 0.85, 0.6, 0.8, 1, 1 },
            components: new[] {
                new L2NormCorrection(
                    distance: snap0distances,
                    x_offset: new double[] { 0.5, 2, 2.8, 5, 5, 5 },
                    y_offset: new double[] { 0, 0, 0, 0, 0, 0 },
                    offset:   new double[] { 1, 1, 1, 0, 0, 0 },
                    scale:    new double[] { 0.6, 1, 0.8, 0.6, 0.2, 0.2 }
                ),
                new L2NormCorrection(
                    distance: snap0distances,
                    x_offset: new double[] { 0.25, 1, 0.7, 2, 2, 2 },
                    y_offset: new double[] { 0.5, 2, 2.8, 4, 6, 6 },
                    offset:   new double[] { 1, 1, 1, 1, 1, 1 },
                    scale:    new double[] { 0.6, 1, 0.8, 0.3, 0.2, 0.2 }
                ),
                new L2NormCorrection(
                    distance: snap0distances,
                    x_offset: new double[] { 0.25, 1, 0.7, 2, 2, 2 },
                    y_offset: new double[] { -0.5, -2, -2.8, -4, -6, -6 },
                    offset:   new double[] { 1, 1, 1, 1, 1, 1 },
                    scale:    new double[] { 0.6, 1, 0.8, 0.3, 0.2, 0.2 }
                ),
                new L2NormCorrection(
                    distance: snap0distances,
                    x_offset: new double[] { 0, 0, -0.5, -2, -3, -3 },
                    y_offset: new double[] { 0, 0, 0, 0, 0, 0 },
                    offset:   new double[] { 1, 1, 1, 1, 1, 1 },
                    scale:    new double[] { -0.7, -1, -0.9, -0.1, -0.1, -0.1 }
                )
            });
        public static readonly AngleCorrection SNAP_0_REPLACEMENT = new AngleCorrection(SNAP_0_OLD, snapdistances, snapdistances, angles, "SNAP_0");

        private static readonly double[] flow3distances = { 0, 1, 2, 3, 4 };
        public static readonly MultiL2NormCorrection FLOW_3_OLD = new MultiL2NormCorrection(
            distance: flow3distances,
            offset: new double[] { -4, -5.3, -5.2, -2.5, -2.5 },
            scale: new double[] { 1, 1, 1, 1, 1 },
            components: new[] {
                new L2NormCorrection(
                    distance: flow3distances,
                    x_offset: new double[] { 0, 1.2, 2, 2, 2 },
                    y_offset: new double[] { 0, 0, 0, 0, 0 },
                    offset:   new double[] { 0, 0, 0, 0, 0 },
                    scale:    new double[] { 1.5, 1, 0.4, 0, 0 }
                ),
                new L2NormCorrection(
                    distance: flow3distances,
                    x_offset: new double[] { 0, 0, 0, 0, 0 },
                    y_offset: new double[] { 0, 0, 0, 0, 0 },
                    offset:   new double[] { 0, 0, 0, 0, 0 },
                    scale:    new double[] { 2, 1.5, 2.5, 3.5, 3.5 }
                ),
                new L2NormCorrection(
                    distance: flow3distances,
                    x_offset: new double[] { 0, 0.3, 0.6, 0.6, 0.6 },
                    y_offset: new double[] { 0, 1, 2.4, 2.4, 2.4 },
                    offset:   new double[] { 0, 0, 0, 0, 0 },
                    scale:    new double[] { 0, 0.4, 0.4, 0, 0 }
                ),
                new L2NormCorrection(
                    distance: flow3distances,
                    x_offset: new double[] { 0, 0.3, 0.6, 0.6, 0.6 },
                    y_offset: new double[] { 0, -1, -2.4, -2.4, -2.4 },
                    offset:   new double[] { 0, 0, 0, 0, 0 },
                    scale:    new double[] { 0, 0.4, 0.4, 0, 0 }
                ),
            });
        public static readonly AngleCorrection FLOW_3_REPLACEMENT = new AngleCorrection(FLOW_3_OLD, flowdistances, flowdistances, angles, "FLOW_3");

        private static readonly double[] snap3distances = { 1, 1.5, 2.5, 4, 6, 8 };
        public static readonly MultiL2NormCorrection SNAP_3_OLD = new MultiL2NormCorrection(
            distance: snap3distances,
            offset: new double[] { -2, -2, -3, -5.4, -4.9, -4.9 },
            scale: new double[] { 1, 1, 1, 1, 1, 1 },
            components: new[] {
                new L2NormCorrection(
                    distance: snap3distances,
                    x_offset: new double[] { -2, -2, -3, -4, -6, -6 },
                    y_offset: new double[] { 0, 0, 0, 0, 0, 0 },
                    offset:   new double[] { 1, 1, 1, 0, 0, 0 },
                    scale:    new double[] { 0.4, 0.4, 0.2, 0.4, 0.3, 0.3 }
                ),
                new L2NormCorrection(
                    distance: snap3distances,
                    x_offset: new double[] { -1, -1, -1.5, -2, -3, -3 },
                    y_offset: new double[] { 1.4, 1.4, 2.1, 2, 3, 3 },
                    offset:   new double[] { 1, 1, 1, 1, 1, 1 },
                    scale:    new double[] { 0.4, 0.4, 0.2, 0.4, 0.2, 0.2 }
                ),
                new L2NormCorrection(
                    distance: snap3distances,
                    x_offset: new double[] { -1, -1, -1.5, -2, -3, -3 },
                    y_offset: new double[] { -1.4, -1.4, -2.1, -2, -3, -3 },
                    offset:   new double[] { 1, 1, 1, 1, 1, 1 },
                    scale:    new double[] { 0.4, 0.4, 0.2, 0.4, 0.2, 0.2 }
                ),
                new L2NormCorrection(
                    distance: snap3distances,
                    x_offset: new double[] { 0, 0, 0, 0, 0, 0 },
                    y_offset: new double[] { 0, 0, 0, 0, 0, 0 },
                    offset:   new double[] { 0, 0, 0, 0, 0, 0 },
                    scale:    new double[] { 0, 0, 1, 0.6, 0.6, 0.6 }
                ),
                new L2NormCorrection(
                    distance: snap3distances,
                    x_offset: new double[] { 1, 1, 1.5, 2, 3, 3 },
                    y_offset: new double[] { 0, 0, 0, 0, 0, 0 },
                    offset:   new double[] { 1, 1, 1, 1, 1, 1 },
                    scale:    new double[] { 0, 0, -0.6, -0.4, -0.3, -0.3 }
                )
            });
        public static readonly AngleCorrection SNAP_3_REPLACEMENT = new AngleCorrection(SNAP_3_OLD, snapdistances, snapdistances, angles, "SNAP_3");


    }
}
