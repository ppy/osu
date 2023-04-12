// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Utils;
using osu.Game.Audio;
using osu.Game.Rulesets.Taiko.Audio;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Taiko.UI
{
    public partial class DrumSampleTriggerSource : GameplaySampleTriggerSource
    {
        public enum SampleBalance
        {
            L, C, R
        }

        public Bindable<SampleBalance> Balance = new Bindable<SampleBalance>(SampleBalance.C);
        private readonly Bindable<double> balanceDouble = new Bindable<double>();
        private const double stereo_separation = 0.2;

        public DrumSampleTriggerSource(HitObjectContainer hitObjectContainer)
            : base(hitObjectContainer)
        {
            Balance.ValueChanged += @event =>
            {
                switch (@event.NewValue)
                {
                    case SampleBalance.L:
                        balanceDouble.Value = -stereo_separation;
                        break;

                    case SampleBalance.C:
                        balanceDouble.Value = 0;
                        break;

                    case SampleBalance.R:
                        balanceDouble.Value = stereo_separation;
                        break;
                }
            };

            AudioMixer.Balance.BindTo(balanceDouble);
        }

        public void Play(HitType hitType)
        {
            var hitObject = GetMostValidObject();
            if (hitObject == null) return;

            string sampleName = hitType switch
            {
                HitType.Centre => HitSampleInfo.HIT_NORMAL,
                HitType.Rim => HitSampleInfo.HIT_CLAP,
                HitType.StrongCentre => TaikoHitSampleInfo.TAIKO_STRONG_HIT,
                HitType.StrongRim => TaikoHitSampleInfo.TAIKO_STRONG_CLAP,
                _ => throw new InvalidOperationException(@"Attempted to trigger sample playback of an invalid HitType")
            };

            if (hitType is HitType.StrongRim or HitType.StrongCentre || (hitType == HitType.Centre && hitObject.SampleControlPoint.SampleVolume >= TaikoHitSampleInfo.SAMPLE_VOLUME_THRESHOLD_HARD))
                FlushPlayback();

            var scp = hitObject.SampleControlPoint;
            PlaySamples(new ISampleInfo[] { new TaikoHitSampleInfo(sampleName, scp.SampleBank, volume: scp.SampleVolume) });
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
}
