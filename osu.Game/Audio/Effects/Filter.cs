// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using ManagedBass.Fx;
using osu.Framework.Audio.Mixing;
using osu.Framework.Bindables;
using osu.Framework.Graphics;

namespace osu.Game.Audio.Effects
{
    public class Filter : Component, ITransformableFilter
    {
        public readonly int MaxCutoff = 22049; // nyquist - 1hz
        private readonly AudioMixer mixer;
        private readonly BQFParameters filter;
        private readonly BQFType type;

        public BindableNumber<int> Cutoff { get; }

        /// <summary>
        /// A Component that implements a BASS FX BiQuad Filter Effect.
        /// </summary>
        /// <param name="mixer">The mixer this effect should be applied to.</param>
        /// <param name="type">The type of filter (e.g. LowPass, HighPass, etc)</param>
        public Filter(AudioMixer mixer, BQFType type = BQFType.LowPass)
        {
            this.mixer = mixer;
            this.type = type;

            int initialCutoff;

            switch (type)
            {
                case BQFType.HighPass:
                    initialCutoff = 1;
                    break;

                case BQFType.LowPass:
                    initialCutoff = MaxCutoff;
                    break;

                default:
                    initialCutoff = 500; // A default that should ensure audio remains audible for other filters.
                    break;
            }

            Cutoff = new BindableNumber<int>
            {
                MinValue = 1,
                MaxValue = MaxCutoff
            };
            filter = new BQFParameters
            {
                lFilter = type,
                fCenter = initialCutoff,
                fBandwidth = 0,
                fQ = 0.7f // This allows fCenter to go up to 22049hz (nyquist - 1hz) without overflowing and causing weird filter behaviour (see: https://www.un4seen.com/forum/?topic=19542.0)
            };

            attachFilter();
            Cutoff.ValueChanged += updateFilter;
            Cutoff.Value = initialCutoff;
        }

        private void attachFilter() => mixer.Effects.Add(filter);

        private void detachFilter() => mixer.Effects.Remove(filter);

        private void updateFilter(ValueChangedEvent<int> cutoff)
        {
            // Workaround for weird behaviour when rapidly setting fCenter of a low-pass filter to nyquist - 1hz.
            if (type == BQFType.LowPass)
            {
                if (cutoff.NewValue >= MaxCutoff)
                {
                    detachFilter();
                    return;
                }

                if (cutoff.OldValue >= MaxCutoff && cutoff.NewValue < MaxCutoff)
                    attachFilter();
            }

            // Workaround for weird behaviour when rapidly setting fCenter of a high-pass filter to 1hz.
            if (type == BQFType.HighPass)
            {
                if (cutoff.NewValue <= 1)
                {
                    detachFilter();
                    return;
                }

                if (cutoff.OldValue <= 1 && cutoff.NewValue > 1)
                    attachFilter();
            }

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
            detachFilter();
        }
    }
}
