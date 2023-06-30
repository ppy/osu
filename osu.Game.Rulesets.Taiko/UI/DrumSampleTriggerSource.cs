// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
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

            List<ISampleInfo> samplesToPlay = new List<ISampleInfo>
            {
                hitObject.CreateHitSampleInfo(hitType == HitType.Rim ? HitSampleInfo.HIT_CLAP : HitSampleInfo.HIT_NORMAL)
            };

            // strong + rim always maps to whistle.
            if ((hitObject as TaikoStrongableHitObject)?.IsStrong == true || hitObject is StrongNestedHitObject)
            {
                samplesToPlay.Add(hitObject.CreateHitSampleInfo(hitType == HitType.Rim ? HitSampleInfo.HIT_WHISTLE : HitSampleInfo.HIT_FINISH));
            }

            PlaySamples(samplesToPlay.ToArray());
        }

        public override void Play() => throw new InvalidOperationException(@"Use override with HitType parameter instead");
    }
}
