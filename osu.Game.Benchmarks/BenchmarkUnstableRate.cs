// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Benchmarks
{
    public class BenchmarkUnstableRate : BenchmarkTest
    {
        private List<HitEvent> events = null!;

        public override void SetUp()
        {
            base.SetUp();
            events = new List<HitEvent>();

            for (int i = 0; i < 2048; i++)
            {
                // Ensure the object has hit windows populated.
                var hitObject = new HitCircle();
                hitObject.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());

                events.Add(new HitEvent(RNG.NextDouble(-200.0, 200.0), RNG.NextDouble(1.0, 2.0), HitResult.Great, hitObject, null, null));
            }
        }

        [Benchmark]
        public void CalculateUnstableRate()
        {
            _ = events.CalculateUnstableRate();
        }
    }
}
