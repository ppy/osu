// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Bindables;
using osu.Framework.Timing;
using osu.Game.Screens.Play;

namespace osu.Game.Tests.NonVisual
{
    [TestFixture]
    public class GameplayClockTest
    {
        [TestCase(0)]
        [TestCase(1)]
        public void TestTrueGameplayRateWithZeroAdjustment(double underlyingClockRate)
        {
            var framedClock = new FramedClock(new ManualClock { Rate = underlyingClockRate });
            var gameplayClock = new TestGameplayClock(framedClock);

            gameplayClock.MutableNonGameplayAdjustments.Add(new BindableDouble());

            Assert.That(gameplayClock.TrueGameplayRate, Is.EqualTo(0));
        }

        private class TestGameplayClock : GameplayClock
        {
            public List<Bindable<double>> MutableNonGameplayAdjustments { get; } = new List<Bindable<double>>();

            public override IEnumerable<Bindable<double>> NonGameplayAdjustments => MutableNonGameplayAdjustments;

            public TestGameplayClock(IFrameBasedClock underlyingClock)
                : base(underlyingClock)
            {
            }
        }
    }
}
