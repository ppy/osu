// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Game.Audio;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.UI;
using osu.Game.Rulesets.UI;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Taiko.Skinning.Argon
{
    internal partial class ArgonDrumSamplePlayer : DrumSamplePlayer
    {
        [BackgroundDependencyLoader]
        private void load(IPooledSampleProvider sampleProvider)
        {
            // Warm up pools for non-standard samples.
            sampleProvider.GetPooledSample(new ArgonDrumSampleTriggerSource.VolumeAwareHitSampleInfo(new HitSampleInfo(HitSampleInfo.HIT_NORMAL), true));
            sampleProvider.GetPooledSample(new ArgonDrumSampleTriggerSource.VolumeAwareHitSampleInfo(new HitSampleInfo(HitSampleInfo.HIT_CLAP), true));
            sampleProvider.GetPooledSample(new ArgonDrumSampleTriggerSource.VolumeAwareHitSampleInfo(new HitSampleInfo(HitSampleInfo.HIT_FLOURISH), true));
        }

        protected override DrumSampleTriggerSource CreateTriggerSource(HitObjectContainer hitObjectContainer, SampleBalance balance) =>
            new ArgonDrumSampleTriggerSource(hitObjectContainer, balance);

        public partial class ArgonDrumSampleTriggerSource : DrumSampleTriggerSource
        {
            private readonly HitObjectContainer hitObjectContainer;

            [Resolved]
            private ISkinSource skinSource { get; set; } = null!;

            /// <summary>
            /// The minimum time to leave between flourishes that are added to strong rim hits.
            /// </summary>
            private const double time_between_flourishes = 2000;

            public ArgonDrumSampleTriggerSource(HitObjectContainer hitObjectContainer, SampleBalance balance)
                : base(hitObjectContainer, balance)
            {
                this.hitObjectContainer = hitObjectContainer;
            }

            public override void Play(HitType hitType, bool strong)
            {
                TaikoHitObject? hitObject = GetMostValidObject() as TaikoHitObject;

                if (hitObject == null)
                    return;

                var originalSample = hitObject.CreateHitSampleInfo(hitType == HitType.Rim ? HitSampleInfo.HIT_CLAP : HitSampleInfo.HIT_NORMAL);

                // If the sample is provided by a legacy skin, we should not try and do anything special.
                if (skinSource.FindProvider(s => s.GetSample(originalSample) != null) is LegacySkinTransformer)
                {
                    base.Play(hitType, strong);
                    return;
                }

                // let the magic begin...
                var samplesToPlay = new List<ISampleInfo> { new VolumeAwareHitSampleInfo(originalSample, strong) };

                if (strong && hitType == HitType.Rim && canPlayFlourish(hitObject))
                    samplesToPlay.Add(new VolumeAwareHitSampleInfo(hitObject.CreateHitSampleInfo(HitSampleInfo.HIT_FLOURISH), true));

                PlaySamples(samplesToPlay.ToArray());
            }

            private bool canPlayFlourish(TaikoHitObject hitObject)
            {
                double? lastFlourish = null;

                var hitObjects = hitObjectContainer.AliveObjects
                                                   .Reverse()
                                                   .Select(d => d.HitObject)
                                                   .OfType<Hit>()
                                                   .Where(h => h.IsStrong && h.Type == HitType.Rim);

                // Add an additional 'flourish' sample to strong rim hits (that are at least `time_between_flourishes` apart).
                // This is applied to hitobjects in reverse order, as to sound more musically coherent by biasing towards to
                // end of groups/combos of strong rim hits instead of the start.
                foreach (var h in hitObjects)
                {
                    bool canFlourish = lastFlourish == null || lastFlourish - h.StartTime >= time_between_flourishes;

                    if (canFlourish)
                        lastFlourish = h.StartTime;

                    if (h == hitObject)
                        return canFlourish;
                }

                return false;
            }

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
    }
}
