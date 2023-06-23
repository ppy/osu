// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Utils;
using osu.Game.Audio;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.UI;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Taiko.UI
{
    public partial class DrumSampleTriggerSource : GameplaySampleTriggerSource
    {
        [Resolved]
        private ISkinSource skinSource { get; set; } = null!;

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

        public void Play(HitType hitType)
        {
            var hitObject = GetMostValidObject();

            if (hitObject == null)
                return;

            var strongSamples = hitObject.Samples.Where(s => s.Bank == HitSampleInfo.BANK_STRONG).Cast<ISampleInfo>().ToArray();

            if (strongSamples.Any())
            {
                // Special behaviour if the skin has strong samples available.
                if (skinSource.GetSample(strongSamples.First()) != null)
                {
                    StopAllPlayback();
                    PlaySamples(strongSamples);
                    return;
                }
            }

            var sample = hitObject.CreateHitSampleInfo(hitType == HitType.Rim ? HitSampleInfo.HIT_CLAP : HitSampleInfo.HIT_NORMAL);
            PlaySamples(new ISampleInfo[] { sample });
        }

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
