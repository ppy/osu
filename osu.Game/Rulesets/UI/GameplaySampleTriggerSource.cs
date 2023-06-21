// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
using osu.Game.Audio;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Play;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.UI
{
    /// <summary>
    /// A component which can trigger the most appropriate hit sound for a given point in time, based on the state of a <see cref="HitObjectContainer"/>
    /// </summary>
    public partial class GameplaySampleTriggerSource : CompositeDrawable
    {
        /// <summary>
        /// The number of concurrent samples allowed to be played concurrently so that it feels better when spam-pressing a key.
        /// </summary>
        private const int max_concurrent_hitsounds = OsuGameBase.SAMPLE_CONCURRENCY;

        private readonly HitObjectContainer hitObjectContainer;

        private int nextHitSoundIndex;

        private readonly Container<SkinnableSound> hitSounds;

        private HitObjectLifetimeEntry? mostValidObject;

        [Resolved]
        private IGameplayClock? gameplayClock { get; set; }

        public GameplaySampleTriggerSource(HitObjectContainer hitObjectContainer)
        {
            this.hitObjectContainer = hitObjectContainer;

            InternalChild = hitSounds = new Container<SkinnableSound>
            {
                Name = "concurrent sample pool",
                ChildrenEnumerable = Enumerable.Range(0, max_concurrent_hitsounds).Select(_ => new PausableSkinnableSound())
            };
        }

        /// <summary>
        /// Play the most appropriate hit sound for the current point in time.
        /// </summary>
        public virtual void Play()
        {
            HitObject? nextObject = GetMostValidObject();

            if (nextObject == null)
                return;

            var samples = nextObject.Samples
                                    .Cast<ISampleInfo>()
                                    .ToArray();

            PlaySamples(samples);
        }

        protected virtual void PlaySamples(ISampleInfo[] samples) => Schedule(() =>
        {
            var hitSound = getNextSample();
            hitSound.Samples = samples;
            hitSound.Play();
        });

        protected HitObject? GetMostValidObject()
        {
            if (mostValidObject == null || isAlreadyHit(mostValidObject))
            {
                // We need to use lifetime entries to find the next object (we can't just use `hitObjectContainer.Objects` due to pooling - it may even be empty).
                // If required, we can make this lookup more efficient by adding support to get next-future-entry in LifetimeEntryManager.
                var candidate =
                    // Use alive entries first as an optimisation.
                    hitObjectContainer.AliveEntries.Select(tuple => tuple.Entry).Where(e => !isAlreadyHit(e)).MinBy(e => e.HitObject.StartTime)
                    ?? hitObjectContainer.Entries.Where(e => !isAlreadyHit(e)).MinBy(e => e.HitObject.StartTime);

                // In the case there are no non-judged objects, the last hit object should be used instead.
                if (candidate == null)
                {
                    mostValidObject = hitObjectContainer.Entries.LastOrDefault();
                }
                else
                {
                    if (isCloseEnoughToCurrentTime(candidate.HitObject))
                    {
                        mostValidObject = candidate;
                    }
                    else
                    {
                        mostValidObject ??= hitObjectContainer.Entries.FirstOrDefault();
                    }
                }
            }

            if (mostValidObject == null)
                return null;

            // If the fallback has been judged then we want the sample from the object itself.
            if (isAlreadyHit(mostValidObject))
                return mostValidObject.HitObject;

            // Else we want the earliest valid nested.
            // In cases of nested objects, they will always have earlier sample data than their parent object.
            return getAllNested(mostValidObject.HitObject).OrderBy(h => h.StartTime).FirstOrDefault(h => h.StartTime > getReferenceTime()) ?? mostValidObject.HitObject;
        }

        private bool isAlreadyHit(HitObjectLifetimeEntry h) => h.Result?.HasResult == true;
        private bool isCloseEnoughToCurrentTime(HitObject h) => getReferenceTime() >= h.StartTime - h.HitWindows.WindowFor(HitResult.Miss) * 2;

        private double getReferenceTime() => (gameplayClock?.CurrentTime ?? Clock.CurrentTime);

        private IEnumerable<HitObject> getAllNested(HitObject hitObject)
        {
            foreach (var h in hitObject.NestedHitObjects)
            {
                yield return h;

                foreach (var n in getAllNested(h))
                    yield return n;
            }
        }

        private SkinnableSound getNextSample()
        {
            SkinnableSound hitSound = hitSounds[nextHitSoundIndex];

            // round robin over available samples to allow for concurrent playback.
            nextHitSoundIndex = (nextHitSoundIndex + 1) % max_concurrent_hitsounds;

            return hitSound;
        }
    }
}
