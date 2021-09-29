// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using ManagedBass.Fx;
using osu.Framework.Audio.Mixing;
using osu.Framework.Bindables;
using osu.Framework.Graphics;

namespace osu.Game.Audio.Effects
{
    public class Filter : Component
    {
        public BQFType FilterType = BQFType.LowPass;
        public float SweepCutoffStart = 2000;
        public float SweepCutoffEnd = 150;
        public float SweepDuration = 100;
        public Easing SweepEasing = Easing.None;

        public bool IsActive { get; private set; }

        private readonly Bindable<float> filterFreq = new Bindable<float>();
        private readonly AudioMixer mixer;
        private BQFParameters filter;

        /// <summary>
        /// A BiQuad filter that performs a filter-sweep when toggled on or off.
        /// </summary>
        /// <param name="mixer">The mixer this effect should be attached to.</param>
        public Filter(AudioMixer mixer)
        {
            this.mixer = mixer;
        }

        public void Enable()
        {
            attachFilter();
            this.TransformBindableTo(filterFreq, SweepCutoffEnd, SweepDuration, SweepEasing);
        }

        public void Disable()
        {
            this.TransformBindableTo(filterFreq, SweepCutoffStart, SweepDuration, SweepEasing).OnComplete(_ => detatchFilter());
        }

        private void attachFilter()
        {
            if (IsActive) return;

            filter = new BQFParameters
            {
                lFilter = FilterType,
                fCenter = filterFreq.Value = SweepCutoffStart
            };

            mixer.Effects.Add(filter);
            filterFreq.ValueChanged += updateFilter;
            IsActive = true;
        }

        private void detatchFilter()
        {
            if (!IsActive) return;

            filterFreq.ValueChanged -= updateFilter;
            mixer.Effects.Remove(filter);
            IsActive = false;
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
