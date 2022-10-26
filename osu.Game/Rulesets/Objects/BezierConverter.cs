// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Utils;
using osu.Game.Rulesets.Objects.Types;
using osuTK;

namespace osu.Game.Rulesets.Objects
{
    public static class BezierConverter
    {
        private struct CircleBezierPreset
        {
            public readonly double ArcLength;
            public readonly Vector2d[] ControlPoints;

            public CircleBezierPreset(double arcLength, Vector2d[] controlPoints)
            {
                ArcLength = arcLength;
                ControlPoints = controlPoints;
            }
        }

        // Extremely accurate a bezier anchor positions for approximating circles of several arc lengths
        private static readonly CircleBezierPreset[] circle_presets =
        {
            new CircleBezierPreset(0.4993379862754501,
                new[] { new Vector2d(1, 0), new Vector2d(1, 0.2549893626632736f), new Vector2d(0.8778997558480327f, 0.47884446188920726f) }),
            new CircleBezierPreset(1.7579419829169447,
                new[] { new Vector2d(1, 0), new Vector2d(1, 0.6263026f), new Vector2d(0.42931178f, 1.0990661f), new Vector2d(-0.18605515f, 0.9825393f) }),
            new CircleBezierPreset(3.1385246920140215,
                new[] { new Vector2d(1, 0), new Vector2d(1, 0.87084764f), new Vector2d(0.002304826f, 1.5033062f), new Vector2d(-0.9973236f, 0.8739115f), new Vector2d(-0.9999953f, 0.0030679568f) }),
            new CircleBezierPreset(5.69720464620727,
                new[] { new Vector2d(1, 0), new Vector2d(1, 1.4137783f), new Vector2d(-1.4305235f, 2.0779421f), new Vector2d(-2.3410065f, -0.94017583f), new Vector2d(0.05132711f, -1.7309346f), new Vector2d(0.8331702f, -0.5530167f) }),
            new CircleBezierPreset(2 * Math.PI,
                new[] { new Vector2d(1, 0), new Vector2d(1, 1.2447058f), new Vector2d(-0.8526471f, 2.118367f), new Vector2d(-2.6211002f, 7.854936e-06f), new Vector2d(-0.8526448f, -2.118357f), new Vector2d(1, -1.2447058f), new Vector2d(1, 0) })
        };

        #region CircularArcProperties

        //TODO: Get this from osu!framework instead
        public readonly struct CircularArcProperties
        {
            public readonly bool IsValid;
            public readonly double ThetaStart;
            public readonly double ThetaRange;
            public readonly double Direction;
            public readonly float Radius;
            public readonly Vector2 Centre;

            public double ThetaEnd => ThetaStart + ThetaRange * Direction;

            public CircularArcProperties(double thetaStart, double thetaRange, double direction, float radius, Vector2 centre)
            {
                IsValid = true;
                ThetaStart = thetaStart;
                ThetaRange = thetaRange;
                Direction = direction;
                Radius = radius;
                Centre = centre;
            }
        }

        /// <summary>
        /// Computes various properties that can be used to approximate the circular arc.
        /// </summary>
        /// <param name="controlPoints">Three distinct points on the arc.</param>
        private static CircularArcProperties circularArcProperties(ReadOnlySpan<Vector2> controlPoints)
        {
            Vector2 a = controlPoints[0];
            Vector2 b = controlPoints[1];
            Vector2 c = controlPoints[2];

            // If we have a degenerate triangle where a side-length is almost zero, then give up and fallback to a more numerically stable method.
            if (Precision.AlmostEquals(0, (b.Y - a.Y) * (c.X - a.X) - (b.X - a.X) * (c.Y - a.Y)))
                return default; // Implicitly sets `IsValid` to false

            // See: https://en.wikipedia.org/wiki/Circumscribed_circle#Cartesian_coordinates_2
            float d = 2 * (a.X * (b - c).Y + b.X * (c - a).Y + c.X * (a - b).Y);
            float aSq = a.LengthSquared;
            float bSq = b.LengthSquared;
            float cSq = c.LengthSquared;

            Vector2 centre = new Vector2(
                aSq * (b - c).Y + bSq * (c - a).Y + cSq * (a - b).Y,
                aSq * (c - b).X + bSq * (a - c).X + cSq * (b - a).X) / d;

            Vector2 dA = a - centre;
            Vector2 dC = c - centre;

            float r = dA.Length;

            double thetaStart = Math.Atan2(dA.Y, dA.X);
            double thetaEnd = Math.Atan2(dC.Y, dC.X);

            while (thetaEnd < thetaStart)
                thetaEnd += 2 * Math.PI;

            double dir = 1;
            double thetaRange = thetaEnd - thetaStart;

            // Decide in which direction to draw the circle, depending on which side of
            // AC B lies.
            Vector2 orthoAtoC = c - a;
            orthoAtoC = new Vector2(orthoAtoC.Y, -orthoAtoC.X);

            if (Vector2.Dot(orthoAtoC, b - a) < 0)
            {
                dir = -dir;
                thetaRange = 2 * Math.PI - thetaRange;
            }

            return new CircularArcProperties(thetaStart, thetaRange, dir, r, centre);
        }

