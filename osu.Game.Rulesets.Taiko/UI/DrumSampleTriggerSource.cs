// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
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
            TaikoHitObject? hitObject = GetMostValidObject() as TaikoHitObject;

            if (hitObject == null)
                return;

            var baseSample = hitObject.CreateHitSampleInfo(hitType == HitType.Rim ? HitSampleInfo.HIT_CLAP : HitSampleInfo.HIT_NORMAL);

            if ((hitObject as TaikoStrongableHitObject)?.IsStrong == true || hitObject is StrongNestedHitObject)
            {
                PlaySamples(new ISampleInfo[]
                {
                    baseSample,
                    hitObject.CreateHitSampleInfo(hitType == HitType.Rim ? HitSampleInfo.HIT_WHISTLE : HitSampleInfo.HIT_FINISH)
                });
            }
            else
            {
                PlaySamples(new ISampleInfo[] { baseSample });
            }
        }

        public override void Play() => throw new InvalidOperationException(@"Use override with HitType parameter instead");
    }
}
