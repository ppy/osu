// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using osu.Framework.MathUtils;
using osu.Game.Rulesets.Objects.Types;
using osuTK;

namespace osu.Game.Rulesets.Objects
{
    public readonly struct PathSegment : IEquatable<PathSegment>
    {
        [JsonProperty]
        public readonly PathType Type;

        [JsonProperty]
        private readonly Vector2[] controlPoints;

        public PathSegment(PathType type, Vector2[] controlPoints)
        {
            if (type == PathType.PerfectCurve && controlPoints.Length != 2)
                throw new ArgumentException($"A {nameof(PathType.PerfectCurve)} must have exactly 2 auxiliary control points.");

            Type = type;
            this.controlPoints = controlPoints;
        }

        public ReadOnlySpan<Vector2> ControlPoints => controlPoints;

        public List<Vector2> ComputePath(Vector2 referencePoint)
        {
            // stackalloc isn't used as controlPoints can be of arbitrary length and may overflow the stack
            var fullControlPoints = new Vector2[controlPoints.Length + 1];
            controlPoints.CopyTo(fullControlPoints.AsSpan().Slice(1));
            fullControlPoints[0] = referencePoint;

            switch (Type)
            {
                case PathType.Linear:
                    return PathApproximator.ApproximateLinear(fullControlPoints);

                case PathType.PerfectCurve:
                    List<Vector2> subpath = PathApproximator.ApproximateCircularArc(fullControlPoints);

                    // If for some reason a circular arc could not be fit to the 3 given points, fall back to a numerically stable bezier approximation.
                    if (subpath.Count == 0)
                        break;

                    return subpath;

                case PathType.Catmull:
                    return PathApproximator.ApproximateCatmull(fullControlPoints);
            }

            return PathApproximator.ApproximateBezier(fullControlPoints);
        }

        public bool Equals(PathSegment other) => Type == other.Type && ControlPoints.SequenceEqual(other.ControlPoints);
    }
}
