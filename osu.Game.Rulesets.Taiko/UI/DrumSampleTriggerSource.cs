// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Audio;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Taiko.UI
{
    public class DrumSampleTriggerSource : GameplaySampleTriggerSource
    {
        public DrumSampleTriggerSource(HitObjectContainer hitObjectContainer)
            : base(hitObjectContainer)
        {
        }

        public void Play(HitType hitType)
        {
            var hitObject = GetMostValidObject();

            if (hitObject == null)
                return;

            PlaySamples(new ISampleInfo[] { hitObject.SampleControlPoint.GetSampleInfo(hitType == HitType.Rim ? HitSampleInfo.HIT_CLAP : HitSampleInfo.HIT_NORMAL) });
        }

        public override void Play() => throw new InvalidOperationException(@"Use override with HitType parameter instead");
    }
}
