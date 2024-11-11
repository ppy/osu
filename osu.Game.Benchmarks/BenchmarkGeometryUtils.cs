// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using BenchmarkDotNet.Attributes;
using osu.Framework.Utils;
using osu.Game.Utils;
using osuTK;

namespace osu.Game.Benchmarks
{
    public class BenchmarkGeometryUtils : BenchmarkTest
    {
        [Params(100, 1000, 2000, 4000, 8000, 10000)]
        public int N;

        private Vector2[] points = null!;

        public override void SetUp()
        {
            points = new Vector2[N];

            for (int i = 0; i < points.Length; ++i)
                points[i] = new Vector2(RNG.Next(512), RNG.Next(384));
        }

        [Benchmark]
        public void MinimumEnclosingCircle() => GeometryUtils.MinimumEnclosingCircle(points);
    }
}
