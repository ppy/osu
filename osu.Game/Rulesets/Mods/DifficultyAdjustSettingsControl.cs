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

        protected readonly BindableNumber<float> CurrentNumber = new BindableNumber<float>();

        protected override Drawable CreateControl() => new ControlDrawable(CurrentNumber);

        private bool isInternalChange;

        private DifficultyBindable difficultyBindable;

        public override Bindable<float?> Current
        {
            get => base.Current;
            set
            {
                // intercept and extract the DifficultyBindable.
                difficultyBindable = (DifficultyBindable)value;
                CurrentNumber.BindTo(difficultyBindable.CurrentNumber);
                base.Current = value;
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            beatmap.BindValueChanged(b =>
            {
                updateFromDifficulty();
            }, true);

            Current.BindValueChanged(current =>
            {
                if (current.NewValue == null)
                    updateFromDifficulty();
            });

            CurrentNumber.BindValueChanged(number =>
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
                isInternalChange = true;
                CurrentNumber.Value = difficultyBindable.ReadFromDifficulty(difficulty);
                isInternalChange = false;
            }
        }

        private class ControlDrawable : CompositeDrawable, IHasCurrentValue<float?>
        {
            private readonly BindableWithCurrent<float?> current = new BindableWithCurrent<float?>();

            public Bindable<float?> Current
            {
                get => current.Current;
                set => current.Current = value;
            }

            public ControlDrawable(BindableNumber<float> currentNumber)
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
