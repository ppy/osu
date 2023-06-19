// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Utils;
using osu.Game.Audio;
using osu.Game.Rulesets.Taiko.Audio;
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

        public void Play(HitType hitType, bool strong)
        {
            var hitObject = GetMostValidObject();

            if (hitObject == null)
                return;

            string sampleName;

            switch (hitType)
            {
                case HitType.Centre:
                    sampleName = strong ? TaikoHitSampleInfo.STRONG_HIT : HitSampleInfo.HIT_NORMAL;
                    break;

                case HitType.Rim:
                    sampleName = strong ? TaikoHitSampleInfo.STRONG_CLAP : HitSampleInfo.HIT_CLAP;
                    break;

                default:
                    throw new InvalidOperationException(@"Attempted to trigger sample playback of an invalid HitType");
            }

            if (strong)
                StopAllPlayback();

            PlaySamples(new ISampleInfo[] { hitObject.CreateHitSampleInfo(sampleName) });
        }

        protected override void ApplySampleInfo(SkinnableSound hitSound, ISampleInfo[] samples)
        {
            base.ApplySampleInfo(hitSound, samples);

            hitSound.Frequency.Value = 0.98 + RNG.NextDouble(0.04);
            hitSound.Balance.Value = -0.05 + RNG.NextDouble(0.1);
        }

        public override void Play() => throw new InvalidOperationException(@"Use Play(HitType, bool) override instead");
    }

    public enum SampleBalance
    {
        Left,
        Centre,
        Right
    }
}
