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
        private readonly List<List<HitEvent>> incrementalEventLists = new List<List<HitEvent>>();

        public override void SetUp()
        {
            base.SetUp();

            var events = new List<HitEvent>();

            for (int i = 0; i < 2048; i++)
            {
                // Ensure the object has hit windows populated.
                var hitObject = new HitCircle();
                hitObject.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());
                events.Add(new HitEvent(RNG.NextDouble(-200.0, 200.0), RNG.NextDouble(1.0, 2.0), HitResult.Great, hitObject, null, null));

                incrementalEventLists.Add(new List<HitEvent>(events));
            }
        }

        [Benchmark]
        public void CalculateUnstableRate()
        {
            for (int i = 0; i < 2048; i++)
            {
                var events = incrementalEventLists[i];
                _ = events.CalculateUnstableRate();
            }
        }

        [Benchmark]
        public void CalculateUnstableRateUsingIncrementalCalculation()
        {
            HitEventExtensions.UnstableRateCalculationResult? last = null;

            for (int i = 0; i < 2048; i++)
            {
                var events = incrementalEventLists[i];
                last = events.CalculateUnstableRate(last);
            }
        }
    }
}