        #endregion

        public static IEnumerable<Vector2> ConvertToLegacyBezier(IList<PathControlPoint> controlPoints, Vector2 position)
        {
            Vector2[] vertices = new Vector2[controlPoints.Count];
            for (int i = 0; i < controlPoints.Count; i++)
                vertices[i] = controlPoints[i].Position;

            var result = new List<Vector2>();
            int start = 0;

            for (int i = 0; i < controlPoints.Count; i++)
            {
                if (controlPoints[i].Type == null && i < controlPoints.Count - 1)
                    continue;

                // The current vertex ends the segment
                var segmentVertices = vertices.AsSpan().Slice(start, i - start + 1);
                var segmentType = controlPoints[start].Type ?? PathType.Linear;

                switch (segmentType)
                {
                    case PathType.Catmull:
                        result.AddRange(from segment in ConvertCatmullToBezierAnchors(segmentVertices) from v in segment select v + position);

                        break;

                    case PathType.Linear:
                        result.AddRange(from segment in ConvertLinearToBezierAnchors(segmentVertices) from v in segment select v + position);

                        break;

                    case PathType.PerfectCurve:
                        result.AddRange(ConvertCircleToBezierAnchors(segmentVertices).Select(v => v + position));

                        break;

                    default:
                        foreach (Vector2 v in segmentVertices)
                        {
                            result.Add(v + position);
                        }

                        break;
                }

                // Start the new segment at the current vertex
                start = i;
            }

            return result;
        }

        public static List<PathControlPoint> ConvertToModernBezier(IList<PathControlPoint> controlPoints)
        {
            Vector2[] vertices = new Vector2[controlPoints.Count];
            for (int i = 0; i < controlPoints.Count; i++)
                vertices[i] = controlPoints[i].Position;

            var result = new List<PathControlPoint>();
            int start = 0;

            for (int i = 0; i < controlPoints.Count; i++)
            {
                if (controlPoints[i].Type == null && i < controlPoints.Count - 1)
                    continue;

                // The current vertex ends the segment
                var segmentVertices = vertices.AsSpan().Slice(start, i - start + 1);
                var segmentType = controlPoints[start].Type ?? PathType.Linear;

                switch (segmentType)
                {
                    case PathType.Catmull:
                        foreach (var segment in ConvertCatmullToBezierAnchors(segmentVertices))
                        {
                            for (int j = 0; j < segment.Length - 1; j++)
                            {
                                result.Add(new PathControlPoint(segment[j], j == 0 ? PathType.Bezier : null));
                            }
                        }

                        break;

                    case PathType.Linear:
                        foreach (var segment in ConvertLinearToBezierAnchors(segmentVertices))
                        {
                            for (int j = 0; j < segment.Length - 1; j++)
                            {
                                result.Add(new PathControlPoint(segment[j], j == 0 ? PathType.Bezier : null));
                            }
                        }

                        break;

                    case PathType.PerfectCurve:
                        var circleResult = ConvertCircleToBezierAnchors(segmentVertices);

                        for (int j = 0; j < circleResult.Length - 1; j++)
                        {
                            result.Add(new PathControlPoint(circleResult[j], j == 0 ? PathType.Bezier : null));
                        }

                        break;

                    default:
                        for (int j = 0; j < segmentVertices.Length - 1; j++)
                        {
                            result.Add(new PathControlPoint(segmentVertices[j], j == 0 ? PathType.Bezier : null));
                        }

                        break;
                }

                // Start the new segment at the current vertex
                start = i;
            }

            result.Add(new PathControlPoint(controlPoints[^1].Position));

            return result;
        }

