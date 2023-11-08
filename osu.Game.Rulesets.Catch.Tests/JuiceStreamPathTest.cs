// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
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
                        double time = rng.NextDouble() * scale * 2 - scale;
                        if (integralValues)
                            time = Math.Round(time);

                        float oldX = path.PositionAtTime(time);
                        int index = path.InsertVertex(time);
                        Assert.That(path.Vertices.Count, Is.EqualTo(vertexCount + 1));
                        Assert.That(path.Vertices[index].Time, Is.EqualTo(time));
                        Assert.That(path.Vertices[index].X, Is.EqualTo(oldX));
                        break;
                    }

                    case 1:
                    {
                        int index = rng.Next(path.Vertices.Count);
                        double time = path.Vertices[index].Time;
                        float newX = (float)(rng.NextDouble() * scale * 2 - scale);
                        if (integralValues)
                            newX = MathF.Round(newX);

                        path.SetVertexPosition(index, newX);
                        Assert.That(path.Vertices.Count, Is.EqualTo(vertexCount));
                        Assert.That(path.Vertices[index].Time, Is.EqualTo(time));
                        Assert.That(path.Vertices[index].X, Is.EqualTo(newX));
                        break;
                    }
                }

                assertInvariants(path.Vertices);
            }
        }

        [Test]
        public void TestRemoveVertices()
        {
            var path = new JuiceStreamPath();
            path.Add(10, 5);
            path.Add(20, -5);

            int removeCount = path.RemoveVertices((v, i) => v.Time == 10 && i == 1);
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

            removeCount = path.RemoveVertices((_, _) => true);
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

        [TestCase(10)]
        [TestCase(0.1)]
        public void TestRandomConvertFromSliderPath(double velocity)
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
                    sliderPath.ControlPoints[start].Type = length <= 2 ? PathType.LINEAR : length == 3 ? PathType.PERFECTCURVE : PathType.BEZIER;
                } while (rng.Next(3) != 0);

                if (rng.Next(5) == 0)
                    sliderPath.ExpectedDistance.Value = rng.NextDouble() * 3e3;
                else
                    sliderPath.ExpectedDistance.Value = null;

                path.ConvertFromSliderPath(sliderPath, velocity);
                Assert.That(path.Vertices[0].Time, Is.EqualTo(0));
                Assert.That(path.Duration * velocity, Is.EqualTo(sliderPath.Distance).Within(1e-3));
                assertInvariants(path.Vertices);

                double[] sampleTimes = Enumerable.Range(0, 10)
                                                 .Select(_ => rng.NextDouble() * sliderPath.Distance / velocity)
                                                 .ToArray();

                foreach (double time in sampleTimes)
                {
                    float expected = sliderPath.PositionAt(time * velocity / sliderPath.Distance).X;
                    Assert.That(path.PositionAtTime(time), Is.EqualTo(expected).Within(1e-3));
                }

                path.ResampleVertices(sampleTimes);
                assertInvariants(path.Vertices);

                foreach (double time in sampleTimes)
                {
                    float expected = sliderPath.PositionAt(time * velocity / sliderPath.Distance).X;
                    Assert.That(path.PositionAtTime(time), Is.EqualTo(expected).Within(1e-3));
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
                    double time = rng.NextDouble() * 1e3;
                    float x = (float)(rng.NextDouble() * 1e3);
                    path.Add(time, x);
                } while (rng.Next(5) != 0);

                float sliderStartY = (float)(rng.NextDouble() * JuiceStreamPath.OSU_PLAYFIELD_HEIGHT);

                double requiredVelocity = path.ComputeRequiredVelocity();
                double velocity = Math.Clamp(requiredVelocity, 1, 100);

                path.ConvertToSliderPath(sliderPath, sliderStartY, velocity);

                foreach (var point in sliderPath.ControlPoints)
                {
                    Assert.That(point.Type, Is.EqualTo(PathType.LINEAR).Or.Null);
                    Assert.That(sliderStartY + point.Position.Y, Is.InRange(0, JuiceStreamPath.OSU_PLAYFIELD_HEIGHT));
                }

                Assert.That(sliderPath.ControlPoints[0].Position.X, Is.EqualTo(path.Vertices[0].X));

                // The path is preserved only if required velocity is used.
                if (velocity < requiredVelocity) continue;

                Assert.That(sliderPath.Distance / velocity, Is.EqualTo(path.Duration).Within(1e-3));

                for (int i = 0; i < 10; i++)
                {
                    double time = rng.NextDouble() * path.Duration;
                    float expected = path.PositionAtTime(time);
                    Assert.That(sliderPath.PositionAt(time * velocity / sliderPath.Distance).X, Is.EqualTo(expected).Within(3e-3));
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

            path.RemoveVertices((v, _) => v.Time == 20);
            checkNewId();

            path.ResampleVertices(new double[] { 5, 10, 15 });
            checkNewId();

            path.Clear();
            checkNewId();

            path.ConvertFromSliderPath(new SliderPath(), 1);
            checkNewId();

            void checkNewId()
            {
                Assert.That(path.InvalidationID, Is.Not.EqualTo(previousId));
                previousId = path.InvalidationID;
            }
        }

        private void assertInvariants(IReadOnlyList<JuiceStreamPathVertex> vertices)
        {
            Assert.That(vertices, Is.Not.Empty);

            for (int i = 0; i < vertices.Count; i++)
            {
                Assert.That(double.IsFinite(vertices[i].Time));
                Assert.That(float.IsFinite(vertices[i].X));
            }

            for (int i = 1; i < vertices.Count; i++)
            {
                Assert.That(vertices[i].Time, Is.GreaterThanOrEqualTo(vertices[i - 1].Time));
            }
        }
    }
}
