// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Audio;

namespace osu.Game.Rulesets.Taiko.Skinning.Argon
{
    public class VolumeAwareHitSampleInfo : HitSampleInfo
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
