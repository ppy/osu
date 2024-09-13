// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Game.Audio;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.UI;
using osu.Game.Rulesets.UI;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Taiko.Skinning.Argon
{
    public partial class ArgonDrumSampleTriggerSource : DrumSampleTriggerSource
    {
        [Resolved]
        private ISkinSource skinSource { get; set; } = null!;

        public ArgonDrumSampleTriggerSource(HitObjectContainer hitObjectContainer, SampleBalance balance)
            : base(hitObjectContainer, balance)
        {
        }

        public override void Play(HitType hitType, bool strong)
        {
            if (GetMostValidObject() is not TaikoHitObject hitObject)
                return;

            var originalSample = hitObject.CreateHitSampleInfo(hitType == HitType.Rim ? HitSampleInfo.HIT_CLAP : HitSampleInfo.HIT_NORMAL);

            // If the sample is provided by a legacy skin, we should not try and do anything special.
            if (skinSource.FindProvider(s => s.GetSample(originalSample) != null) is LegacySkinTransformer)
            {
                base.Play(hitType, strong);
                return;
            }

            // let the magic begin...
            var samplesToPlay = new List<ISampleInfo> { new VolumeAwareHitSampleInfo(originalSample, strong) };

            PlaySamples(samplesToPlay.ToArray());
        }
    }
}
