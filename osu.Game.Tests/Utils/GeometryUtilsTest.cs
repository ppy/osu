// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Utils;
using osuTK;

namespace osu.Game.Tests.Utils
{
    [TestFixture]
    public class GeometryUtilsTest
    {
        [TestCase(new int[] { }, new int[] { })]
        [TestCase(new[] { 0, 0 }, new[] { 0, 0 })]
        [TestCase(new[] { 0, 0, 1, 1, 1, -1, 2, 0 }, new[] { 0, 0, 1, 1, 2, 0, 1, -1 })]
        [TestCase(new[] { 0, 0, 1, 1, 1, -1, 2, 0, 1, 0 }, new[] { 0, 0, 1, 1, 2, 0, 1, -1 })]
        [TestCase(new[] { 0, 0, 1, 1, 2, -1, 2, 0, 1, 0, 4, 10 }, new[] { 0, 0, 4, 10, 2, -1 })]
        public void TestConvexHull(int[] values, int[] expected)
        {
            var points = new Vector2[values.Length / 2];
            for (int i = 0; i < values.Length; i += 2)
                points[i / 2] = new Vector2(values[i], values[i + 1]);

            var expectedPoints = new Vector2[expected.Length / 2];
            for (int i = 0; i < expected.Length; i += 2)
                expectedPoints[i / 2] = new Vector2(expected[i], expected[i + 1]);

            var hull = GeometryUtils.GetConvexHull(points);

            Assert.That(hull, Is.EquivalentTo(expectedPoints));
        }

        [TestCase(new int[] { }, 0, 0, 0)]
        [TestCase(new[] { 0, 0 }, 0, 0, 0)]
        [TestCase(new[] { 0, 0, 1, 1, 1, -1, 2, 0 }, 1, 0, 1)]
        [TestCase(new[] { 0, 0, 1, 1, 1, -1, 2, 0, 1, 0 }, 1, 0, 1)]
        [TestCase(new[] { 0, 0, 1, 1, 2, -1, 2, 0, 1, 0, 4, 10 }, 3, 4.5f, 5.5901699f)]
        public void TestMinimumEnclosingCircle(int[] values, float x, float y, float r)
        {
            var points = new Vector2[values.Length / 2];
            for (int i = 0; i < values.Length; i += 2)
                points[i / 2] = new Vector2(values[i], values[i + 1]);

            (var centre, float radius) = GeometryUtils.MinimumEnclosingCircle(points);

            Assert.That(centre.X, Is.EqualTo(x).Within(0.0001));
            Assert.That(centre.Y, Is.EqualTo(y).Within(0.0001));
            Assert.That(radius, Is.EqualTo(r).Within(0.0001));
        }
    }
}
