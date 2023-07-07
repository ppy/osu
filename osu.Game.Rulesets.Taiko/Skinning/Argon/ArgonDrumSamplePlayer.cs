// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Game.Audio;
using osu.Game.Rulesets.Taiko.UI;
using osu.Game.Rulesets.UI;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Taiko.Skinning.Argon
{
    internal partial class ArgonDrumSamplePlayer : DrumSamplePlayer
    {
        [BackgroundDependencyLoader]
        private void load(IPooledSampleProvider sampleProvider)
        {
            // Warm up pools for non-standard samples.
            sampleProvider.GetPooledSample(new VolumeAwareHitSampleInfo(new HitSampleInfo(HitSampleInfo.HIT_NORMAL), true));
            sampleProvider.GetPooledSample(new VolumeAwareHitSampleInfo(new HitSampleInfo(HitSampleInfo.HIT_CLAP), true));
            sampleProvider.GetPooledSample(new VolumeAwareHitSampleInfo(new HitSampleInfo(HitSampleInfo.HIT_FLOURISH), true));
        }

        protected override DrumSampleTriggerSource CreateTriggerSource(HitObjectContainer hitObjectContainer, SampleBalance balance) =>
            new ArgonDrumSampleTriggerSource(hitObjectContainer, balance);
    }
}
