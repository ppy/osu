// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Game.Audio;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Taiko.UI
{
    public partial class DrumSampleTriggerSource : GameplaySampleTriggerSource
    {
        public DrumSampleTriggerSource(HitObjectContainer hitObjectContainer)
            : base(hitObjectContainer)
        {
        }

        public void Play(HitType hitType)
        {
            var hitSample = GetMostValidObject()?.Samples?.FirstOrDefault(o => o.Name == HitSampleInfo.HIT_NORMAL);

            if (hitSample == null)
                return;

            PlaySamples(new ISampleInfo[] { new HitSampleInfo(hitType == HitType.Rim ? HitSampleInfo.HIT_CLAP : HitSampleInfo.HIT_NORMAL, hitSample.Bank, volume: hitSample.Volume) });
        }

        public override void Play() => throw new InvalidOperationException(@"Use override with HitType parameter instead");
    }
}
