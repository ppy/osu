// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Audio;

namespace osu.Game.Skinning
{
    /// <summary>
    /// A sound consisting of one or more samples to be played.
    /// </summary>
    public class SkinnableSound : SkinReloadableDrawable, IAdjustableAudioComponent
    {
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

        protected readonly AudioContainer<PoolableSkinnableSample> SamplesContainer;

        [Resolved]
        private ISampleStore sampleStore { get; set; }

        [Resolved(CanBeNull = true)]
        private IPooledSampleProvider samplePool { get; set; }

        /// <summary>
        /// Creates a new <see cref="SkinnableSound"/>.
        /// </summary>
        public SkinnableSound()
        {
            InternalChild = SamplesContainer = new AudioContainer<PoolableSkinnableSample>();
        }

        /// <summary>
        /// Creates a new <see cref="SkinnableSound"/> with some initial samples.
        /// </summary>
        /// <param name="samples">The initial samples.</param>
        public SkinnableSound([NotNull] IEnumerable<ISampleInfo> samples)
            : this()
        {
            this.samples = samples.ToArray();
        }

        /// <summary>
        /// Creates a new <see cref="SkinnableSound"/> with an initial sample.
        /// </summary>
        /// <param name="sample">The initial sample.</param>
        public SkinnableSound([NotNull] ISampleInfo sample)
            : this(new[] { sample })
        {
        }

        private ISampleInfo[] samples = Array.Empty<ISampleInfo>();

        /// <summary>
        /// The samples that should be played.
        /// </summary>
        public ISampleInfo[] Samples
        {
            get => samples;
            set
            {
                value ??= Array.Empty<ISampleInfo>();

                if (samples == value)
                    return;

                samples = value;

                if (LoadState >= LoadState.Ready)
                    updateSamples();
            }
        }

        private bool looping;

        /// <summary>
        /// Whether the samples should loop on completion.
        /// </summary>
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

        /// <summary>
        /// Plays the samples.
        /// </summary>
        public virtual void Play()
        {
            SamplesContainer.ForEach(c =>
            {
                if (PlayWhenZeroVolume || c.AggregateVolume.Value > 0)
                    c.Play();
            });
        }

        /// <summary>
        /// Stops the samples.
        /// </summary>
        public virtual void Stop()
        {
            SamplesContainer.ForEach(c => c.Stop());
        }

        protected override void SkinChanged(ISkinSource skin, bool allowFallback)
        {
            base.SkinChanged(skin, allowFallback);
            updateSamples();
        }

        private void updateSamples()
        {
            bool wasPlaying = IsPlaying;

            // Remove all pooled samples (return them to the pool), and dispose the rest.
            SamplesContainer.RemoveAll(s => s.IsInPool);
            SamplesContainer.Clear();

            foreach (var s in samples)
            {
                var sample = samplePool?.GetPooledSample(s) ?? new PoolableSkinnableSample(s);
                sample.Looping = Looping;
                sample.Volume.Value = s.Volume / 100.0;

                SamplesContainer.Add(sample);
            }

            if (wasPlaying)
                Play();
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

        /// <summary>
        /// Whether any samples are currently playing.
        /// </summary>
        public bool IsPlaying => SamplesContainer.Any(s => s.Playing);

        #endregion
    }
}
