// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Localisation;
using osu.Framework.Utils;
using osu.Game.Extensions;

namespace osu.Game.Graphics.UserInterface
{
    public abstract partial class OsuSliderBar<T> : SliderBar<T>, IHasTooltip
        where T : struct, INumber<T>, IMinMaxValue<T>
    {
        public override bool AcceptsFocus => !Current.Disabled;

        public bool PlaySamplesOnAdjust { get; set; } = true;

        /// <summary>
        /// Whether to format the tooltip as a percentage or the actual value.
        /// </summary>
        public bool DisplayAsPercentage { get; set; }

        public virtual LocalisableString TooltipText { get; protected set; }

        /// <summary>
        /// Maximum number of decimal digits to be displayed in the tooltip.
        /// </summary>
        public const int MAX_DECIMAL_DIGITS = 5;

        private Sample sample = null!;

        private double lastSampleTime;
        private T lastSampleValue;

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            sample = audio.Samples.Get(@"UI/notch-tick");
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            CurrentNumber.BindValueChanged(current => TooltipText = GetTooltipText(current.NewValue), true);
        }

        protected override void OnUserChange(T value)
        {
            base.OnUserChange(value);

            playSample(value);

            TooltipText = GetTooltipText(value);
        }

        private void playSample(T value)
        {
            if (!PlaySamplesOnAdjust)
                return;

            if (Clock.CurrentTime - lastSampleTime <= 30)
                return;

            if (value.Equals(lastSampleValue))
                return;

            lastSampleValue = value;
            lastSampleTime = Clock.CurrentTime;

            var channel = sample.GetChannel();

            channel.Frequency.Value = 0.99f + RNG.NextDouble(0.02f) + NormalizedValue * 0.2f;

            // intentionally pitched down, even when hitting max.
            if (NormalizedValue == 0 || NormalizedValue == 1)
                channel.Frequency.Value -= 0.5f;

            channel.Play();
        }

        protected virtual LocalisableString GetTooltipText(T value) => value.ToStandardFormattedString(MAX_DECIMAL_DIGITS, DisplayAsPercentage);
    }
}
