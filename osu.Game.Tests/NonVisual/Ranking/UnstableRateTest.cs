// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Utils;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Ranking.Statistics;

namespace osu.Game.Tests.NonVisual.Ranking
{
    [TestFixture]
    public class UnstableRateTest
    {
        [Test]
        public void TestDistributedHits()
        {
            var events = Enumerable.Range(-5, 11)
                                   .Select(t => new HitEvent(t - 5, HitResult.Great, new HitObject(), null, null));

            var unstableRate = new UnstableRate(events);

            Assert.IsNotNull(unstableRate.Value);
            Assert.IsTrue(Precision.AlmostEquals(unstableRate.Value.Value, 10 * Math.Sqrt(10)));
        }

        [Test]
        public void TestMissesAndEmptyWindows()
        {
            var events = new[]
            {
                new HitEvent(-100, HitResult.Miss, new HitObject(), null, null),
                new HitEvent(0, HitResult.Great, new HitObject(), null, null),
                new HitEvent(200, HitResult.Meh, new HitObject { HitWindows = HitWindows.Empty }, null, null),
            };

            var unstableRate = new UnstableRate(events);

            Assert.AreEqual(0, unstableRate.Value);
        }
    }
}
