// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Utils;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osuTK;

namespace osu.Game.Rulesets.Catch.Objects
{
    /// <summary>
    /// Represents the path of a juice stream.
    /// <para>
    /// A <see cref="JuiceStream"/> holds a legacy <see cref="SliderPath"/> as the representation of the path.
    /// However, the <see cref="SliderPath"/> representation is difficult to work with.
    /// This <see cref="JuiceStreamPath"/> represents the path in a more convenient way, a polyline connecting list of <see cref="JuiceStreamPathVertex"/>s.
    /// </para>
    /// </summary>
    public class JuiceStreamPath
    {
        /// <summary>
        /// The height of legacy osu!standard playfield.
        /// The sliders converted by <see cref="ConvertToSliderPath"/> are vertically contained in this height.
        /// </summary>
        internal const float OSU_PLAYFIELD_HEIGHT = 384;

        /// <summary>
        /// The list of vertices of the path, which is represented as a polyline connecting the vertices.
        /// </summary>
        public IReadOnlyList<JuiceStreamPathVertex> Vertices => vertices;

        /// <summary>
        /// The current version number.
        /// This starts from <c>1</c> and incremented whenever this <see cref="JuiceStreamPath"/> is modified.
        /// </summary>
        public int InvalidationID { get; private set; } = 1;

        /// <summary>
        /// The difference between first vertex's <see cref="JuiceStreamPathVertex.Time"/> and last vertex's <see cref="JuiceStreamPathVertex.Time"/>.
        /// </summary>
        public double Duration => vertices[^1].Time - vertices[0].Time;

        /// <remarks>
        /// This list should always be non-empty.
        /// </remarks>
        private readonly List<JuiceStreamPathVertex> vertices = new List<JuiceStreamPathVertex>
        {
            new JuiceStreamPathVertex()
        };

        /// <summary>
        /// Compute the x-position of the path at the given <paramref name="time"/>.
        /// </summary>
        /// <remarks>
        /// When the given time is outside of the path, the x position at the corresponding endpoint is returned,
        /// </remarks>
        public float PositionAtTime(double time)
        {
            int index = vertexIndexAtTime(time);
            return positionAtTime(time, index);
        }

        /// <summary>
        /// Remove all vertices of this path, then add a new vertex <c>(0, 0)</c>.
        /// </summary>
        public void Clear()
        {
            vertices.Clear();
            vertices.Add(new JuiceStreamPathVertex());
            invalidate();
        }

        /// <summary>
        /// Insert a vertex at given <paramref name="time"/>.
        /// The <see cref="PositionAtTime"/> is used as the position of the new vertex.
        /// Thus, the set of points of the path is not changed (up to floating-point precision).
        /// </summary>
        /// <returns>The index of the new vertex.</returns>
        public int InsertVertex(double time)
        {
            if (!double.IsFinite(time))
                throw new ArgumentOutOfRangeException(nameof(time));

            int index = vertexIndexAtTime(time);
            float x = positionAtTime(time, index);
            vertices.Insert(index, new JuiceStreamPathVertex(time, x));

            invalidate();
            return index;
        }

