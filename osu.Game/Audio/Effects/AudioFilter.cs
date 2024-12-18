// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using ManagedBass.Fx;
using osu.Framework.Audio.Mixing;
using osu.Framework.Graphics;

namespace osu.Game.Audio.Effects
{
    public partial class AudioFilter : Component, ITransformableFilter
    {
        /// <summary>
        /// The maximum cutoff frequency that can be used with a low-pass filter.
        /// This is equal to nyquist - 1hz.
        /// </summary>
        public const int MAX_LOWPASS_CUTOFF = 22049; // nyquist - 1hz

        /// <summary>
        /// Whether this filter is currently attached to the audio track and thus applying an adjustment.
        /// </summary>
        public bool IsAttached { get; private set; }

        private readonly AudioMixer mixer;
        private readonly BQFParameters filter;
        private readonly BQFType type;

        private int cutoff;

        /// <summary>
        /// The cutoff frequency of this filter.
        /// </summary>
        public int Cutoff
        {
            get => cutoff;
            set
            {
                if (value == cutoff)
                    return;

                cutoff = value;
                updateFilter();
            }
        }

        /// <summary>
        /// A Component that implements a BASS FX BiQuad Filter Effect.
        /// </summary>
        /// <param name="mixer">The mixer this effect should be applied to.</param>
        /// <param name="type">The type of filter (e.g. LowPass, HighPass, etc)</param>
        public AudioFilter(AudioMixer mixer, BQFType type = BQFType.LowPass)
        {
            this.mixer = mixer;
            this.type = type;

            filter = new BQFParameters
            {
                lFilter = type,
                fBandwidth = 0,
                // This allows fCenter to go up to 22049hz (nyquist - 1hz) without overflowing and causing weird filter behaviour (see: https://www.un4seen.com/forum/?topic=19542.0)
                fQ = 0.7f
            };

            cutoff = getInitialCutoff(type);

            updateFilter();
        }

        private int getInitialCutoff(BQFType type)
        {
            switch (type)
            {
                case BQFType.HighPass:
                    return 1;

                case BQFType.LowPass:
                    return MAX_LOWPASS_CUTOFF;

                default:
                    return 500; // A default that should ensure audio remains audible for other filters.
            }
        }

        private void updateFilter()
        {
            switch (type)
            {
                case BQFType.LowPass:
                    // Workaround for weird behaviour when rapidly setting fCenter of a low-pass filter to nyquist - 1hz.
                    if (Cutoff >= MAX_LOWPASS_CUTOFF)
                    {
                        ensureDetached();
                        return;
                    }

                    break;

                // Workaround for weird behaviour when rapidly setting fCenter of a high-pass filter to 1hz.
                case BQFType.HighPass:
                    if (Cutoff <= 1)
                    {
                        ensureDetached();
                        return;
                    }

                    break;
            }

            ensureAttached();

            filter.fCenter = Cutoff;
            mixer.UpdateEffect(filter);
        }

        private void ensureAttached()
        {
            if (IsAttached)
                return;

            mixer.AddEffect(filter);
            IsAttached = true;
        }

        private void ensureDetached()
        {
            if (!IsAttached)
                return;

            mixer.RemoveEffect(filter);
            IsAttached = false;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            ensureDetached();
        }
    }
}
