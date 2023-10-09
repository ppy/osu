// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Audio;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Timing;
using osu.Game.Screens.Play;

namespace osu.Game.Tests.NonVisual
{
    [TestFixture]
    public partial class GameplayClockContainerTest
    {
        [TestCase(0)]
        [TestCase(1)]
        public void TestTrueGameplayRateWithGameplayAdjustment(double underlyingClockRate)
        {
            var trackVirtual = new TrackVirtual(60000) { Frequency = { Value = underlyingClockRate } };
            var gameplayClock = new TestGameplayClockContainer(trackVirtual);

            Assert.That(gameplayClock.GetTrueGameplayRate(), Is.EqualTo(2));
        }

        private partial class TestGameplayClockContainer : GameplayClockContainer
        {
            public TestGameplayClockContainer(IClock underlyingClock)
                : base(underlyingClock, false, false)
            {
                AdjustmentsFromMods.AddAdjustment(AdjustableProperty.Frequency, new BindableDouble(2.0));
            }
        }
    }
}
