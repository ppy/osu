// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
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
    public partial class SkinnableSample : SkinReloadableDrawable, IAdjustableAudioComponent
    {
        /// <summary>
        /// The currently-loaded <see cref="DrawableSample"/>.
        /// </summary>
        [CanBeNull]
        public DrawableSample Sample { get; private set; }

        private readonly AudioContainer<DrawableSample> sampleContainer;
        private ISampleInfo sampleInfo;
        private SampleChannel activeChannel;

        /// <summary>
        /// Creates a new <see cref="SkinnableSample"/> with no applied <see cref="ISampleInfo"/>.
        /// An <see cref="ISampleInfo"/> can be applied later via <see cref="Apply"/>.
        /// </summary>
        public SkinnableSample()
        {
            InternalChild = sampleContainer = new AudioContainer<DrawableSample> { RelativeSizeAxes = Axes.Both };
        }

        /// <summary>
        /// Creates a new <see cref="SkinnableSample"/> with an applied <see cref="ISampleInfo"/>.
        /// </summary>
        /// <param name="sampleInfo">The <see cref="ISampleInfo"/> to attach.</param>
        public SkinnableSample(ISampleInfo sampleInfo)
            : this()
        {
            Apply(sampleInfo);
        }

        /// <summary>
        /// Applies an <see cref="ISampleInfo"/> that describes the sample to retrieve.
        /// Only one <see cref="ISampleInfo"/> can ever be applied to a <see cref="SkinnableSample"/>.
        /// </summary>
        /// <param name="sampleInfo">The <see cref="ISampleInfo"/> to apply.</param>
        /// <exception cref="InvalidOperationException">If an <see cref="ISampleInfo"/> has already been applied to this <see cref="SkinnableSample"/>.</exception>
        public void Apply(ISampleInfo sampleInfo)
        {
            if (this.sampleInfo != null)
                throw new InvalidOperationException($"A {nameof(SkinnableSample)} cannot be applied multiple {nameof(ISampleInfo)}s.");

            this.sampleInfo = sampleInfo;

            Volume.Value = sampleInfo.Volume / 100.0;

            if (LoadState >= LoadState.Ready)
                updateSample();
        }

        protected override void SkinChanged(ISkinSource skin)
        {
            base.SkinChanged(skin);
            updateSample();
        }

        /// <summary>
        /// Whether this sample was playing before a skin source change.
        /// </summary>
        private bool wasPlaying;

        private void clearPreviousSamples()
        {
            // only run if the samples aren't already cleared.
            // this ensures the "wasPlaying" state is stored correctly even if multiple clear calls are executed.
            if (!sampleContainer.Any()) return;

            wasPlaying = Playing;

            sampleContainer.Clear();
            Sample = null;
            activeChannel = null;
        }

        private void updateSample()
        {
            clearPreviousSamples();

            if (sampleInfo == null)
                return;

            var sample = CurrentSkin.GetSample(sampleInfo);

            if (sample == null)
                return;

            sampleContainer.Add(Sample = new DrawableSample(sample));

            // Start playback internally for the new sample if the previous one was playing beforehand.
            if (wasPlaying && Looping)
                Play();
        }

        /// <summary>
        /// Plays the sample.
        /// </summary>
        public void Play()
        {
            FlushPendingSkinChanges();

            if (Sample == null)
                return;

            activeChannel = Sample.GetChannel();
            activeChannel.Looping = Looping;
            activeChannel.Play();

            WasPlayed = true;
        }

        /// <summary>
        /// Stops the sample.
        /// </summary>
        public void Stop()
        {
            activeChannel?.Stop();
            activeChannel = null;
        }

        /// <summary>
        /// Whether the sample is currently playing.
        /// </summary>
        public bool Playing => activeChannel?.Playing ?? false;

        /// <summary>
        /// Whether the sample was ever started. Becomes <c>true</c> on <see cref="Play"/> and never changes after that point.
        /// </summary>
        public bool WasPlayed { get; private set; }

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

                if (activeChannel != null)
                    activeChannel.Looping = value;
            }
        }

        #region Re-expose AudioContainer

        public BindableNumber<double> Volume => sampleContainer.Volume;

        public BindableNumber<double> Balance => sampleContainer.Balance;

        public BindableNumber<double> Frequency => sampleContainer.Frequency;

        public BindableNumber<double> Tempo => sampleContainer.Tempo;

        public void BindAdjustments(IAggregateAudioAdjustment component) => sampleContainer.BindAdjustments(component);

        public void UnbindAdjustments(IAggregateAudioAdjustment component) => sampleContainer.UnbindAdjustments(component);

        public void AddAdjustment(AdjustableProperty type, IBindable<double> adjustBindable) => sampleContainer.AddAdjustment(type, adjustBindable);

        public void RemoveAdjustment(AdjustableProperty type, IBindable<double> adjustBindable) => sampleContainer.RemoveAdjustment(type, adjustBindable);

        public void RemoveAllAdjustments(AdjustableProperty type) => sampleContainer.RemoveAllAdjustments(type);

        public IBindable<double> AggregateVolume => sampleContainer.AggregateVolume;

        public IBindable<double> AggregateBalance => sampleContainer.AggregateBalance;

        public IBindable<double> AggregateFrequency => sampleContainer.AggregateFrequency;

        public IBindable<double> AggregateTempo => sampleContainer.AggregateTempo;

        #endregion
    }
}
