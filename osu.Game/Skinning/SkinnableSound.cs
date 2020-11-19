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
        private IPooledSampleProvider pooledProvider { get; set; }

        public SkinnableSound()
        {
            InternalChild = SamplesContainer = new AudioContainer<PoolableSkinnableSample>();
        }

        public SkinnableSound([NotNull] IEnumerable<ISampleInfo> samples)
            : this()
        {
            this.samples = samples.ToArray();
        }

        public SkinnableSound([NotNull] ISampleInfo sample)
            : this(new[] { sample })
        {
        }

        private ISampleInfo[] samples;

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

        public virtual void Play()
        {
            SamplesContainer.ForEach(c =>
            {
                if (PlayWhenZeroVolume || c.AggregateVolume.Value > 0)
                    c.Play();
            });
        }

        public virtual void Stop()
        {
            SamplesContainer.ForEach(c => c.Stop());
        }

        protected override void SkinChanged(ISkinSource skin, bool allowFallback)
        {
            // Start playback internally for the new samples if the previous ones were playing beforehand.
            if (IsPlaying)
                Play();
        }

        private void updateSamples()
        {
            bool wasPlaying = IsPlaying;

            // Remove all pooled samples (return them to the pool), and dispose the rest.
            SamplesContainer.RemoveAll(s => s.IsInPool);
            SamplesContainer.Clear();

            foreach (var s in samples)
            {
                var sample = pooledProvider?.GetPooledSample(s) ?? new PoolableSkinnableSample(s);
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

        public bool IsPlaying => SamplesContainer.Any(s => s.Playing);

        #endregion
    }
}
