// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Audio;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.UI;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Taiko.Skinning.Argon
{
    public partial class ArgonDrumSamplePlayer : DrumSamplePlayer
    {
        protected override DrumSampleTriggerSource CreateTriggerSource(HitObjectContainer hitObjectContainer, SampleBalance balance) =>
            new ArgonDrumSampleTriggerSource(hitObjectContainer, balance);

        public partial class ArgonDrumSampleTriggerSource : DrumSampleTriggerSource
        {
            public ArgonDrumSampleTriggerSource(HitObjectContainer hitObjectContainer, SampleBalance balance)
                : base(hitObjectContainer, balance)
            {
            }

            public override void Play(HitType hitType)
            {
                // let the magic begin...

                TaikoHitObject? hitObject = GetMostValidObject() as TaikoHitObject;

                if (hitObject == null)
                    return;

                var baseSample = hitObject.CreateHitSampleInfo(hitType == HitType.Rim ? HitSampleInfo.HIT_CLAP : HitSampleInfo.HIT_NORMAL);

                if ((hitObject as TaikoStrongableHitObject)?.IsStrong == true || hitObject is StrongNestedHitObject)
                {
                    PlaySamples(new ISampleInfo[]
                    {
                        hitObject.CreateHitSampleInfo(hitType == HitType.Rim ? HitSampleInfo.HIT_WHISTLE : HitSampleInfo.HIT_FINISH),
                        baseSample
                    });
                }
                else
                {
                    PlaySamples(new ISampleInfo[] { baseSample });
                }
            }
        }
    }
}
