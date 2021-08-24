// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
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
        private readonly HitObjectContainer hitObjectContainer;

        private int nextHitSoundIndex;

        /// <summary>
        /// The number of concurrent samples allowed to be played concurrently so that it feels better when spam-pressing a key.
        /// </summary>
        private const int max_concurrent_hitsounds = OsuGameBase.SAMPLE_CONCURRENCY;

        private readonly Container<SkinnableSound> hitSounds;

        [Resolved]
        private DrawableRuleset drawableRuleset { get; set; }

        public GameplaySampleTriggerSource(HitObjectContainer hitObjectContainer)
        {
            this.hitObjectContainer = hitObjectContainer;
            InternalChildren = new Drawable[]
            {
                hitSounds = new Container<SkinnableSound>
                {
                    Name = "concurrent sample pool",
                    RelativeSizeAxes = Axes.Both,
                    Children = Enumerable.Range(0, max_concurrent_hitsounds).Select(_ => new SkinnableSound()).ToArray()
                },
            };
        }

        private HitObject fallbackObject;

        /// <summary>
        /// Play the most appropriate hit sound for the current point in time.
        /// </summary>
        public void Play()
        {
            var nextObject = hitObjectContainer.AliveObjects.FirstOrDefault(h => h.HitObject.StartTime > Time.Current)?.HitObject;

            if (nextObject == null)
            {
                if (fallbackObject == null || fallbackObject.StartTime < Time.Current)
                {
                    // in the case a next object isn't available in drawable form, we need to do a somewhat expensive traversal to get a valid sound to play.
                    // note that we don't want to cache the object if it is an alive object, as once it is hit we don't want to continue playing its sound.
                    // check whether we can use the previous computed sample.

                    // fallback to non-alive objects to find next off-screen object
                    // TODO: make lookup more efficient?
                    fallbackObject = hitObjectContainer.Entries
                                                       .Where(e => e.Result?.HasResult != true && e.HitObject.StartTime > Time.Current)?
                                                       .OrderBy(e => e.HitObject.StartTime)
                                                       .FirstOrDefault()?.HitObject ?? hitObjectContainer.Entries.FirstOrDefault()?.HitObject;
                }

                nextObject = fallbackObject;
            }

            if (nextObject != null)
            {
                var hitSound = getNextSample();
                hitSound.Samples = GetPlayableSampleInfo(nextObject).Select(s => nextObject.SampleControlPoint.ApplyTo(s)).Cast<ISampleInfo>().ToArray();
                hitSound.Play();
            }
        }

        protected virtual HitSampleInfo[] GetPlayableSampleInfo(HitObject nextObject) =>
            nextObject.Samples.ToArray();

        private SkinnableSound getNextSample()
        {
            var hitSound = hitSounds[nextHitSoundIndex];

            // round robin over available samples to allow for concurrent playback.
            nextHitSoundIndex = (nextHitSoundIndex + 1) % max_concurrent_hitsounds;

            return hitSound;
        }
    }
}
