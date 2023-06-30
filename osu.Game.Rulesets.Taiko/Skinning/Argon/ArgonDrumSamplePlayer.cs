// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Game.Audio;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.UI;
using osu.Game.Rulesets.UI;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Taiko.Skinning.Argon
{
    public partial class ArgonDrumSamplePlayer : DrumSamplePlayer
    {
        protected override DrumSampleTriggerSource CreateTriggerSource(HitObjectContainer hitObjectContainer, SampleBalance balance) =>
            new ArgonDrumSampleTriggerSource(hitObjectContainer, balance);

        public partial class ArgonDrumSampleTriggerSource : DrumSampleTriggerSource
        {
            [Resolved]
            private ISkinSource skinSource { get; set; } = null!;

            public ArgonDrumSampleTriggerSource(HitObjectContainer hitObjectContainer, SampleBalance balance)
                : base(hitObjectContainer, balance)
            {
                // TODO: pool flourish sample
            }

            public override void Play(HitType hitType)
            {
                TaikoHitObject? hitObject = GetMostValidObject() as TaikoHitObject;

                if (hitObject == null)
                    return;

                var baseSample = new VolumeAwareHitSampleInfo(hitObject.CreateHitSampleInfo(hitType == HitType.Rim ? HitSampleInfo.HIT_CLAP : HitSampleInfo.HIT_NORMAL));

                // If the sample is provided by a legacy skin, we should not try and do anything special.
                if (skinSource.FindProvider(s => s.GetSample(baseSample) != null) is LegacySkin)
                {
                    base.Play(hitType);
                    return;
                }

                // let the magic begin...

                if ((hitObject as TaikoStrongableHitObject)?.IsStrong == true || hitObject is StrongNestedHitObject)
                {
                    PlaySamples(new ISampleInfo[]
                    {
                        new VolumeAwareHitSampleInfo(hitObject.CreateHitSampleInfo(hitType == HitType.Rim ? HitSampleInfo.HIT_CLAP : HitSampleInfo.HIT_NORMAL), true),
                        // TODO: flourish should only play every time_between_flourishes.
                        new VolumeAwareHitSampleInfo(hitObject.CreateHitSampleInfo(hitType == HitType.Rim ? HitSampleInfo.HIT_FLOURISH : string.Empty), true),
                        baseSample
                    });
                }
                else
                {
                    PlaySamples(new ISampleInfo[] { new VolumeAwareHitSampleInfo(baseSample) });
                }
            }

            private class VolumeAwareHitSampleInfo : HitSampleInfo
            {
                public const int SAMPLE_VOLUME_THRESHOLD_HARD = 90;
                public const int SAMPLE_VOLUME_THRESHOLD_MEDIUM = 60;

                public VolumeAwareHitSampleInfo(HitSampleInfo sampleInfo, bool isStrong = false)
                    : base(sampleInfo.Name, isStrong ? BANK_STRONG : getBank(sampleInfo.Bank, sampleInfo.Name, sampleInfo.Volume), sampleInfo.Suffix, sampleInfo.Volume)
                {
                }

                public override IEnumerable<string> LookupNames
                {
                    get
                    {
                        foreach (string name in base.LookupNames)
                            yield return name.Insert(name.LastIndexOf('/') + 1, "Argon/taiko-");
                    }
                }

                private static string getBank(string originalBank, string sampleName, int volume)
                {
                    // So basically we're overwriting mapper's bank intentions here.
                    // The rationale is that most taiko beatmaps only use a single bank, but regularly adjust volume.

                    switch (sampleName)
                    {
                        case HIT_NORMAL:
                        case HIT_CLAP:
                        {
                            if (volume >= SAMPLE_VOLUME_THRESHOLD_HARD)
                                return BANK_DRUM;

                            if (volume >= SAMPLE_VOLUME_THRESHOLD_MEDIUM)
                                return BANK_NORMAL;

                            return BANK_SOFT;
                        }

                        default:
                            return originalBank;
                    }
                }
            }
        }
    }
}