        /// <summary>
        /// Move the vertex of given <paramref name="index"/> to the given position <paramref name="newX"/>.
        /// </summary>
        public void SetVertexPosition(int index, float newX)
        {
            if (index < 0 || index >= vertices.Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            if (!float.IsFinite(newX))
                throw new ArgumentOutOfRangeException(nameof(newX));

            vertices[index] = new JuiceStreamPathVertex(vertices[index].Time, newX);

            invalidate();
        }

        /// <summary>
        /// Add a new vertex at given <paramref name="time"/> and position.
        /// </summary>
        public void Add(double time, float x)
        {
            int index = InsertVertex(time);
            SetVertexPosition(index, x);
        }

        /// <summary>
        /// Remove all vertices that satisfy the given <paramref name="predicate"/>.
        /// </summary>
        /// <remarks>
        /// If all vertices are removed, a new vertex <c>(0, 0)</c> is added.
        /// </remarks>
        /// <param name="predicate">The predicate to determine whether a vertex should be removed given the vertex and its index in the path.</param>
        /// <returns>The number of removed vertices.</returns>
        public int RemoveVertices(Func<JuiceStreamPathVertex, int, bool> predicate)
        {
            int index = 0;
            int removeCount = vertices.RemoveAll(vertex => predicate(vertex, index++));

            if (vertices.Count == 0)
                vertices.Add(new JuiceStreamPathVertex());

            if (removeCount != 0)
                invalidate();

            return removeCount;
        }

        /// <summary>
        /// Recreate this path by using difference set of vertices at given time points.
        /// In addition to the given <paramref name="sampleTimes"/>, the first vertex and the last vertex are always added to the new path.
        /// New vertices use the positions on the original path. Thus, <see cref="PositionAtTime"/>s at <paramref name="sampleTimes"/> are preserved.
        /// </summary>
        public void ResampleVertices(IEnumerable<double> sampleTimes)
        {
            var sampledVertices = new List<JuiceStreamPathVertex>();

            foreach (double time in sampleTimes)
            {
                if (!double.IsFinite(time))
                    throw new ArgumentOutOfRangeException(nameof(sampleTimes));

                double clampedTime = Math.Clamp(time, vertices[0].Time, vertices[^1].Time);
                float x = PositionAtTime(clampedTime);
                sampledVertices.Add(new JuiceStreamPathVertex(clampedTime, x));
            }

            sampledVertices.Sort();

            // The first vertex and the last vertex are always used in the result.
            vertices.RemoveRange(1, vertices.Count - (vertices.Count == 1 ? 1 : 2));
            vertices.InsertRange(1, sampledVertices);

            invalidate();
        }

        /// <summary>
        /// Convert a <see cref="SliderPath"/> to list of vertices and write the result to this <see cref="JuiceStreamPath"/>.
        /// </summary>
        /// <remarks>
        /// Duplicated vertices are automatically removed.
        /// </remarks>
        public void ConvertFromSliderPath(SliderPath sliderPath, double velocity)
        {
            var sliderPathVertices = new List<Vector2>();
            sliderPath.GetPathToProgress(sliderPathVertices, 0, 1);

            double time = 0;

            vertices.Clear();
            vertices.Add(new JuiceStreamPathVertex(0, sliderPathVertices.FirstOrDefault().X));

            for (int i = 1; i < sliderPathVertices.Count; i++)
            {
                time += Vector2.Distance(sliderPathVertices[i - 1], sliderPathVertices[i]) / velocity;

                if (!Precision.AlmostEquals(vertices[^1].Time, time))
                    Add(time, sliderPathVertices[i].X);
            }

            invalidate();
        }

        /// <summary>
        /// Computes the minimum slider velocity required to convert this path to a <see cref="SliderPath"/>.
        /// </summary>
        public double ComputeRequiredVelocity()
        {
            double maximumSlope = 0;

            for (int i = 1; i < vertices.Count; i++)
            {
                double xDifference = Math.Abs((double)vertices[i].X - vertices[i - 1].X);
                double timeDifference = vertices[i].Time - vertices[i - 1].Time;

                // A short segment won't affect the resulting path much anyways so ignore it to avoid divide-by-zero.
                if (Precision.AlmostEquals(timeDifference, 0))
                    continue;

                maximumSlope = Math.Max(maximumSlope, xDifference / timeDifference);
            }

            return maximumSlope;
        }

        /// <summary>
        /// Convert the path of this <see cref="JuiceStreamPath"/> to a <see cref="SliderPath"/> and write the result to <paramref name="sliderPath"/>.
        /// The resulting slider is "folded" to make it vertically contained in the playfield `(0..<see cref="OSU_PLAYFIELD_HEIGHT"/>)` assuming the slider start position is <paramref name="sliderStartY"/>.
        ///
        /// The velocity of the converted slider is assumed to be <paramref name="velocity"/>.
        /// To preserve the path, <paramref name="velocity"/> should be at least the value returned by <see cref="ComputeRequiredVelocity"/>.
        /// </summary>
        public void ConvertToSliderPath(SliderPath sliderPath, float sliderStartY, double velocity)
        {
            const float margin = 1;

            // Note: these two variables and `sliderPath` are modified by the local functions.
            double currentTime = 0;
            Vector2 lastPosition = new Vector2(vertices[0].X, 0);

            sliderPath.ControlPoints.Clear();
            sliderPath.ControlPoints.Add(new PathControlPoint(lastPosition));

            for (int i = 1; i < vertices.Count; i++)
            {
                sliderPath.ControlPoints[^1].Type = PathType.LINEAR;

                float deltaX = vertices[i].X - lastPosition.X;
                double length = (vertices[i].Time - currentTime) * velocity;

                // Should satisfy `deltaX^2 + deltaY^2 = length^2`.
                // The expression inside the `sqrt` is (almost) non-negative if the slider velocity is large enough.
                double deltaY = Math.Sqrt(Math.Max(0, length * length - (double)deltaX * deltaX));

                // When `deltaY` is small, one segment is always enough.
                // This case is handled separately to prevent divide-by-zero.
                if (deltaY <= OSU_PLAYFIELD_HEIGHT / 2 - margin)
                {
                    float nextX = vertices[i].X;
                    float nextY = (float)(lastPosition.Y + getYDirection() * deltaY);
                    addControlPoint(nextX, nextY);
                    continue;
                }

                // When `deltaY` is large or when the slider velocity is fast, the segment must be partitioned to subsegments to stay in bounds.
                for (double currentProgress = 0; currentProgress < deltaY;)
                {
                    double nextProgress = Math.Min(currentProgress + getMaxDeltaY(), deltaY);
                    float nextX = (float)(vertices[i - 1].X + nextProgress / deltaY * deltaX);
                    float nextY = (float)(lastPosition.Y + getYDirection() * (nextProgress - currentProgress));
                    addControlPoint(nextX, nextY);
                    currentProgress = nextProgress;
                }
            }

            int getYDirection()
            {
                float lastSliderY = sliderStartY + lastPosition.Y;
                return lastSliderY < OSU_PLAYFIELD_HEIGHT / 2 ? 1 : -1;
            }

            float getMaxDeltaY()
            {
                float lastSliderY = sliderStartY + lastPosition.Y;
                return Math.Max(lastSliderY, OSU_PLAYFIELD_HEIGHT - lastSliderY) - margin;
            }

            void addControlPoint(float nextX, float nextY)
            {
                Vector2 nextPosition = new Vector2(nextX, nextY);
                sliderPath.ControlPoints.Add(new PathControlPoint(nextPosition));
                currentTime += Vector2.Distance(lastPosition, nextPosition) / velocity;
                lastPosition = nextPosition;
            }
        }

        /// <summary>
        /// Find the index at which a new vertex with <paramref name="time"/> can be inserted.
        /// </summary>
        private int vertexIndexAtTime(double time)
        {
            // The position of `(time, Infinity)` is uniquely determined because infinite positions are not allowed.
            int i = vertices.BinarySearch(new JuiceStreamPathVertex(time, float.PositiveInfinity));
            return i < 0 ? ~i : i;
        }

        /// <summary>
        /// Compute the position at the given <paramref name="time"/>, assuming <paramref name="index"/> is the vertex index returned by <see cref="vertexIndexAtTime"/>.
        /// </summary>
        private float positionAtTime(double time, int index)
        {
            if (index <= 0)
                return vertices[0].X;
            if (index >= vertices.Count)
                return vertices[^1].X;

            double duration = vertices[index].Time - vertices[index - 1].Time;
            if (Precision.AlmostEquals(duration, 0))
                return vertices[index].X;

            float deltaX = vertices[index].X - vertices[index - 1].X;

            return (float)(vertices[index - 1].X + deltaX * ((time - vertices[index - 1].Time) / duration));
        }

        private void invalidate() => InvalidationID++;
    }
}
