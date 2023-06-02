// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Utils;
using osu.Game.Audio;
using osu.Game.Rulesets.Taiko.Audio;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.UI;

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
            var hitSample = GetMostValidObject()?.Samples?.FirstOrDefault(o => o.Name == HitSampleInfo.HIT_NORMAL);

            if (hitSample == null)
                return;

            string sampleName;

            switch (hitType)
            {
                case HitType.Centre:
                    sampleName = strong ? TaikoHitSampleInfo.TAIKO_STRONG_HIT : HitSampleInfo.HIT_NORMAL;
                    break;

                case HitType.Rim:
                    sampleName = strong ? TaikoHitSampleInfo.TAIKO_STRONG_CLAP : HitSampleInfo.HIT_CLAP;
                    break;

                default:
                    throw new InvalidOperationException(@"Attempted to trigger sample playback of an invalid HitType");
            }

            if (strong)
                FlushPlayback();

            PlaySamples(new ISampleInfo[] { new HitSampleInfo(sampleName, hitSample.Bank, volume: hitSample.Volume) });
        }

        public void FlushPlayback()
        {
            foreach (var sound in HitSounds)
                sound.Stop();
        }

        protected override void PlaySamples(ISampleInfo[] samples) => Schedule(() =>
        {
            var hitSound = GetNextSample();
            hitSound.Samples = samples;

            hitSound.Frequency.Value = 0.98 + RNG.NextDouble(0.04);
            hitSound.Balance.Value = -0.05 + RNG.NextDouble(0.1);

            hitSound.Play();
        });

        public override void Play() => throw new InvalidOperationException(@"Use override with HitType parameter instead");
    }

    public enum SampleBalance
    {
        Left,
        Centre,
        Right
    }
}
