// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
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
            var events = Enumerable.Range(-5, 11)
                                   .Select(t => new HitEvent(0, 1.0, HitResult.Great, new HitCircle(), new HitCircle(), new Vector2(t, t)));

            var aimError = new AimError(events);

            Assert.IsNotNull(aimError.Value);
            Assert.AreEqual(Math.Sqrt(20) * 10, aimError.Value!.Value, 1e-5);
        }

        [Test]
        public void TestNullPositionsReturnNull()
        {
            var events = new[]
            {
                new HitEvent(0, 1.0, HitResult.Miss, new HitCircle(), new HitCircle(), null),
                new HitEvent(0, 1.0, HitResult.Miss, new HitCircle(), new HitCircle(), null),
                new HitEvent(0, 1.0, HitResult.Miss, new HitCircle(), new HitCircle(), null),
            };

            var aimError = new AimError(events);

            Assert.IsNull(aimError.Value);
        }

        [Test]
        public void TestNullLastObjectReturnsNull()
        {
            var events = new[]
            {
                new HitEvent(0, 1.0, HitResult.Miss, new HitCircle(), null, new Vector2(0, 0)),
                new HitEvent(0, 1.0, HitResult.Miss, new HitCircle(), null, new Vector2(0, 0)),
                new HitEvent(0, 1.0, HitResult.Miss, new HitCircle(), null, new Vector2(0, 0)),
            };

            var aimError = new AimError(events);

            Assert.IsNull(aimError.Value);
        }
    }
}