        /// <summary>
        /// Converts perfect curve anchors to bezier anchors.
        /// </summary>
        /// <param name="controlPoints">The control point positions to convert.</param>
        public static Vector2[] ConvertCircleToBezierAnchors(ReadOnlySpan<Vector2> controlPoints)
        {
            var pr = circularArcProperties(controlPoints);
            if (!pr.IsValid)
                return controlPoints.ToArray();

            CircleBezierPreset preset = circle_presets.Last();

            foreach (CircleBezierPreset cbp in circle_presets)
            {
                if (cbp.ArcLength < pr.ThetaRange) continue;

                preset = cbp;
                break;
            }

            double arcLength = preset.ArcLength;
            var arc = new Vector2d[preset.ControlPoints.Length];
            preset.ControlPoints.CopyTo(arc, 0);

            // Converge on arcLength of thetaRange
            int n = arc.Length - 1;
            double tf = pr.ThetaRange / arcLength;

            while (Math.Abs(tf - 1) > 1E-7)
            {
                for (int j = 0; j < n; j++)
                {
                    for (int i = n; i > j; i--)
                    {
                        arc[i] = arc[i] * tf + arc[i - 1] * (1 - tf);
                    }
                }

                arcLength = Math.Atan2(arc.Last()[1], arc.Last()[0]);

                if (arcLength < 0)
                {
                    arcLength += 2 * Math.PI;
                }

                tf = pr.ThetaRange / arcLength;
            }

            // Adjust rotation, radius, and position
            var result = new Vector2[arc.Length];

            for (int i = 0; i < arc.Length; i++)
            {
                result[i] = new Vector2(
                    (float)((Math.Cos(pr.ThetaStart) * arc[i].X + -Math.Sin(pr.ThetaStart) * pr.Direction * arc[i].Y) * pr.Radius + pr.Centre.X),
                    (float)((Math.Sin(pr.ThetaStart) * arc[i].X + Math.Cos(pr.ThetaStart) * pr.Direction * arc[i].Y) * pr.Radius + pr.Centre.Y));
            }

            return result;
        }

        /// <summary>
        /// Converts catmull anchors to bezier anchors.
        /// </summary>
        /// <param name="controlPoints">The control point positions to convert.</param>
        public static Vector2[][] ConvertCatmullToBezierAnchors(ReadOnlySpan<Vector2> controlPoints)
        {
            int iLen = controlPoints.Length;
            var bezier = new Vector2[iLen - 1][];

            for (int i = 0; i < iLen - 1; i++)
            {
                var v1 = i > 0 ? controlPoints[i - 1] : controlPoints[i];
                var v2 = controlPoints[i];
                var v3 = i < iLen - 1 ? controlPoints[i + 1] : v2 + v2 - v1;
                var v4 = i < iLen - 2 ? controlPoints[i + 2] : v3 + v3 - v2;

                bezier[i] = new[]
                {
                    v2,
                    (-v1 + 6 * v2 + v3) / 6,
                    (-v4 + 6 * v3 + v2) / 6,
                    v3
                };
            }

            return bezier;
        }

        /// <summary>
        /// Converts linear anchors to bezier anchors.
        /// </summary>
        /// <param name="controlPoints">The control point positions to convert.</param>
        public static Vector2[][] ConvertLinearToBezierAnchors(ReadOnlySpan<Vector2> controlPoints)
        {
            int iLen = controlPoints.Length;
            var bezier = new Vector2[iLen - 1][];

            for (int i = 0; i < iLen - 1; i++)
            {
                bezier[i] = new[]
                {
                    controlPoints[i],
                    controlPoints[i + 1]
                };
            }

            return bezier;
        }
    }
}
