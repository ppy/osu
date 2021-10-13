// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Utils;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osuTK;

namespace osu.Game.Rulesets.Catch.Tests
{
    [TestFixture]
    public class JuiceStreamPathTest
    {
        [TestCase(1e3, true, false)]
        // When the coordinates are large, the slope invariant fails within the specified absolute allowance due to the floating-number precision.
        [TestCase(1e9, false, false)]
        // Using discrete values sometimes discover more edge cases.
        [TestCase(10, true, true)]
        public void TestRandomInsertSetPosition(double scale, bool checkSlope, bool integralValues)
        {
            var rng = new Random(1);
            var path = new JuiceStreamPath();

            for (int iteration = 0; iteration < 100000; iteration++)
            {
                if (rng.Next(10) == 0)
                    path.Clear();

                int vertexCount = path.Vertices.Count;

                switch (rng.Next(2))
                {
                    case 0:
                    {
                        double distance = rng.NextDouble() * scale * 2 - scale;
                        if (integralValues)
                            distance = Math.Round(distance);

                        float oldX = path.PositionAtDistance(distance);
                        int index = path.InsertVertex(distance);
                        Assert.That(path.Vertices.Count, Is.EqualTo(vertexCount + 1));
                        Assert.That(path.Vertices[index].Distance, Is.EqualTo(distance));
                        Assert.That(path.Vertices[index].X, Is.EqualTo(oldX));
                        break;
                    }

                    case 1:
                    {
                        int index = rng.Next(path.Vertices.Count);
                        double distance = path.Vertices[index].Distance;
                        float newX = (float)(rng.NextDouble() * scale * 2 - scale);
                        if (integralValues)
                            newX = MathF.Round(newX);

                        path.SetVertexPosition(index, newX);
                        Assert.That(path.Vertices.Count, Is.EqualTo(vertexCount));
                        Assert.That(path.Vertices[index].Distance, Is.EqualTo(distance));
                        Assert.That(path.Vertices[index].X, Is.EqualTo(newX));
                        break;
                    }
                }

                assertInvariants(path.Vertices, checkSlope);
            }
        }

        [Test]
        public void TestRemoveVertices()
        {
            var path = new JuiceStreamPath();
            path.Add(10, 5);
            path.Add(20, -5);

            int removeCount = path.RemoveVertices((v, i) => v.Distance == 10 && i == 1);
            Assert.That(removeCount, Is.EqualTo(1));
            Assert.That(path.Vertices, Is.EqualTo(new[]
            {
                new JuiceStreamPathVertex(0, 0),
                new JuiceStreamPathVertex(20, -5)
            }));

            removeCount = path.RemoveVertices((_, i) => i == 0);
            Assert.That(removeCount, Is.EqualTo(1));
            Assert.That(path.Vertices, Is.EqualTo(new[]
            {
                new JuiceStreamPathVertex(20, -5)
            }));

            removeCount = path.RemoveVertices((_, i) => true);
            Assert.That(removeCount, Is.EqualTo(1));
            Assert.That(path.Vertices, Is.EqualTo(new[]
            {
                new JuiceStreamPathVertex()
            }));
        }

        [Test]
        public void TestResampleVertices()
        {
            var path = new JuiceStreamPath();
            path.Add(-100, -10);
            path.Add(100, 50);
            path.ResampleVertices(new double[]
            {
                -50,
                0,
                70,
                120
            });
            Assert.That(path.Vertices, Is.EqualTo(new[]
            {
                new JuiceStreamPathVertex(-100, -10),
                new JuiceStreamPathVertex(-50, -5),
                new JuiceStreamPathVertex(0, 0),
                new JuiceStreamPathVertex(70, 35),
                new JuiceStreamPathVertex(100, 50),
                new JuiceStreamPathVertex(100, 50),
            }));

            path.Clear();
            path.SetVertexPosition(0, 10);
            path.ResampleVertices(Array.Empty<double>());
            Assert.That(path.Vertices, Is.EqualTo(new[]
            {
                new JuiceStreamPathVertex(0, 10)
            }));
        }

