// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Audio;
using osu.Framework.Graphics.Containers;
using osu.Game.Audio;

namespace osu.Game.Skinning
{
    /// <summary>
    /// A sample corresponding to an <see cref="ISampleInfo"/> that supports being pooled and responding to skin changes.
    /// </summary>
    public class PoolableSkinnableSample : SkinReloadableDrawable, IAggregateAudioAdjustment, IAdjustableAudioComponent
    {
        /// <summary>
        /// The currently-loaded <see cref="DrawableSample"/>.
        /// </summary>
        [CanBeNull]
        public DrawableSample Sample { get; private set; }

        private readonly AudioContainer<DrawableSample> sampleContainer;
        private ISampleInfo sampleInfo;

        [Resolved]
        private ISampleStore sampleStore { get; set; }

        /// <summary>
        /// Creates a new <see cref="PoolableSkinnableSample"/> with no applied <see cref="ISampleInfo"/>.
        /// An <see cref="ISampleInfo"/> can be applied later via <see cref="Apply"/>.
        /// </summary>
        public PoolableSkinnableSample()
        {
            InternalChild = sampleContainer = new AudioContainer<DrawableSample> { RelativeSizeAxes = Axes.Both };
        }

        /// <summary>
        /// Creates a new <see cref="PoolableSkinnableSample"/> with an applied <see cref="ISampleInfo"/>.
        /// </summary>
        /// <param name="sampleInfo">The <see cref="ISampleInfo"/> to attach.</param>
        public PoolableSkinnableSample(ISampleInfo sampleInfo)
            : this()
        {
            Apply(sampleInfo);
        }

        /// <summary>
        /// Applies an <see cref="ISampleInfo"/> that describes the sample to retrieve.
        /// Only one <see cref="ISampleInfo"/> can ever be applied to a <see cref="PoolableSkinnableSample"/>.
        /// </summary>
        /// <param name="sampleInfo">The <see cref="ISampleInfo"/> to apply.</param>
        /// <exception cref="InvalidOperationException">If an <see cref="ISampleInfo"/> has already been applied to this <see cref="PoolableSkinnableSample"/>.</exception>
        public void Apply(ISampleInfo sampleInfo)
        {
            if (this.sampleInfo != null)
                throw new InvalidOperationException($"A {nameof(PoolableSkinnableSample)} cannot be applied multiple {nameof(ISampleInfo)}s.");

            this.sampleInfo = sampleInfo;

            Volume.Value = sampleInfo.Volume / 100.0;

            if (LoadState >= LoadState.Ready)
                updateSample();
        }

        protected override void SkinChanged(ISkinSource skin, bool allowFallback)
        {
            base.SkinChanged(skin, allowFallback);
            updateSample();
        }

        private void updateSample()
        {
            if (sampleInfo == null)
                return;

            bool wasPlaying = Playing;

            sampleContainer.Clear();
            Sample = null;

            var ch = CurrentSkin.GetSample(sampleInfo);

            if (ch == null && AllowDefaultFallback)
            {
                foreach (var lookup in sampleInfo.LookupNames)
                {
                    if ((ch = sampleStore.Get(lookup)) != null)
                        break;
                }
            }

            if (ch == null)
                return;

            sampleContainer.Add(Sample = new DrawableSample(ch) { Looping = Looping });

            // Start playback internally for the new sample if the previous one was playing beforehand.
            if (wasPlaying && Looping)
                Play();
        }

        /// <summary>
        /// Plays the sample.
        /// </summary>
        /// <param name="restart">Whether to play the sample from the beginning.</param>
        public void Play(bool restart = true) => Sample?.Play(restart);

        /// <summary>
        /// Stops the sample.
        /// </summary>
        public void Stop() => Sample?.Stop();

        /// <summary>
        /// Whether the sample is currently playing.
        /// </summary>
        public bool Playing => Sample?.Playing ?? false;

        private bool looping;

        /// <summary>
        /// Whether the sample should loop on completion.
        /// </summary>
        public bool Looping
        {
            get => looping;
            set
            {
                looping = value;

                if (Sample != null)
                    Sample.Looping = value;
            }
        }

        #region Re-expose AudioContainer

        public BindableNumber<double> Volume => sampleContainer.Volume;

        public BindableNumber<double> Balance => sampleContainer.Balance;

        public BindableNumber<double> Frequency => sampleContainer.Frequency;

        public BindableNumber<double> Tempo => sampleContainer.Tempo;

        public void AddAdjustment(AdjustableProperty type, BindableNumber<double> adjustBindable) => sampleContainer.AddAdjustment(type, adjustBindable);

        public void RemoveAdjustment(AdjustableProperty type, BindableNumber<double> adjustBindable) => sampleContainer.RemoveAdjustment(type, adjustBindable);

        public void RemoveAllAdjustments(AdjustableProperty type) => sampleContainer.RemoveAllAdjustments(type);

        public IBindable<double> AggregateVolume => sampleContainer.AggregateVolume;

        public IBindable<double> AggregateBalance => sampleContainer.AggregateBalance;

        public IBindable<double> AggregateFrequency => sampleContainer.AggregateFrequency;

        public IBindable<double> AggregateTempo => sampleContainer.AggregateTempo;

        #endregion
    }
}
