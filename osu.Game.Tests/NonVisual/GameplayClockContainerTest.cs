// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Timing;
using osu.Game.Screens.Play;

namespace osu.Game.Tests.NonVisual
{
    [TestFixture]
    public class GameplayClockContainerTest
    {
        [TestCase(0)]
        [TestCase(1)]
        public void TestTrueGameplayRateWithGameplayAdjustment(double underlyingClockRate)
        {
            var framedClock = new FramedClock(new ManualClock { Rate = underlyingClockRate });
            var gameplayClock = new TestGameplayClockContainer(framedClock);

            Assert.That(gameplayClock.GetTrueGameplayRate(), Is.EqualTo(2));
        }

        private class TestGameplayClockContainer : GameplayClockContainer
        {
            public override IEnumerable<double> GameplayAdjustments => new[] { 2.0 };

            public TestGameplayClockContainer(IFrameBasedClock underlyingClock)
                : base(underlyingClock)
            {
            }
        }
    }
}
