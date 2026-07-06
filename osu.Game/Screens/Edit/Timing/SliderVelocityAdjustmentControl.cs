// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Localisation;
using osuTK;

namespace osu.Game.Screens.Edit.Timing
{
    public partial class SliderVelocityAdjustmentControl : CompositeDrawable, IHasCurrentValue<double>
    {
        public Bindable<double> Current
        {
            get => current.Current;
            set => current.Current = value;
        }

        private readonly BindableNumberWithCurrent<double> current = new BindableNumberWithCurrent<double>(1)
        {
            Precision = 0.01,
            MinValue = 0.1,
            MaxValue = 10
        };

        /// <summary>
        /// This is a hack to allow the text box to show an indication that multiple slider velocity values are active
        /// when the selection contains multiple objects with different velocities.
        /// </summary>
        public bool IsMultipleValues
        {
            get => isMultipleValues;
            set
            {
                if (isMultipleValues == value)
                    return;

                isMultipleValues = value;
                updateIndeterminateState();
            }
        }

        private bool isMultipleValues;

        private FormDiscreteAdjustmentControl<double> control = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            InternalChild = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(5),
                Children = new Drawable[]
                {
                    control = new FormDiscreteAdjustmentControl<double>(0.05)
                    {
                        Caption = "Slider velocity",
                        Current = Current,
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            control.TextBox.Focused.BindValueChanged(focused =>
            {
                if (focused.NewValue && IsMultipleValues)
                    control.TextBox.Text = string.Empty;
            });
            updateIndeterminateState();
        }

        private void updateIndeterminateState()
        {
            control.LabelFormat = IsMultipleValues
                ? static _ => "(multiple)"
                : v => LocalisableString.Interpolate($"{v:0.00}x");
            control.TextBox.PlaceholderText = IsMultipleValues ? "(multiple)" : string.Empty;
        }

        public bool TakeFocus() => GetContainingFocusManager()!.ChangeFocus(control.TextBox);
    }
}
