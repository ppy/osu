// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Utils;
using osu.Game.Audio;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.UI;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Taiko.UI
{
    public partial class DrumSampleTriggerSource : GameplaySampleTriggerSource
    {
        private const double stereo_separation = 0.2;

        public DrumSampleTriggerSource(HitObjectContainer hitObjectContainer, SampleBalance balance = SampleBalance.Centre)
            : base(hitObjectContainer)
        {
            switch (balance)
            {
                case SampleBalance.Left:
                    AudioContainer.Balance.Value = -stereo_separation;
                    break;

                case SampleBalance.Centre:
                    AudioContainer.Balance.Value = 0;
                    break;

                case SampleBalance.Right:
                    AudioContainer.Balance.Value = stereo_separation;
                    break;
            }
        }

        public virtual void Play(HitType hitType, bool strong)
        {
            if (GetMostValidObject() is not TaikoHitObject hitObject)
                return;

            var baseSample = hitObject.CreateHitSampleInfo(hitType == HitType.Rim ? HitSampleInfo.HIT_CLAP : HitSampleInfo.HIT_NORMAL);

            if (strong)
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

        protected override void ApplySampleInfo(SkinnableSound hitSound, ISampleInfo[] samples)
        {
            base.ApplySampleInfo(hitSound, samples);

            hitSound.Balance.Value = -0.05 + RNG.NextDouble(0.1);
        }
    }

    public enum SampleBalance
    {
        Left,
        Centre,
        Right
    }
}
