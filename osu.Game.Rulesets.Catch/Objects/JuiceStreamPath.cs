// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Utils;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osuTK;

#nullable enable

namespace osu.Game.Rulesets.Catch.Objects
{
    /// <summary>
    /// Represents the path of a juice stream.
    /// <para>
    /// A <see cref="JuiceStream"/> holds a legacy <see cref="SliderPath"/> as the representation of the path.
    /// However, the <see cref="SliderPath"/> representation is difficult to work with.
    /// This <see cref="JuiceStreamPath"/> represents the path in a more convenient way, a polyline connecting list of <see cref="JuiceStreamPathVertex"/>s.
    /// </para>
    /// <para>
    /// The path can be regarded as a function from the closed interval <c>[Vertices[0].Distance, Vertices[^1].Distance]</c> to the x position, given by <see cref="PositionAtDistance"/>.
    /// To ensure the path is convertible to a <see cref="SliderPath"/>, the slope of the function must not be more than <c>1</c> everywhere,
    /// and this slope condition is always maintained as an invariant.
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
        /// The difference between first vertex's <see cref="JuiceStreamPathVertex.Distance"/> and last vertex's <see cref="JuiceStreamPathVertex.Distance"/>.
        /// </summary>
        public double Distance => vertices[^1].Distance - vertices[0].Distance;

        /// <remarks>
        /// This list should always be non-empty.
        /// </remarks>
        private readonly List<JuiceStreamPathVertex> vertices = new List<JuiceStreamPathVertex>
        {
            new JuiceStreamPathVertex()
        };

