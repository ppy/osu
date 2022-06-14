// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Audio;
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

        /// <summary>
        /// All raw <see cref="DrawableSamples"/>s contained in this <see cref="SkinnableSound"/>.
        /// </summary>
        [NotNull, ItemNotNull]
        protected IEnumerable<DrawableSample> DrawableSamples => samplesContainer.Select(c => c.Sample).Where(s => s != null);

        private readonly AudioContainer<PoolableSkinnableSample> samplesContainer;

        [Resolved(CanBeNull = true)]
        private IPooledSampleProvider samplePool { get; set; }

        /// <summary>
        /// Creates a new <see cref="SkinnableSound"/>.
        /// </summary>
        public SkinnableSound()
        {
            InternalChild = samplesContainer = new AudioContainer<PoolableSkinnableSample>();
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

                samplesContainer.ForEach(c => c.Looping = looping);
            }
        }

        /// <summary>
        /// Plays the samples.
        /// </summary>
        public virtual void Play()
        {
            samplesContainer.ForEach(c =>
            {
                if (PlayWhenZeroVolume || c.AggregateVolume.Value > 0)
                {
                    c.Stop();
                    c.Play();
                }
            });
        }

        protected override void LoadAsyncComplete()
        {
            // ensure samples are constructed before SkinChanged() is called via base.LoadAsyncComplete().
            if (!samplesContainer.Any())
                updateSamples();

            base.LoadAsyncComplete();
        }

        /// <summary>
        /// Stops the samples.
        /// </summary>
        public virtual void Stop()
        {
            samplesContainer.ForEach(c => c.Stop());
        }

        private void updateSamples()
        {
            bool wasPlaying = IsPlaying;

            // Remove all pooled samples (return them to the pool), and dispose the rest.
            samplesContainer.RemoveAll(s => s.IsInPool);
            samplesContainer.Clear();

            foreach (var s in samples)
            {
                var sample = samplePool?.GetPooledSample(s) ?? new PoolableSkinnableSample(s);
                sample.Looping = Looping;
                sample.Volume.Value = s.Volume / 100.0;

                samplesContainer.Add(sample);
            }

            if (wasPlaying && Looping)
                Play();
        }

        #region Re-expose AudioContainer

        public BindableNumber<double> Volume => samplesContainer.Volume;

        public BindableNumber<double> Balance => samplesContainer.Balance;

        public BindableNumber<double> Frequency => samplesContainer.Frequency;

        public BindableNumber<double> Tempo => samplesContainer.Tempo;

        public void BindAdjustments(IAggregateAudioAdjustment component) => samplesContainer.BindAdjustments(component);

        public void UnbindAdjustments(IAggregateAudioAdjustment component) => samplesContainer.UnbindAdjustments(component);

        public void AddAdjustment(AdjustableProperty type, IBindable<double> adjustBindable) => samplesContainer.AddAdjustment(type, adjustBindable);

        public void RemoveAdjustment(AdjustableProperty type, IBindable<double> adjustBindable) => samplesContainer.RemoveAdjustment(type, adjustBindable);

        public void RemoveAllAdjustments(AdjustableProperty type) => samplesContainer.RemoveAllAdjustments(type);

        /// <summary>
        /// Whether any samples are currently playing.
        /// </summary>
        public bool IsPlaying => samplesContainer.Any(s => s.Playing);

        public bool IsPlayed => samplesContainer.Any(s => s.Played);

        public IBindable<double> AggregateVolume => samplesContainer.AggregateVolume;

        public IBindable<double> AggregateBalance => samplesContainer.AggregateBalance;

        public IBindable<double> AggregateFrequency => samplesContainer.AggregateFrequency;

        public IBindable<double> AggregateTempo => samplesContainer.AggregateTempo;

        #endregion
    }
}
