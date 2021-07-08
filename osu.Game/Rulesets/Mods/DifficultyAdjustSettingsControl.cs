// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Beatmaps;
using osu.Game.Overlays.Settings;

namespace osu.Game.Rulesets.Mods
{
    public class DifficultyAdjustSettingsControl : SettingsItem<float?>
    {
        [Resolved]
        private IBindable<WorkingBeatmap> beatmap { get; set; }

        /// <summary>
        /// Used to track the display value on the setting slider.
        /// This can either be a user override or the beatmap default (when <see cref="Current"/> is null).
        /// </summary>
        private readonly BindableNumber<float> displayNumber = new BindableNumber<float>();

        protected override Drawable CreateControl() => new SliderControl(displayNumber);

        private bool isInternalChange;

        private DifficultyBindable difficultyBindable;

        public override Bindable<float?> Current
        {
            get => base.Current;
            set
            {
                // intercept and extract the DifficultyBindable.
                difficultyBindable = (DifficultyBindable)value;

                // this bind is used to transfer bounds/precision only.
                displayNumber.BindTo(difficultyBindable.CurrentNumber);

                base.Current = value;
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            beatmap.BindValueChanged(b => updateFromDifficulty(), true);

            Current.BindValueChanged(current =>
            {
                // the user override has changed; transfer the correct value to the visual display.
                if (current.NewValue == null)
                    updateFromDifficulty();
                else
                    displayNumber.Value = current.NewValue.Value;
            });

            displayNumber.BindValueChanged(number =>
            {
                if (!isInternalChange)
                    Current.Value = number.NewValue;
            });
        }

        private void updateFromDifficulty()
        {
            var difficulty = beatmap.Value.BeatmapInfo.BaseDifficulty;

            if (difficulty == null)
                return;

            if (Current.Value == null)
            {
                // ensure the beatmap's value is not transferred as a user override.
                isInternalChange = true;
                displayNumber.Value = difficultyBindable.ReadFromDifficulty(difficulty);
                isInternalChange = false;
            }
        }

        private class SliderControl : CompositeDrawable, IHasCurrentValue<float?>
        {
            private readonly BindableWithCurrent<float?> current = new BindableWithCurrent<float?>();

            // Mainly just for fulfilling the interface requirements.
            // The actual update flow is done via the provided number.
            // Of note, this is used for the "reset to default" flow.
            public Bindable<float?> Current
            {
                get => current.Current;
                set => current.Current = value;
            }

            public SliderControl(BindableNumber<float> currentNumber)
            {
                InternalChildren = new Drawable[]
                {
                    new SettingsSlider<float>
                    {
                        ShowsDefaultIndicator = false,
                        Current = currentNumber,
                    }
                };

                AutoSizeAxes = Axes.Y;
                RelativeSizeAxes = Axes.X;
            }
        }
    }
}
