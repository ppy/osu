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

        /// <summary>
        /// Counts the number of segments in a slider path.
        /// </summary>
        /// <param name="controlPoints">The control points of the path.</param>
        /// <returns>The number of segments in a slider path.</returns>
        public static int CountSegments(IList<PathControlPoint> controlPoints) => controlPoints.Where((t, i) => t.Type != null && i < controlPoints.Count - 1).Count();

        /// <summary>
        /// Converts a slider path to bezier control point positions compatible with the legacy osu! client.
        /// </summary>
        /// <param name="controlPoints">The control points of the path.</param>
        /// <param name="position">The offset for the whole path.</param>
        /// <returns>The list of legacy bezier control point positions.</returns>
        public static List<Vector2> ConvertToLegacyBezier(IList<PathControlPoint> controlPoints, Vector2 position)
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
                var segmentType = controlPoints[start].Type ?? PathType.LINEAR;

                switch (segmentType.Type)
                {
                    case SplineType.Catmull:
                        result.AddRange(from segment in ConvertCatmullToBezierAnchors(segmentVertices) from v in segment select v + position);
                        break;

                    case SplineType.Linear:
                        result.AddRange(from segment in ConvertLinearToBezierAnchors(segmentVertices) from v in segment select v + position);
                        break;

                    case SplineType.PerfectCurve:
                        result.AddRange(ConvertCircleToBezierAnchors(segmentVertices).Select(v => v + position));
                        break;

                    case SplineType.BSpline:
                        if (segmentType.Degree != null)
                            throw new NotImplementedException("BSpline conversion of arbitrary degree is not implemented.");

                        foreach (Vector2 v in segmentVertices)
                        {
                            result.Add(v + position);
                        }

                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(segmentType.Type), segmentType.Type, "Unsupported segment type found when converting to legacy Bezier");
                }

                // Start the new segment at the current vertex
                start = i;
            }

            return result;
        }

        /// <summary>
        /// Converts a path of control points to an identical path using only BEZIER type control points.
        /// </summary>
        /// <param name="controlPoints">The control points of the path.</param>
        /// <returns>The list of bezier control points.</returns>
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
                var segmentType = controlPoints[start].Type ?? PathType.LINEAR;

                switch (segmentType.Type)
                {
                    case SplineType.Catmull:
                        foreach (var segment in ConvertCatmullToBezierAnchors(segmentVertices))
                        {
                            for (int j = 0; j < segment.Length - 1; j++)
                            {
                                result.Add(new PathControlPoint(segment[j], j == 0 ? PathType.BEZIER : null));
                            }
                        }

                        break;

                    case SplineType.Linear:
                        foreach (var segment in ConvertLinearToBezierAnchors(segmentVertices))
                        {
                            for (int j = 0; j < segment.Length - 1; j++)
                            {
                                result.Add(new PathControlPoint(segment[j], j == 0 ? PathType.BEZIER : null));
                            }
                        }

                        break;

                    case SplineType.PerfectCurve:
                        var circleResult = ConvertCircleToBezierAnchors(segmentVertices);

                        for (int j = 0; j < circleResult.Length - 1; j++)
                        {
                            result.Add(new PathControlPoint(circleResult[j], j == 0 ? PathType.BEZIER : null));
                        }

                        break;

                    case SplineType.BSpline:
                        for (int j = 0; j < segmentVertices.Length - 1; j++)
                        {
                            result.Add(new PathControlPoint(segmentVertices[j], j == 0 ? segmentType : null));
                        }

                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(segmentType.Type), segmentType.Type, "Unsupported segment type found when converting to legacy Bezier");
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
            if (controlPoints.Length != 3)
                return controlPoints.ToArray();

            var pr = new CircularArcProperties(controlPoints);
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
