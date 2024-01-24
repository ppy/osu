// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Statistics;
using osu.Game.Rulesets.Scoring;
using osuTK;

namespace osu.Game.Rulesets.Osu.Tests
{
    [TestFixture]
    public class AimErrorTest
    {
        [Test]
        public void TestDistributedHits()
        {
            var events = Enumerable.Range(0, 10)
                                   .Select(t => new HitEvent(0, 1.0, HitResult.Great, new HitCircle(), null, new Vector2(t, t)));

            IBeatmap beatmap = new Beatmap();
            beatmap.Difficulty.CircleSize = 0;

            var aimError = new AimError(events, beatmap);

            Assert.IsNotNull(aimError.Value);
            Assert.AreEqual(Math.Sqrt(57) * 10, aimError.Value!.Value);
        }

        [Test]
        public void TestMissesIncreaseDeviation()
        {
            var eventsWithMiss = new[]
            {
                new HitEvent(0, 1.0, HitResult.Miss, new HitCircle(), null, null),
                new HitEvent(0, 1.0, HitResult.Great, new HitCircle(), null, new Vector2(0, 0)),
                new HitEvent(0, 1.0, HitResult.Great, new HitCircle(), null, new Vector2(0, 0)),
            };

            var eventsWithoutMiss = new[]
            {
                new HitEvent(0, 1.0, HitResult.Great, new HitCircle(), null, new Vector2(0, 0)),
                new HitEvent(0, 1.0, HitResult.Great, new HitCircle(), null, new Vector2(0, 0)),
                new HitEvent(0, 1.0, HitResult.Great, new HitCircle(), null, new Vector2(0, 0)),
            };

            IBeatmap beatmap = new Beatmap();
            beatmap.Difficulty.CircleSize = 0;

            var aimErrorWithMiss = new AimError(eventsWithMiss, beatmap);
            var aimErrorWithoutMiss = new AimError(eventsWithoutMiss, beatmap);

            Assert.IsTrue(aimErrorWithMiss.Value != null && aimErrorWithoutMiss.Value != null && Precision.DefinitelyBigger(aimErrorWithMiss.Value.Value, aimErrorWithoutMiss.Value.Value));
        }

        [Test]
        public void TestNullPositionsReturnNull()
        {
            var events = new[]
            {
                new HitEvent(0, 1.0, HitResult.Miss, new HitCircle(), null, null),
                new HitEvent(0, 1.0, HitResult.Miss, new HitCircle(), null, null),
                new HitEvent(0, 1.0, HitResult.Miss, new HitCircle(), null, null),
            };

            IBeatmap beatmap = new Beatmap();
            beatmap.Difficulty.CircleSize = 0;

            var aimError = new AimError(events, beatmap);

            Assert.IsNull(aimError.Value);
        }
    }
}
