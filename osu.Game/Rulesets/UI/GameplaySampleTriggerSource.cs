// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Graphics.Containers;
using osu.Game.Audio;
using osu.Game.Rulesets.Objects;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.UI
{
    /// <summary>
    /// A component which can trigger the most appropriate hit sound for a given point in time, based on the state of a <see cref="HitObjectContainer"/>
    /// </summary>
    public class GameplaySampleTriggerSource : CompositeDrawable
    {
        /// <summary>
        /// The number of concurrent samples allowed to be played concurrently so that it feels better when spam-pressing a key.
        /// </summary>
        private const int max_concurrent_hitsounds = OsuGameBase.SAMPLE_CONCURRENCY;

        private readonly HitObjectContainer hitObjectContainer;

        private int nextHitSoundIndex;

        private readonly Container<SkinnableSound> hitSounds;

        public GameplaySampleTriggerSource(HitObjectContainer hitObjectContainer)
        {
            this.hitObjectContainer = hitObjectContainer;

            InternalChild = hitSounds = new Container<SkinnableSound>
            {
                Name = "concurrent sample pool",
                ChildrenEnumerable = Enumerable.Range(0, max_concurrent_hitsounds).Select(_ => new PausableSkinnableSound())
            };
        }

        private HitObjectLifetimeEntry fallbackObject;

        /// <summary>
        /// Play the most appropriate hit sound for the current point in time.
        /// </summary>
        public virtual void Play()
        {
            var nextObject = GetMostValidObject();

            if (nextObject == null)
                return;

            var samples = nextObject.Samples
                                    .Select(s => nextObject.SampleControlPoint.ApplyTo(s))
                                    .Cast<ISampleInfo>()
                                    .ToArray();

            PlaySamples(samples);
        }

        protected void PlaySamples(ISampleInfo[] samples)
        {
            var hitSound = getNextSample();
            hitSound.Samples = samples;
            hitSound.Play();
        }

        protected HitObject GetMostValidObject()
        {
            // The most optimal lookup case we have is when an object is alive. There are usually very few alive objects so there's no drawbacks in attempting this lookup each time.
            var hitObject = hitObjectContainer.AliveObjects.FirstOrDefault(h => h.Result?.HasResult != true)?.HitObject;

            // In the case a next object isn't available in drawable form, we need to do a somewhat expensive traversal to get a valid sound to play.
            if (hitObject == null)
            {
                // This lookup can be skipped if the last entry is still valid (in the future and not yet hit).
                if (fallbackObject == null || fallbackObject.Result?.HasResult == true)
                {
                    // We need to use lifetime entries to find the next object (we can't just use `hitObjectContainer.Objects` due to pooling - it may even be empty).
                    // If required, we can make this lookup more efficient by adding support to get next-future-entry in LifetimeEntryManager.
                    fallbackObject = hitObjectContainer.Entries
                                                       .Where(e => e.Result?.HasResult != true)
                                                       .OrderBy(e => e.HitObject.StartTime)
                                                       .FirstOrDefault();

                    // In the case there are no unjudged objects, the last hit object should be used instead.
                    fallbackObject ??= hitObjectContainer.Entries.LastOrDefault();
                }

                hitObject = fallbackObject?.HitObject;
            }

            return hitObject;
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
