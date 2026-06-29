// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Extensions;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Localisation;
using osu.Game.Overlays;

namespace osu.Game.Screens.Edit.Timing
{
    public partial class FormDiscreteAdjustmentControl<T> : CompositeDrawable, IHasCurrentValue<T>, IFormControl
        where T : struct, INumber<T>, IMinMaxValue<T>, IMultiplyOperators<T, T, T>, IParsable<T>
    {
        public Bindable<T> Current
        {
            get => current.Current;
            set => current.Current = value;
        }

        private readonly BindableNumberWithCurrent<T> current = new BindableNumberWithCurrent<T>();

        private readonly DiscreteAdjustmentControl<T> adjustmentControl;

        private CompositeDrawable? tabbableContentContainer;

        public CompositeDrawable? TabbableContentContainer
        {
            set
            {
                tabbableContentContainer = value;

                if (textBox.IsNotNull())
                    textBox.TabbableContentContainer = tabbableContentContainer;
            }
        }

        private LocalisableString caption;

        /// <summary>
        /// Caption describing this slider bar, displayed on top of the controls.
        /// </summary>
        public LocalisableString Caption
        {
            get => caption;
            set
            {
                caption = value;

                if (IsLoaded)
                    captionText.Caption = value;
            }
        }

        /// <summary>
        /// Hint text containing an extended description of this slider bar, displayed in a tooltip when hovering the caption.
        /// </summary>
        public LocalisableString HintText { get; init; }

        private Func<T, LocalisableString> labelFormat;

        /// <summary>
        /// The string formatting function to use for the value label.
        /// </summary>
        public Func<T, LocalisableString> LabelFormat
        {
            get => labelFormat;
            set
            {
                labelFormat = value;

                if (IsLoaded)
                    updateValueDisplay();
            }
        }

        /// <summary>
        /// The string formatting function to use for the slider's tooltip text.
        /// If not provided, <see cref="LabelFormat"/> is used.
        /// </summary>
        public Func<T, LocalisableString> TooltipFormat { get; init; }

        private FormControlBackground background = null!;
        private FormTextBox.InnerTextBox textBox = null!;
        private OsuSpriteText valueLabel = null!;
        private FormFieldCaption captionText = null!;
        private IFocusManager focusManager = null!;
        private InputManager inputManager = null!;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        private readonly Bindable<Language> currentLanguage = new Bindable<Language>();

        public bool TakeFocus() => GetContainingFocusManager()?.ChangeFocus(textBox) == true;

        public FormDiscreteAdjustmentControl(T baseIncrement)
        {
            labelFormat ??= DefaultLabelFormat;
            TooltipFormat ??= v => LabelFormat(v);

            adjustmentControl = new DiscreteAdjustmentControl<T>(baseIncrement);

            current.ValueChanged += e =>
            {
                ValueChanged?.Invoke();
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuGame? game)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            InternalChildren = new Drawable[]
            {
                background = new FormControlBackground(),
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Padding = new MarginPadding
                    {
                        Vertical = 5,
                        Left = 9,
                        Right = 5,
                    },
                    Children = new Drawable[]
                    {
                        new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Direction = FillDirection.Vertical,
                            Spacing = new osuTK.Vector2(0f, 4f),
                            Width = 0.5f,
                            Padding = new MarginPadding
                            {
                                Right = 10,
                                Vertical = 4,
                            },
                            Children = new Drawable[]
                            {
                                captionText = new FormFieldCaption
                                {
                                    TooltipText = HintText,
                                },
                                new Container
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Children = new Drawable[]
                                    {
                                        textBox = new FormNumberBox.InnerNumberBox(allowDecimals: true)
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            // the textbox is hidden when the control is unfocused,
                                            // but clicking on the label should reach the textbox,
                                            // therefore make it always present.
                                            AlwaysPresent = true,
                                            CommitOnFocusLost = true,
                                            SelectAllOnFocus = true,
                                            OnInputError = background.FlashOnInputError,
                                            TabbableContentContainer = tabbableContentContainer,
                                        },
                                        valueLabel = new TruncatingSpriteText
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            Padding = new MarginPadding { Right = 5 },
                                        },
                                    },
                                },
                            },
                        },
                        adjustmentControl.With(s =>
                        {
                            s.Anchor = Anchor.CentreRight;
                            s.Origin = Anchor.CentreRight;
                            s.Width = 0.5f;
                            s.Action = change => Current.Value += change;
                        })
                    },
                },
            };

            if (game != null)
                currentLanguage.BindTo(game.CurrentLanguage);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            captionText.Caption = caption;

            focusManager = GetContainingFocusManager()!;
            inputManager = GetContainingInputManager()!;

            textBox.Focused.BindValueChanged(_ => updateState());
            textBox.OnCommit += textCommitted;

            currentLanguage.BindValueChanged(_ => Schedule(updateValueDisplay));
            current.BindValueChanged(_ =>
            {
                updateState();
                updateValueDisplay();
            }, true);
        }

        private void textCommitted(TextBox t, bool isNew)
        {
            background.FlashOnCommit();

            try
            {
                if (T.TryParse(t.Text, null, out T parsed))
                    Current.Value = parsed;
            }
            catch
            {
                // TriggerChange below will restore the previous text value on failure.
            }

            // This is run regardless of parsing success as the parsed number may not actually trigger a change
            // due to bindable clamping. Even in such a case we want to update the textbox to a sane visual state.
            Current.TriggerChange();
        }

        protected override bool OnHover(HoverEvent e)
        {
            updateState();
            return true;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            base.OnHoverLost(e);
            updateState();
        }

        protected override bool OnClick(ClickEvent e)
        {
            if (!Current.Disabled)
                focusManager.ChangeFocus(textBox);
            return true;
        }

        private void updateState()
        {
            textBox.ReadOnly = Current.Disabled;
            textBox.Alpha = textBox.Focused.Value ? 1 : 0;
            valueLabel.Alpha = textBox.Focused.Value ? 0 : 1;

            captionText.Colour = Current.Disabled ? colourProvider.Background1 : colourProvider.Content2;
            textBox.Colour = Current.Disabled ? colourProvider.Background1 : colourProvider.Content1;
            valueLabel.Colour = Current.Disabled ? colourProvider.Background1 : colourProvider.Content1;

            if (Current.Disabled)
                background.VisualStyle = VisualStyle.Disabled;
            else if (textBox.Focused.Value)
                background.VisualStyle = VisualStyle.Focused;
            else if (IsHovered || adjustmentControl.Contains(inputManager.CurrentState.Mouse.Position))
                background.VisualStyle = VisualStyle.Hovered;
            else
                background.VisualStyle = VisualStyle.Normal;
        }

        private void updateValueDisplay()
        {
            textBox.Text = NumberFormattingExtensions.Normalise(decimal.CreateTruncating(Current.Value), OsuSliderBar<T>.MAX_DECIMAL_DIGITS).ToString(CultureInfo.CurrentCulture);
            valueLabel.Text = LabelFormat(Current.Value);
        }

        public LocalisableString DefaultLabelFormat(T value) => value.ToStandardFormattedString(OsuSliderBar<T>.MAX_DECIMAL_DIGITS);

        public IEnumerable<LocalisableString> FilterTerms => new[] { Caption, HintText };

        public event Action? ValueChanged;

        public bool IsDefault => Current.IsDefault;

        public void SetDefault() => Current.SetDefault();

        public bool IsDisabled => Current.Disabled;

        public float MainDrawHeight => DrawHeight;
    }
}