        /// <summary>
        /// Compute the x-position of the path at the given <paramref name="distance"/>.
        /// </summary>
        /// <remarks>
        /// When the given distance is outside of the path, the x position at the corresponding endpoint is returned,
        /// </remarks>
        public float PositionAtDistance(double distance)
        {
            int index = vertexIndexAtDistance(distance);
            return positionAtDistance(distance, index);
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
        /// Insert a vertex at given <paramref name="distance"/>.
        /// The <see cref="PositionAtDistance"/> is used as the position of the new vertex.
        /// Thus, the set of points of the path is not changed (up to floating-point precision).
        /// </summary>
        /// <returns>The index of the new vertex.</returns>
        public int InsertVertex(double distance)
        {
            if (!double.IsFinite(distance))
                throw new ArgumentOutOfRangeException(nameof(distance));

            int index = vertexIndexAtDistance(distance);
            float x = positionAtDistance(distance, index);
            vertices.Insert(index, new JuiceStreamPathVertex(distance, x));

            invalidate();
            return index;
        }

        /// <summary>
        /// Move the vertex of given <paramref name="index"/> to the given position <paramref name="newX"/>.
        /// When the distances between vertices are too small for the new vertex positions, the adjacent vertices are moved towards <paramref name="newX"/>.
        /// </summary>
        public void SetVertexPosition(int index, float newX)
        {
            if (index < 0 || index >= vertices.Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            if (!float.IsFinite(newX))
                throw new ArgumentOutOfRangeException(nameof(newX));

            var newVertex = new JuiceStreamPathVertex(vertices[index].Distance, newX);

            for (int i = index - 1; i >= 0 && !canConnect(vertices[i], newVertex); i--)
            {
                float clampedX = clampToConnectablePosition(newVertex, vertices[i]);
                vertices[i] = new JuiceStreamPathVertex(vertices[i].Distance, clampedX);
            }

            for (int i = index + 1; i < vertices.Count; i++)
            {
                float clampedX = clampToConnectablePosition(newVertex, vertices[i]);
                vertices[i] = new JuiceStreamPathVertex(vertices[i].Distance, clampedX);
            }

            vertices[index] = newVertex;

            invalidate();
        }

        /// <summary>
        /// Add a new vertex at given <paramref name="distance"/> and position.
        /// Adjacent vertices are moved when necessary in the same way as <see cref="SetVertexPosition"/>.
        /// </summary>
        public void Add(double distance, float x)
        {
            int index = InsertVertex(distance);
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
        /// Recreate this path by using difference set of vertices at given distances.
        /// In addition to the given <paramref name="sampleDistances"/>, the first vertex and the last vertex are always added to the new path.
        /// New vertices use the positions on the original path. Thus, <see cref="PositionAtDistance"/>s at <paramref name="sampleDistances"/> are preserved.
        /// </summary>
        public void ResampleVertices(IEnumerable<double> sampleDistances)
        {
            var sampledVertices = new List<JuiceStreamPathVertex>();

            foreach (double distance in sampleDistances)
            {
                if (!double.IsFinite(distance))
                    throw new ArgumentOutOfRangeException(nameof(sampleDistances));

                double clampedDistance = Math.Clamp(distance, vertices[0].Distance, vertices[^1].Distance);
                float x = PositionAtDistance(clampedDistance);
                sampledVertices.Add(new JuiceStreamPathVertex(clampedDistance, x));
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
        public void ConvertFromSliderPath(SliderPath sliderPath)
        {
            var sliderPathVertices = new List<Vector2>();
            sliderPath.GetPathToProgress(sliderPathVertices, 0, 1);

            double distance = 0;

            vertices.Clear();
            vertices.Add(new JuiceStreamPathVertex(0, sliderPathVertices.FirstOrDefault().X));

            for (int i = 1; i < sliderPathVertices.Count; i++)
            {
                distance += Vector2.Distance(sliderPathVertices[i - 1], sliderPathVertices[i]);

                if (!Precision.AlmostEquals(vertices[^1].Distance, distance))
                    vertices.Add(new JuiceStreamPathVertex(distance, sliderPathVertices[i].X));
            }

            invalidate();
        }

        /// <summary>
        /// Convert the path of this <see cref="JuiceStreamPath"/> to a <see cref="SliderPath"/> and write the result to <paramref name="sliderPath"/>.
        /// The resulting slider is "folded" to make it vertically contained in the playfield `(0..<see cref="OSU_PLAYFIELD_HEIGHT"/>)` assuming the slider start position is <paramref name="sliderStartY"/>.
        /// </summary>
        public void ConvertToSliderPath(SliderPath sliderPath, float sliderStartY)
        {
            const float margin = 1;

            // Note: these two variables and `sliderPath` are modified by the local functions.
            double currentDistance = 0;
            Vector2 lastPosition = new Vector2(vertices[0].X, 0);

            sliderPath.ControlPoints.Clear();
            sliderPath.ControlPoints.Add(new PathControlPoint(lastPosition));

            for (int i = 1; i < vertices.Count; i++)
            {
                sliderPath.ControlPoints[^1].Type = PathType.Linear;

                float deltaX = vertices[i].X - lastPosition.X;
                double length = vertices[i].Distance - currentDistance;

                // Should satisfy `deltaX^2 + deltaY^2 = length^2`.
                // By invariants, the expression inside the `sqrt` is (almost) non-negative.
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
                currentDistance += Vector2.Distance(lastPosition, nextPosition);
                lastPosition = nextPosition;
            }
        }

        /// <summary>
        /// Find the index at which a new vertex with <paramref name="distance"/> can be inserted.
        /// </summary>
        private int vertexIndexAtDistance(double distance)
        {
            // The position of `(distance, Infinity)` is uniquely determined because infinite positions are not allowed.
            int i = vertices.BinarySearch(new JuiceStreamPathVertex(distance, float.PositiveInfinity));
            return i < 0 ? ~i : i;
        }

        /// <summary>
        /// Compute the position at the given <paramref name="distance"/>, assuming <paramref name="index"/> is the vertex index returned by <see cref="vertexIndexAtDistance"/>.
        /// </summary>
        private float positionAtDistance(double distance, int index)
        {
            if (index <= 0)
                return vertices[0].X;
            if (index >= vertices.Count)
                return vertices[^1].X;

            double length = vertices[index].Distance - vertices[index - 1].Distance;
            if (Precision.AlmostEquals(length, 0))
                return vertices[index].X;

            float deltaX = vertices[index].X - vertices[index - 1].X;

            return (float)(vertices[index - 1].X + deltaX * ((distance - vertices[index - 1].Distance) / length));
        }

        /// <summary>
        /// Check the two vertices can connected directly while satisfying the slope condition.
        /// </summary>
        private bool canConnect(JuiceStreamPathVertex vertex1, JuiceStreamPathVertex vertex2, float allowance = 0)
        {
            double xDistance = Math.Abs((double)vertex2.X - vertex1.X);
            float length = (float)Math.Abs(vertex2.Distance - vertex1.Distance);
            return xDistance <= length + allowance;
        }

        /// <summary>
        /// Move the position of <paramref name="movableVertex"/> towards the position of <paramref name="fixedVertex"/>
        /// until the vertex pair satisfies the condition <see cref="canConnect"/>.
        /// </summary>
        /// <returns>The resulting position of <paramref name="movableVertex"/>.</returns>
        private float clampToConnectablePosition(JuiceStreamPathVertex fixedVertex, JuiceStreamPathVertex movableVertex)
        {
            float length = (float)Math.Abs(movableVertex.Distance - fixedVertex.Distance);
            return Math.Clamp(movableVertex.X, fixedVertex.X - length, fixedVertex.X + length);
        }

        private void invalidate() => InvalidationID++;
    }
}