        [Test]
        public void TestRandomConvertFromSliderPath()
        {
            var rng = new Random(1);
            var path = new JuiceStreamPath();
            var sliderPath = new SliderPath();

            for (int iteration = 0; iteration < 10000; iteration++)
            {
                sliderPath.ControlPoints.Clear();

                do
                {
                    int start = sliderPath.ControlPoints.Count;

                    do
                    {
                        float x = (float)(rng.NextDouble() * 1e3);
                        float y = (float)(rng.NextDouble() * 1e3);
                        sliderPath.ControlPoints.Add(new PathControlPoint(new Vector2(x, y)));
                    } while (rng.Next(2) != 0);

                    int length = sliderPath.ControlPoints.Count - start + 1;
                    sliderPath.ControlPoints[start].Type = length <= 2 ? PathType.Linear : length == 3 ? PathType.PerfectCurve : PathType.Bezier;
                } while (rng.Next(3) != 0);

                if (rng.Next(5) == 0)
                    sliderPath.ExpectedDistance.Value = rng.NextDouble() * 3e3;
                else
                    sliderPath.ExpectedDistance.Value = null;

                path.ConvertFromSliderPath(sliderPath);
                Assert.That(path.Vertices[0].Distance, Is.EqualTo(0));
                Assert.That(path.Distance, Is.EqualTo(sliderPath.Distance).Within(1e-3));
                assertInvariants(path.Vertices, true);

                double[] sampleDistances = Enumerable.Range(0, 10)
                                                     .Select(_ => rng.NextDouble() * sliderPath.Distance)
                                                     .ToArray();

                foreach (double distance in sampleDistances)
                {
                    float expected = sliderPath.PositionAt(distance / sliderPath.Distance).X;
                    Assert.That(path.PositionAtDistance(distance), Is.EqualTo(expected).Within(1e-3));
                }

                path.ResampleVertices(sampleDistances);
                assertInvariants(path.Vertices, true);

                foreach (double distance in sampleDistances)
                {
                    float expected = sliderPath.PositionAt(distance / sliderPath.Distance).X;
                    Assert.That(path.PositionAtDistance(distance), Is.EqualTo(expected).Within(1e-3));
                }
            }
        }

        [Test]
        public void TestRandomConvertToSliderPath()
        {
            var rng = new Random(1);
            var path = new JuiceStreamPath();
            var sliderPath = new SliderPath();

            for (int iteration = 0; iteration < 10000; iteration++)
            {
                path.Clear();

                do
                {
                    double distance = rng.NextDouble() * 1e3;
                    float x = (float)(rng.NextDouble() * 1e3);
                    path.Add(distance, x);
                } while (rng.Next(5) != 0);

                float sliderStartY = (float)(rng.NextDouble() * JuiceStreamPath.OSU_PLAYFIELD_HEIGHT);

                path.ConvertToSliderPath(sliderPath, sliderStartY);
                Assert.That(sliderPath.Distance, Is.EqualTo(path.Distance).Within(1e-3));
                Assert.That(sliderPath.ControlPoints[0].Position.X, Is.EqualTo(path.Vertices[0].X));
                assertInvariants(path.Vertices, true);

                foreach (var point in sliderPath.ControlPoints)
                {
                    Assert.That(point.Type, Is.EqualTo(PathType.Linear).Or.Null);
                    Assert.That(sliderStartY + point.Position.Y, Is.InRange(0, JuiceStreamPath.OSU_PLAYFIELD_HEIGHT));
                }

                for (int i = 0; i < 10; i++)
                {
                    double distance = rng.NextDouble() * path.Distance;
                    float expected = path.PositionAtDistance(distance);
                    Assert.That(sliderPath.PositionAt(distance / sliderPath.Distance).X, Is.EqualTo(expected).Within(1e-3));
                }
            }
        }

        [Test]
        public void TestInvalidation()
        {
            var path = new JuiceStreamPath();
            Assert.That(path.InvalidationID, Is.EqualTo(1));
            int previousId = path.InvalidationID;

            path.InsertVertex(10);
            checkNewId();

            path.SetVertexPosition(1, 5);
            checkNewId();

            path.Add(20, 0);
            checkNewId();

            path.RemoveVertices((v, _) => v.Distance == 20);
            checkNewId();

            path.ResampleVertices(new double[] { 5, 10, 15 });
            checkNewId();

            path.Clear();
            checkNewId();

            path.ConvertFromSliderPath(new SliderPath());
            checkNewId();

            void checkNewId()
            {
                Assert.That(path.InvalidationID, Is.Not.EqualTo(previousId));
                previousId = path.InvalidationID;
            }
        }

        private void assertInvariants(IReadOnlyList<JuiceStreamPathVertex> vertices, bool checkSlope)
        {
            Assert.That(vertices, Is.Not.Empty);

            for (int i = 0; i < vertices.Count; i++)
            {
                Assert.That(double.IsFinite(vertices[i].Distance));
                Assert.That(float.IsFinite(vertices[i].X));
            }

            for (int i = 1; i < vertices.Count; i++)
            {
                Assert.That(vertices[i].Distance, Is.GreaterThanOrEqualTo(vertices[i - 1].Distance));

                if (!checkSlope) continue;

                float xDiff = Math.Abs(vertices[i].X - vertices[i - 1].X);
                double distanceDiff = vertices[i].Distance - vertices[i - 1].Distance;
                Assert.That(xDiff, Is.LessThanOrEqualTo(distanceDiff).Within(Precision.FLOAT_EPSILON));
            }
        }
    }
}
