// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Utils;
using System.Linq;
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

        private readonly Bindable<double> balanceBindable = new Bindable<double>();
        private const double stereo_separation = 0.2;

        public DrumSampleTriggerSource(HitObjectContainer hitObjectContainer)
            : base(hitObjectContainer)
        {
            Balance.ValueChanged += change =>
            {
                switch (change.NewValue)
                {
                    case SampleBalance.L:
                        balanceBindable.Value = -stereo_separation;
                        break;

                    case SampleBalance.C:
                        balanceBindable.Value = 0;
                        break;

                    case SampleBalance.R:
                        balanceBindable.Value = stereo_separation;
                        break;
                }
            };

            AudioContainer.Balance.BindTo(balanceBindable);
        }

        public void Play(HitType hitType)
        {
            var hitSample = GetMostValidObject()?.Samples?.FirstOrDefault(o => o.Name == HitSampleInfo.HIT_NORMAL);

            if (hitSample == null)
                return;

            string sampleName;

            switch (hitType)
            {
                case HitType.Centre:
                    sampleName = HitSampleInfo.HIT_NORMAL;
                    break;

                case HitType.Rim:
                    sampleName = HitSampleInfo.HIT_CLAP;
                    break;

                case HitType.StrongCentre:
                    sampleName = TaikoHitSampleInfo.TAIKO_STRONG_HIT;
                    break;

                case HitType.StrongRim:
                    sampleName = TaikoHitSampleInfo.TAIKO_STRONG_CLAP;
                    break;

                default:
                    throw new InvalidOperationException(@"Attempted to trigger sample playback of an invalid HitType");
            }

            if (hitType is HitType.StrongRim or HitType.StrongCentre)
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
}
