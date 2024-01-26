// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Audio;
using osu.Framework.Graphics.Containers;
using osu.Framework.Threading;
using osu.Game.Audio;

namespace osu.Game.Skinning
{
    /// <summary>
    /// A group of (generally gameplay) samples to be played together as one.
    /// Unlike <see cref="SkinnableSample"/>, this uses <see cref="IPooledSampleProvider"/> internally to retrieve all samples from pools when available.
    /// Playback is automatically paused when a <see cref="ISamplePlaybackDisabler"/> is cached.
    /// </summary>
    public partial class SkinnableSamples : SkinReloadableDrawable, IAdjustableAudioComponent
    {
        /// <summary>
        /// The length in milliseconds of the longest sample.
        /// </summary>
        public double Length => !DrawableSamples.Any() ? 0 : DrawableSamples.Max(sample => sample.Length);

        /// <summary>
        /// The minimum allowable volume for <see cref="Samples"/>.
        /// <see cref="Samples"/> that specify a lower <see cref="ISampleInfo.Volume"/> will be forcibly pulled up to this volume.
        /// </summary>
        public int MinimumSampleVolume { get; set; }

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
        /// All raw <see cref="DrawableSamples"/>s contained in this <see cref="SkinnableSamples"/>.
        /// </summary>
        protected IEnumerable<DrawableSample> DrawableSamples => samplesContainer.Select(c => c.Sample).Where(s => s != null);

        private readonly AudioContainer<SkinnableSample> samplesContainer;

        [Resolved]
        private IPooledSampleProvider? samplePool { get; set; }

        /// <summary>
        /// Creates a new <see cref="SkinnableSamples"/> with an initial sample.
        /// </summary>
        /// <param name="sample">The initial sample.</param>
        public SkinnableSamples(ISampleInfo sample)
            : this(new[] { sample })
        {
        }

        /// <summary>
        /// Creates a new <see cref="SkinnableSamples"/> with some initial samples.
        /// </summary>
        /// <param name="samples">The initial samples.</param>
        public SkinnableSamples(IEnumerable<ISampleInfo> samples)
            : this()
        {
            Samples = samples.ToArray();
        }

        /// <summary>
        /// Creates a new <see cref="SkinnableSamples"/>.
        /// </summary>
        public SkinnableSamples()
        {
            InternalChild = samplesContainer = new AudioContainer<SkinnableSample>();
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
                if (samples == value)
                    return;

                samples = value;

                if (LoadState >= LoadState.Ready)
                    updateSamples();
            }
        }

        public void ClearSamples() => Samples = Array.Empty<ISampleInfo>();

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
        public void Play()
        {
            cancelPendingStart();
            RequestedPlaying = true;

            if (samplePlaybackDisabled.Value)
                return;

            play();
        }

        /// <summary>
        /// Stops the samples.
        /// </summary>
        public void Stop()
        {
            cancelPendingStart();
            RequestedPlaying = false;
            stop();
        }

        protected override void LoadAsyncComplete()
        {
            // ensure samples are constructed before SkinChanged() is called via base.LoadAsyncComplete().
            if (!samplesContainer.Any())
                updateSamples();

            base.LoadAsyncComplete();
        }

        private void play()
        {
            FlushPendingSkinChanges();

            samplesContainer.ForEach(c =>
            {
                if (PlayWhenZeroVolume || c.AggregateVolume.Value > 0)
                {
                    c.Stop();
                    c.Play();
                }
            });
        }

        private void stop()
        {
            samplesContainer.ForEach(c => c.Stop());
        }

        private void updateSamples()
        {
            bool wasPlaying = IsPlaying;

            // Remove all pooled samples (return them to the pool), and dispose the rest.
            samplesContainer.RemoveAll(s => s.IsInPool, false);
            samplesContainer.Clear();

            foreach (var s in samples)
            {
                var sample = samplePool?.GetPooledSample(s) ?? new SkinnableSample(s);
                sample.Looping = Looping;
                sample.Volume.Value = Math.Max(s.Volume, MinimumSampleVolume) / 100.0;

                samplesContainer.Add(sample);
            }

            if (wasPlaying && Looping)
                Play();
        }

        #region Pausing support

        public bool RequestedPlaying { get; private set; }

        private readonly IBindable<bool> samplePlaybackDisabled = new Bindable<bool>();

        private ScheduledDelegate? scheduledStart;

        [BackgroundDependencyLoader]
        private void load(ISamplePlaybackDisabler? samplePlaybackDisabler)
        {
            // if in a gameplay context, pause sample playback when gameplay is paused.
            if (samplePlaybackDisabler != null)
            {
                samplePlaybackDisabled.BindTo(samplePlaybackDisabler.SamplePlaybackDisabled);
                samplePlaybackDisabled.BindValueChanged(SamplePlaybackDisabledChanged);
            }
        }

        protected virtual void SamplePlaybackDisabledChanged(ValueChangedEvent<bool> disabled)
        {
            if (!RequestedPlaying) return;

            // let non-looping samples that have already been started play out to completion (sounds better than abruptly cutting off).
            if (!Looping) return;

            cancelPendingStart();

            if (disabled.NewValue)
                stop();
            else
            {
                // schedule so we don't start playing a sample which is no longer alive.
                scheduledStart = Schedule(() =>
                {
                    if (RequestedPlaying)
                        play();
                });
            }
        }

        private void cancelPendingStart()
        {
            scheduledStart?.Cancel();
            scheduledStart = null;
        }

        #endregion

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
        public bool IsPlaying
        {
            get
            {
                foreach (SkinnableSample s in samplesContainer)
                {
                    if (s.Playing)
                        return true;
                }

                return false;
            }
        }

        public bool IsPlayed
        {
            get
            {
                foreach (SkinnableSample s in samplesContainer)
                {
                    if (s.Played)
                        return true;
                }

                return false;
            }
        }

        public IBindable<double> AggregateVolume => samplesContainer.AggregateVolume;

        public IBindable<double> AggregateBalance => samplesContainer.AggregateBalance;

        public IBindable<double> AggregateFrequency => samplesContainer.AggregateFrequency;

        public IBindable<double> AggregateTempo => samplesContainer.AggregateTempo;

        #endregion
    }
}
