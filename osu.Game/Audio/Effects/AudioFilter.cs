// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using ManagedBass.Fx;
using osu.Framework.Audio.Mixing;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Logging;

namespace osu.Game.Audio.Effects
{
    public class AudioFilter : Component, ITransformableFilter
    {
        /// <summary>
        /// The maximum cutoff frequency that can be used with a low-pass filter.
        /// This is equal to nyquist - 1hz.
        /// </summary>
        public const int MAX_LOWPASS_CUTOFF = 22049; // nyquist - 1hz

        private readonly AudioMixer mixer;
        private readonly BQFParameters filter;
        private readonly BQFType type;

        /// <summary>
        /// The current cutoff of this filter.
        /// </summary>
        public BindableNumber<int> Cutoff { get; }

        /// <summary>
        /// A Component that implements a BASS FX BiQuad Filter Effect.
        /// </summary>
        /// <param name="mixer">The mixer this effect should be applied to.</param>
        /// <param name="type">The type of filter (e.g. LowPass, HighPass, etc)</param>
        public AudioFilter(AudioMixer mixer, BQFType type = BQFType.LowPass)
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
                    initialCutoff = MAX_LOWPASS_CUTOFF;
                    break;

                default:
                    initialCutoff = 500; // A default that should ensure audio remains audible for other filters.
                    break;
            }

            Cutoff = new BindableNumber<int>(initialCutoff)
            {
                MinValue = 1,
                MaxValue = MAX_LOWPASS_CUTOFF
            };

            filter = new BQFParameters
            {
                lFilter = type,
                fCenter = initialCutoff,
                fBandwidth = 0,
                fQ = 0.7f // This allows fCenter to go up to 22049hz (nyquist - 1hz) without overflowing and causing weird filter behaviour (see: https://www.un4seen.com/forum/?topic=19542.0)
            };

            // Don't start attached if this is low-pass or high-pass filter (as they have special auto-attach/detach logic)
            if (type != BQFType.LowPass && type != BQFType.HighPass)
                attachFilter();

            Cutoff.ValueChanged += updateFilter;
        }

        private void attachFilter()
        {
            Debug.Assert(!mixer.Effects.Contains(filter));
            mixer.Effects.Add(filter);
        }

        private void detachFilter()
        {
            Debug.Assert(mixer.Effects.Contains(filter));
            mixer.Effects.Remove(filter);
        }

        private void updateFilter(ValueChangedEvent<int> cutoff)
        {
            // Workaround for weird behaviour when rapidly setting fCenter of a low-pass filter to nyquist - 1hz.
            if (type == BQFType.LowPass)
            {
                if (cutoff.NewValue >= MAX_LOWPASS_CUTOFF)
                {
                    detachFilter();
                    return;
                }

                if (cutoff.OldValue >= MAX_LOWPASS_CUTOFF && cutoff.NewValue < MAX_LOWPASS_CUTOFF)
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

            if (mixer.Effects[filterIndex] is BQFParameters existingFilter)
            {
                existingFilter.fCenter = cutoff.NewValue;

                // required to update effect with new parameters.
                mixer.Effects[filterIndex] = existingFilter;
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            try
            {
                if (mixer.Effects.Contains(filter))
                    detachFilter();
            }
            catch (Exception e)
            {
                Logger.Log($"Exception in audio filter disposal: {e}");
            }
        }
    }
}
