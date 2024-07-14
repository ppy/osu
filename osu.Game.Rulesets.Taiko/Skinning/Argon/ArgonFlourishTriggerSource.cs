// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Game.Audio;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.UI;
using osu.Game.Rulesets.UI;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Taiko.Skinning.Argon
{
    internal partial class ArgonFlourishTriggerSource : DrumSampleTriggerSource
    {
        private readonly HitObjectContainer hitObjectContainer;

        [Resolved]
        private ISkinSource skinSource { get; set; } = null!;

        /// <summary>
        /// The minimum time to leave between flourishes that are added to strong rim hits.
        /// </summary>
        private const double time_between_flourishes = 2000;

        public ArgonFlourishTriggerSource(HitObjectContainer hitObjectContainer)
            : base(hitObjectContainer)
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
                return;

            if (strong && hitType == HitType.Rim && canPlayFlourish(hitObject))
                PlaySamples(new ISampleInfo[] { new VolumeAwareHitSampleInfo(hitObject.CreateHitSampleInfo(HitSampleInfo.HIT_FLOURISH), true) });
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

                // hitObject can be either the strong hit itself (if hit late), or its nested strong object (if hit early)
                // due to `GetMostValidObject()` idiosyncrasies.
                // whichever it is, if we encounter it during iteration, stop looking.
                if (h == hitObject || h.NestedHitObjects.Contains(hitObject))
                    return canFlourish;
            }

            return false;
        }
    }
}
