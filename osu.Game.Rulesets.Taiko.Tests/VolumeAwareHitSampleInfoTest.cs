// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Audio;
using osu.Game.Rulesets.Taiko.Skinning.Argon;

namespace osu.Game.Rulesets.Taiko.Tests
{
    [TestFixture]
    public class VolumeAwareHitSampleInfoTest
    {
        [Test]
        public void TestVolumeAwareHitSampleInfoIsNotEqualToItsUnderlyingSample(
            [Values(HitSampleInfo.HIT_NORMAL, HitSampleInfo.HIT_CLAP)]
            string sample,
            [Values(HitSampleInfo.BANK_NORMAL, HitSampleInfo.BANK_SOFT)]
            string bank,
            [Values(30, 70, 100)] int volume)
        {
            var underlyingSample = new HitSampleInfo(sample, bank, volume: volume);
            var volumeAwareSample = new VolumeAwareHitSampleInfo(underlyingSample);

            Assert.That(underlyingSample, Is.Not.EqualTo(volumeAwareSample));
        }
    }
}
