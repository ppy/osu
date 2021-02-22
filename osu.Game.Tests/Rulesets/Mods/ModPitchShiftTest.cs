// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Audio;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Tests.Rulesets.Mods
{
    [TestFixture]
    public class ModPitchShiftTest
    {
        private TrackVirtual track;

        [SetUp]
        public void SetUp()
        {
            track = new TrackVirtual(20_000);
        }

        [TestCase(0.5)]
        [TestCase(1.0)]
        [TestCase(1.5)]
        public void TestModPitchShift(double pitch)
        {
            var mod = new ModPitchShift();
            mod.ApplyToTrack(track);

            mod.PitchChange.Value = pitch;

            Assert.That(track.AggregateTempo.Value, Is.EqualTo(1.0 / pitch));
            Assert.That(track.AggregateFrequency.Value, Is.EqualTo(pitch));
            Assert.That(track.Rate, Is.EqualTo(1.0));
        }

        [TestCase(0.5)]
        [TestCase(1.0)]
        [TestCase(1.5)]
        public void TestMatchTempo(double tempo)
        {
            track.AddAdjustment(AdjustableProperty.Tempo, new BindableDouble(tempo));

            var mod = new ModPitchShift();
            mod.ApplyToTrack(track);

            mod.MatchTempo.Value = true;

            mod.Update(null);

            Assert.That(track.AggregateFrequency.Value, Is.EqualTo(tempo));
        }
    }
}
