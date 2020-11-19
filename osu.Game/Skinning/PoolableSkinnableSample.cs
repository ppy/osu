// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
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
    public class PoolableSkinnableSample : SkinReloadableDrawable, IAggregateAudioAdjustment, IAdjustableAudioComponent
    {
        private readonly AudioContainer<DrawableSample> sampleContainer;

        private ISampleInfo sampleInfo;
        private DrawableSample sample;

        [Resolved]
        private ISampleStore sampleStore { get; set; }

        public PoolableSkinnableSample()
        {
            InternalChild = sampleContainer = new AudioContainer<DrawableSample> { RelativeSizeAxes = Axes.Both };
        }

        public PoolableSkinnableSample(ISampleInfo sampleInfo)
            : this()
        {
            Apply(sampleInfo);
        }

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
            sampleContainer.Clear();

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

            sampleContainer.Add(sample = new DrawableSample(ch) { Looping = Looping });
        }

        public void Play(bool restart = true) => sample?.Play(restart);

        public void Stop() => sample?.Stop();

        public bool Playing => sample?.Playing ?? false;

        private bool looping;

        public bool Looping
        {
            get => looping;
            set
            {
                looping = value;

                if (sample != null)
                    sample.Looping = value;
            }
        }

        /// <summary>
        /// The volume of this component.
        /// </summary>
        public BindableNumber<double> Volume => sampleContainer.Volume;

        /// <summary>
        /// The playback balance of this sample (-1 .. 1 where 0 is centered)
        /// </summary>
        public BindableNumber<double> Balance => sampleContainer.Balance;

        /// <summary>
        /// Rate at which the component is played back (affects pitch). 1 is 100% playback speed, or default frequency.
        /// </summary>
        public BindableNumber<double> Frequency => sampleContainer.Frequency;

        /// <summary>
        /// Rate at which the component is played back (does not affect pitch). 1 is 100% playback speed.
        /// </summary>
        public BindableNumber<double> Tempo => sampleContainer.Tempo;

        public void AddAdjustment(AdjustableProperty type, BindableNumber<double> adjustBindable) => sampleContainer.AddAdjustment(type, adjustBindable);

        public void RemoveAdjustment(AdjustableProperty type, BindableNumber<double> adjustBindable) => sampleContainer.RemoveAdjustment(type, adjustBindable);

        public void RemoveAllAdjustments(AdjustableProperty type) => sampleContainer.RemoveAllAdjustments(type);

        public IBindable<double> AggregateVolume => sampleContainer.AggregateVolume;

        public IBindable<double> AggregateBalance => sampleContainer.AggregateBalance;

        public IBindable<double> AggregateFrequency => sampleContainer.AggregateFrequency;

        public IBindable<double> AggregateTempo => sampleContainer.AggregateTempo;
    }
}
