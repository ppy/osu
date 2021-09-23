// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using ManagedBass.Fx;
using osu.Framework.Audio.Mixing;
using osu.Framework.Bindables;
using osu.Framework.Graphics;

namespace osu.Game.Audio.Effects
{
    public class LowPassFilter : Component
    {
        private const float filter_cutoff_start = 2000;
        private const float filter_cutoff_end = 150;
        private const float filter_sweep_duration = 100;
        private readonly Bindable<float> filterFreq = new Bindable<float>(filter_cutoff_start);
        private readonly AudioMixer mixer;
        private readonly BQFParameters filter;

        /// <summary>
        /// A toggle-able low-pass filter with a subtle filter-sweep effect when toggled that can be attached to an <see cref="AudioMixer"/>.
        /// </summary>
        public LowPassFilter(AudioMixer mixer)
        {
            this.mixer = mixer;
            filter = new BQFParameters
            {
                lFilter = BQFType.LowPass,
                fCenter = filterFreq.Value
            };
        }

        public void Enable()
        {
            attachFilter();
            this.TransformBindableTo(filterFreq, filter_cutoff_end, filter_sweep_duration);
        }

        public void Disable()
        {
            this.TransformBindableTo(filterFreq, filter_cutoff_start, filter_sweep_duration)
                .OnComplete(_ => detatchFilter());
        }

        private void attachFilter()
        {
            mixer.Effects.Add(filter);
            filterFreq.ValueChanged += updateFilter;
        }

        private void detatchFilter()
        {
            filterFreq.ValueChanged -= updateFilter;
            mixer.Effects.Remove(filter);
        }

        private void updateFilter(ValueChangedEvent<float> cutoff)
        {
            var filterIndex = mixer.Effects.IndexOf(filter);
            if (filterIndex < 0) return;

            var existingFilter = mixer.Effects[filterIndex] as BQFParameters;
            if (existingFilter == null) return;

            existingFilter.fCenter = cutoff.NewValue;
            mixer.Effects[filterIndex] = existingFilter;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            detatchFilter();
        }
    }
}
