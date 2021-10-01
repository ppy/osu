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
        public readonly int MaxCutoff;
        private readonly AudioMixer mixer;
        private readonly BQFParameters filter;
        private readonly BQFType type;

        public BindableNumber<int> Cutoff { get; }

        /// <summary>
        /// A BiQuad filter that performs a filter-sweep when toggled on or off.
        /// </summary>
        /// <param name="mixer">The mixer this effect should be attached to.</param>
        /// <param name="type">The type of filter (e.g. LowPass, HighPass, etc)</param>
        public Filter(AudioMixer mixer, BQFType type = BQFType.LowPass)
        {
            this.mixer = mixer;
            this.type = type;

            var initialCutoff = 1;

            // These max cutoff values are a work-around for BASS' BiQuad filters behaving weirdly when approaching nyquist.
            // Note that these values assume a sample rate of 44100 (as per BassAudioMixer in osu.Framework)
            // See also https://www.un4seen.com/forum/?topic=19542.0 for more information.
            switch (type)
            {
                case BQFType.HighPass:
                    MaxCutoff = 21968; // beyond this value, the high-pass cuts out
                    break;

                case BQFType.LowPass:
                    MaxCutoff = initialCutoff = 14000; // beyond (roughly) this value, the low-pass filter audibly wraps/reflects
                    break;

                case BQFType.BandPass:
                    MaxCutoff = 16000; // beyond (roughly) this value, the band-pass filter audibly wraps/reflects
                    break;

                default:
                    MaxCutoff = 22050; // default to nyquist for other filter types, TODO: handle quirks of other filter types
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
                fCenter = initialCutoff
            };

            attachFilter();

            Cutoff.ValueChanged += updateFilter;
            Cutoff.Value = initialCutoff;
        }

        private void attachFilter() => mixer.Effects.Add(filter);

        private void detachFilter() => mixer.Effects.Remove(filter);

        private void updateFilter(ValueChangedEvent<int> cutoff)
        {
            // This is another workaround for quirks in BASS' BiQuad filters.
            // Because the cutoff can't be set above ~14khz (i.e. outside of human hearing range) without the aforementioned wrapping/reflecting quirk occuring, we instead
            // remove the effect from the mixer when the cutoff is at maximum so that a LowPass filter isn't always attenuating high frequencies just by existing.
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
