// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics.Audio;
using osu.Framework.Graphics.Containers;
using osu.Game.Audio;
using osu.Game.Screens.Play;

namespace osu.Game.Skinning
{
    public class SkinnableSound : SkinReloadableDrawable, IAdjustableAudioComponent
    {
        private readonly ISampleInfo[] hitSamples;

        [Resolved]
        private ISampleStore samples { get; set; }

        /// <summary>
        /// Whether playback of this sound has been requested, regardless of whether it could be played or not (due to being paused, for instance).
        /// </summary>
        protected bool PlaybackRequested;

        public override bool RemoveWhenNotAlive => false;
        public override bool RemoveCompletedTransforms => false;

        /// <summary>
        /// Whether to play the underlying sample when aggregate volume is zero.
        /// Note that this is checked at the point of calling <see cref="Play"/>; changing the volume post-play will not begin playback.
        /// Defaults to false unless <see cref="Looping"/>.
        /// </summary>
        /// <remarks>
        /// Can serve as an optimisation if it is known ahead-of-time that this behaviour is allowed in a given use case.
        /// </remarks>
        protected bool PlayWhenZeroVolume => Looping;

        protected virtual bool PlayWhenPaused => false;

        protected readonly AudioContainer<DrawableSample> SamplesContainer;

        public SkinnableSound(ISampleInfo hitSamples)
            : this(new[] { hitSamples })
        {
        }

        public SkinnableSound(IEnumerable<ISampleInfo> hitSamples)
        {
            this.hitSamples = hitSamples.ToArray();
            InternalChild = SamplesContainer = new AudioContainer<DrawableSample>();
        }

        private readonly IBindable<bool> samplePlaybackDisabled = new Bindable<bool>();

        [BackgroundDependencyLoader(true)]
        private void load(ISamplePlaybackDisabler samplePlaybackDisabler)
        {
            // if in a gameplay context, pause sample playback when gameplay is paused.
            if (samplePlaybackDisabler != null)
            {
                samplePlaybackDisabled.BindTo(samplePlaybackDisabler.SamplePlaybackDisabled);
                samplePlaybackDisabled.BindValueChanged(disabled =>
                {
                    if (PlaybackRequested)
                    {
                        if (disabled.NewValue && !PlayWhenPaused)
                            stop();
                        // it's not easy to know if a sample has finished playing (to end).
                        // to keep things simple only resume playing looping samples.
                        else if (Looping)
                            play();
                    }
                });
            }
        }

        private bool looping;

        public bool Looping
        {
            get => looping;
            set
            {
                if (value == looping) return;

                looping = value;

                SamplesContainer.ForEach(c => c.Looping = looping);
            }
        }

        public void Play()
        {
            PlaybackRequested = true;
            play();
        }

        private void play()
        {
            if (samplePlaybackDisabled.Value && !PlayWhenPaused)
                return;

            SamplesContainer.ForEach(c =>
            {
                if (PlayWhenZeroVolume || c.AggregateVolume.Value > 0)
                    c.Play();
            });
        }

        public void Stop()
        {
            PlaybackRequested = false;
            stop();
        }

        private void stop()
        {
            SamplesContainer.ForEach(c => c.Stop());
        }

        protected override void SkinChanged(ISkinSource skin, bool allowFallback)
        {
            bool wasPlaying = IsPlaying;

            var channels = hitSamples.Select(s =>
            {
                var ch = skin.GetSample(s);

                if (ch == null && allowFallback)
                {
                    foreach (var lookup in s.LookupNames)
                    {
                        if ((ch = samples.Get($"Gameplay/{lookup}")) != null)
                            break;
                    }
                }

                if (ch != null)
                {
                    ch.Looping = looping;
                    ch.Volume.Value = s.Volume / 100.0;
                }

                return ch;
            }).Where(c => c != null);

            SamplesContainer.ChildrenEnumerable = channels.Select(c => new DrawableSample(c));

            // Start playback internally for the new samples if the previous ones were playing beforehand.
            if (wasPlaying)
                play();
        }

        #region Re-expose AudioContainer

        public BindableNumber<double> Volume => SamplesContainer.Volume;

        public BindableNumber<double> Balance => SamplesContainer.Balance;

        public BindableNumber<double> Frequency => SamplesContainer.Frequency;

        public BindableNumber<double> Tempo => SamplesContainer.Tempo;

        public void AddAdjustment(AdjustableProperty type, BindableNumber<double> adjustBindable)
            => SamplesContainer.AddAdjustment(type, adjustBindable);

        public void RemoveAdjustment(AdjustableProperty type, BindableNumber<double> adjustBindable)
            => SamplesContainer.RemoveAdjustment(type, adjustBindable);

        public void RemoveAllAdjustments(AdjustableProperty type)
            => SamplesContainer.RemoveAllAdjustments(type);

        public bool IsPlaying => SamplesContainer.Any(s => s.Playing);

        #endregion
    }
}
