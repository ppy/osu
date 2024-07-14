// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Beatmaps;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Settings;

namespace osu.Game.Rulesets.Mods
{
    public partial class DifficultyAdjustSettingsControl : SettingsItem<float?>
    {
        [Resolved]
        private IBindable<WorkingBeatmap> beatmap { get; set; }

        /// <summary>
        /// Used to track the display value on the setting slider.
        /// </summary>
        /// <remarks>
        /// When the mod is overriding a default, this will match the value of <see cref="Current"/>.
        /// When there is no override (ie. <see cref="Current"/> is null), this value will match the beatmap provided default via <see cref="updateCurrentFromSlider"/>.
        /// </remarks>
        private readonly BindableNumber<float> sliderDisplayCurrent = new BindableNumber<float>();

        protected sealed override Drawable CreateControl() => new SliderControl(sliderDisplayCurrent, CreateSlider);

        protected virtual RoundedSliderBar<float> CreateSlider(BindableNumber<float> current) => new RoundedSliderBar<float>
        {
            RelativeSizeAxes = Axes.X,
            Current = current,
            KeyboardStep = 0.1f,
        };

        /// <summary>
        /// Guards against beatmap values displayed on slider bars being transferred to user override.
        /// </summary>
        private bool isInternalChange;

        private DifficultyBindable difficultyBindable;

        public override Bindable<float?> Current
        {
            get => base.Current;
            set
            {
                // Intercept and extract the internal number bindable from DifficultyBindable.
                // This will provide bounds and precision specifications for the slider bar.
                difficultyBindable = (DifficultyBindable)value.GetBoundCopy();
                sliderDisplayCurrent.BindTo(difficultyBindable.CurrentNumber);

                base.Current = difficultyBindable;
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Current.BindValueChanged(_ => updateCurrentFromSlider());
            beatmap.BindValueChanged(_ => updateCurrentFromSlider(), true);

            sliderDisplayCurrent.BindValueChanged(number =>
            {
                // this handles the transfer of the slider value to the main bindable.
                // as such, should be skipped if the slider is being updated via updateFromDifficulty().
                if (!isInternalChange)
                    Current.Value = number.NewValue;
            });
        }

        private void updateCurrentFromSlider()
        {
            if (Current.Value != null)
            {
                // a user override has been added or updated.
                sliderDisplayCurrent.Value = Current.Value.Value;
                return;
            }

            var difficulty = beatmap.Value.BeatmapInfo.Difficulty;

            // generally should always be implemented, else the slider will have a zero default.
            if (difficultyBindable.ReadCurrentFromDifficulty == null)
                return;

            isInternalChange = true;
            sliderDisplayCurrent.Value = difficultyBindable.ReadCurrentFromDifficulty(difficulty);
            isInternalChange = false;
        }

        private partial class SliderControl : CompositeDrawable, IHasCurrentValue<float?>
        {
            // This is required as SettingsItem relies heavily on this bindable for internal use.
            // The actual update flow is done via the bindable provided in the constructor.
            private readonly DifficultyBindableWithCurrent current = new DifficultyBindableWithCurrent();

            public Bindable<float?> Current
            {
                get => current.Current;
                set => current.Current = value;
            }

            public SliderControl(BindableNumber<float> currentNumber, Func<BindableNumber<float>, RoundedSliderBar<float>> createSlider)
            {
                InternalChildren = new Drawable[]
                {
                    createSlider(currentNumber)
                };

                AutoSizeAxes = Axes.Y;
                RelativeSizeAxes = Axes.X;
            }
        }

        private class DifficultyBindableWithCurrent : DifficultyBindable, IHasCurrentValue<float?>
        {
            private Bindable<float?> currentBound;

            public Bindable<float?> Current
            {
                get => this;
                set
                {
                    ArgumentNullException.ThrowIfNull(value);

                    if (currentBound != null) UnbindFrom(currentBound);
                    BindTo(currentBound = value);
                }
            }

            public DifficultyBindableWithCurrent(float? defaultValue = default)
                : base(defaultValue)
            {
            }

            protected override Bindable<float?> CreateInstance() => new DifficultyBindableWithCurrent();
        }
    }
}
