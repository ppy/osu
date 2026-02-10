// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Extensions;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;
using osu.Game.Overlays;
using osuTK.Graphics;
using Vector2 = osuTK.Vector2;

namespace osu.Game.Graphics.UserInterfaceV2
{
    public partial class FormSliderBar<T> : CompositeDrawable, IHasCurrentValue<T>, IFormControl
        where T : struct, INumber<T>, IMinMaxValue<T>
    {
        public Bindable<T> Current
        {
            get => current.Current;
            set
            {
                current.Current = value;

                // the above `Current` set could have disabled the instantaneous bindable too,
                // but we still need to copy out `Default` manually,
                // so lift that disable for a second and then restore it
                currentNumberInstantaneous.Disabled = false;
                currentNumberInstantaneous.Default = current.Default;
                currentNumberInstantaneous.Disabled = current.Disabled;
            }
        }

        private readonly BindableNumberWithCurrent<T> current = new BindableNumberWithCurrent<T>();

        private readonly BindableNumber<T> currentNumberInstantaneous = new BindableNumber<T>();
        private readonly InnerSlider slider;

        /// <summary>
        /// Whether changes to the value should instantaneously transfer to outside bindables.
        /// If <see langword="false"/>, the transfer will happen on text box commit (explicit, or implicit via focus loss), or on slider commit.
        /// </summary>
        public bool TransferValueOnCommit { get; set; }

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

        /// <summary>
        /// A custom step value for each key press which actuates a change on this control.
        /// </summary>
        public float KeyboardStep
        {
            get => slider.KeyboardStep;
            set => slider.KeyboardStep = value;
        }

        /// <summary>
        /// Whether to format the tooltip as a percentage or the actual value.
        /// </summary>
        public bool DisplayAsPercentage { get; init; }

        /// <summary>
        /// Whether sound effects should play when adjusting this slider.
        /// </summary>
        public bool PlaySamplesOnAdjust { get; init; } = true;

        /// <summary>
        /// The string formatting function to use for the value label.
        /// </summary>
        public Func<T, LocalisableString> LabelFormat { get; init; }

        /// <summary>
        /// The string formatting function to use for the slider's tooltip text.
        /// If not provided, <see cref="LabelFormat"/> is used.
        /// </summary>
        public Func<T, LocalisableString> TooltipFormat { get; init; }

        private FormControlBackground background = null!;
        private Box flashLayer = null!;
        private FormTextBox.InnerTextBox textBox = null!;
        private OsuSpriteText valueLabel = null!;
        private FormFieldCaption captionText = null!;
        private IFocusManager focusManager = null!;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        private readonly Bindable<Language> currentLanguage = new Bindable<Language>();

        public bool TakeFocus() => GetContainingFocusManager()?.ChangeFocus(textBox) == true;

        public FormSliderBar()
        {
            LabelFormat ??= defaultLabelFormat;
            TooltipFormat ??= v => LabelFormat(v);

            // the reason why this slider is created in constructor rather than in BDL like the rest of drawable hierarchy is as follows:
            // `SliderBar<T>` (the base framework class for all sliders) also does its `Current` initialisation in its ctor.
            // if that precedent is not followed, it is possible to run into a crippling issue
            // when a `FormSliderBar` instance is on a screen and said screen is exited before said instance's `LoadComplete()` is invoked.
            // in that case, the screen exit will unbind the `InnerSlider`'s internal bindings & value change callbacks:
            // https://github.com/ppy/osu-framework/blob/23ac694fa2c342ce39f563c8a1b975119249d5e9/osu.Framework/Screens/ScreenStack.cs#L353
            // the callbacks are supposed to propagate `{Min,Max}Value` from `Current` to its internal `currentNumberInstantaneous` bindable:
            // https://github.com/ppy/osu-framework/blob/64624795b0816261dfc5e930e1d9b9ec7e8bb8c5/osu.Framework/Graphics/UserInterface/SliderBar.cs#L62-L63
            // thus, the callbacks getting unbound by the screen exit prevents `{Min,Max}Value` from ever correctly propagating, which finally causes a crash at
            // https://github.com/ppy/osu-framework/blob/64624795b0816261dfc5e930e1d9b9ec7e8bb8c5/osu.Framework/Graphics/UserInterface/SliderBar.cs#L112 ->
            // https://github.com/ppy/osu-framework/blob/64624795b0816261dfc5e930e1d9b9ec7e8bb8c5/osu.Framework/Graphics/UserInterface/SliderBar.cs#L88-L92.
            // moving the slider creation & binding to constructor does little to fix the issue other than to make it less likely to be hit.
            slider = new InnerSlider
            {
                Current = currentNumberInstantaneous,
                OnCommit = () => current.Value = currentNumberInstantaneous.Value,
                TooltipFormat = TooltipFormat,
                DisplayAsPercentage = DisplayAsPercentage,
                PlaySamplesOnAdjust = PlaySamplesOnAdjust,
                ResetToDefault = () =>
                {
                    if (!IsDisabled)
                        SetDefault();
                }
            };

            current.ValueChanged += e =>
            {
                currentNumberInstantaneous.Value = e.NewValue;
                ValueChanged?.Invoke();
            };

            current.MinValueChanged += v => currentNumberInstantaneous.MinValue = v;
            current.MaxValueChanged += v => currentNumberInstantaneous.MaxValue = v;
            current.PrecisionChanged += v => currentNumberInstantaneous.Precision = v;
            current.DisabledChanged += disabled =>
            {
                if (disabled)
                {
                    // revert any changes before disabling to make sure we are in a consistent state.
                    currentNumberInstantaneous.Value = current.Value;
                }

                currentNumberInstantaneous.Disabled = disabled;
                if (IsLoaded)
                    updateState();
            };

            current.CopyTo(currentNumberInstantaneous);
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours, OsuGame? game)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            Masking = true;
            CornerRadius = 5;
            CornerExponent = 2.5f;

            InternalChildren = new Drawable[]
            {
                background = new FormControlBackground(),
                flashLayer = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Colour4.Transparent,
                },
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
                            Spacing = new Vector2(0f, 4f),
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
                                            OnInputError = () =>
                                            {
                                                flashLayer.Colour = ColourInfo.GradientVertical(colours.Red3.Opacity(0), colours.Red3);
                                                flashLayer.FadeOutFromOne(200, Easing.OutQuint);
                                            },
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
                        slider.With(s =>
                        {
                            s.Anchor = Anchor.CentreRight;
                            s.Origin = Anchor.CentreRight;
                            s.RelativeSizeAxes = Axes.X;
                            s.Width = 0.5f;
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

            textBox.Focused.BindValueChanged(_ => updateState());
            textBox.OnCommit += textCommitted;
            textBox.Current.BindValueChanged(textChanged);

            slider.IsDragging.BindValueChanged(_ => updateState());
            slider.Focused.BindValueChanged(_ => updateState());

            currentLanguage.BindValueChanged(_ => Schedule(updateValueDisplay));
            currentNumberInstantaneous.BindDisabledChanged(_ => updateState());
            currentNumberInstantaneous.BindValueChanged(e =>
            {
                if (!TransferValueOnCommit)
                    current.Value = e.NewValue;

                updateState();
                updateValueDisplay();
            }, true);
        }

        private bool updatingFromTextBox;

        private void textChanged(ValueChangedEvent<string> change)
        {
            tryUpdateSliderFromTextBox();
        }

        private void textCommitted(TextBox t, bool isNew)
        {
            tryUpdateSliderFromTextBox();
            // If the attempted update above failed, restore text box to match the slider.
            currentNumberInstantaneous.TriggerChange();
            current.Value = currentNumberInstantaneous.Value;

            background.Flash();
        }

        private void tryUpdateSliderFromTextBox()
        {
            updatingFromTextBox = true;

            try
            {
                switch (currentNumberInstantaneous)
                {
                    case Bindable<int> bindableInt:
                        bindableInt.Value = int.Parse(textBox.Current.Value);
                        break;

                    case Bindable<double> bindableDouble:
                        bindableDouble.Value = double.Parse(textBox.Current.Value) / (DisplayAsPercentage ? 100 : 1);
                        break;

                    case Bindable<float> bindableFloat:
                        bindableFloat.Value = float.Parse(textBox.Current.Value) / (DisplayAsPercentage ? 100 : 1);
                        break;

                    default:
                        currentNumberInstantaneous.Parse(textBox.Current.Value, CultureInfo.CurrentCulture);
                        break;
                }
            }
            catch
            {
                // ignore parsing failures.
                // sane state will eventually be restored by a commit (either explicit, or implicit via focus loss).
            }

            updatingFromTextBox = false;
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
            bool childHasFocus = slider.Focused.Value || textBox.Focused.Value;

            textBox.ReadOnly = currentNumberInstantaneous.Disabled;
            textBox.Alpha = textBox.Focused.Value ? 1 : 0;
            valueLabel.Alpha = textBox.Focused.Value ? 0 : 1;

            captionText.Colour = currentNumberInstantaneous.Disabled ? colourProvider.Background1 : colourProvider.Content2;
            textBox.Colour = currentNumberInstantaneous.Disabled ? colourProvider.Background1 : colourProvider.Content1;
            valueLabel.Colour = currentNumberInstantaneous.Disabled ? colourProvider.Background1 : colourProvider.Content1;

            if (Current.Disabled)
                background.VisualStyle = VisualStyle.Disabled;
            else if (childHasFocus)
                background.VisualStyle = VisualStyle.Focused;
            else if (IsHovered || slider.IsDragging.Value)
                background.VisualStyle = VisualStyle.Hovered;
            else
                background.VisualStyle = VisualStyle.Normal;
        }

        private void updateValueDisplay()
        {
            if (updatingFromTextBox) return;

            if (DisplayAsPercentage)
            {
                double floatValue = double.CreateTruncating(currentNumberInstantaneous.Value);

                // if `DisplayAsPercentage` is true and `T` is not `int`, then `Current` / `currentNumberInstantaneous` are in the range of [0,1].
                // in the text box, we want to show the percentage in the range of [0,100], but without the percentage sign.
                // the reason we don't want a percentage sign is that `TextBox`es with numerical `TextInputType`s
                // have framework-side limitations on which characters they accept and they won't accept a percentage sign.
                //
                // therefore, the instantaneous value needs to be multiplied by 100 if it's not `int`, so that `ToStandardFormattedString()`,
                // which is called *intentionally* without `asPercentage: true` specified as to not emit the percentage sign, spits out the correct number.
                //
                // additionally note that `ToStandardFormattedString()`, when called with `asPercentage: true` specified, does the *inverse* of this,
                // which is that it brings the formatted number *into* the [0,1] range,
                // because .NET number formatting *automatically* multiplies the formatted number by 100 when it is told to stringify a number as percentage
                // (https://learn.microsoft.com/en-us/dotnet/standard/base-types/custom-numeric-format-strings#the--custom-specifier-3).
                // it's all very confusing.
                if (currentNumberInstantaneous.Value is not int)
                    floatValue *= 100;

                textBox.Text = floatValue.ToStandardFormattedString(Math.Max(0, OsuSliderBar<T>.MAX_DECIMAL_DIGITS - 2));
            }
            else
                textBox.Text = currentNumberInstantaneous.Value.ToStandardFormattedString(OsuSliderBar<T>.MAX_DECIMAL_DIGITS);

            valueLabel.Text = LabelFormat(currentNumberInstantaneous.Value);
        }

        private LocalisableString defaultLabelFormat(T value) => currentNumberInstantaneous.Value.ToStandardFormattedString(OsuSliderBar<T>.MAX_DECIMAL_DIGITS, DisplayAsPercentage);

        public partial class InnerSlider : OsuSliderBar<T>
        {
            public BindableBool Focused { get; } = new BindableBool();

            public BindableBool IsDragging { get; } = new BindableBool();

            public Action? ResetToDefault { get; init; }

            public Action? OnCommit { get; init; }

            public sealed override LocalisableString TooltipText => base.TooltipText;

            public required Func<T, LocalisableString> TooltipFormat { get; init; }

            private Box leftBox = null!;
            private Box rightBox = null!;
            private InnerSliderNub nub = null!;
            public const float NUB_WIDTH = 10;

            [Resolved]
            private OverlayColourProvider colourProvider { get; set; } = null!;

            [BackgroundDependencyLoader]
            private void load()
            {
                Height = 40;
                RelativeSizeAxes = Axes.X;
                RangePadding = NUB_WIDTH / 2;

                Children = new Drawable[]
                {
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Masking = true,
                        CornerRadius = 5,
                        Children = new Drawable[]
                        {
                            leftBox = new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                            },
                            rightBox = new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Anchor = Anchor.CentreRight,
                                Origin = Anchor.CentreRight,
                            },
                        },
                    },
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Padding = new MarginPadding { Horizontal = RangePadding, },
                        Child = nub = new InnerSliderNub
                        {
                            ResetToDefault = ResetToDefault,
                        }
                    },
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                Current.BindDisabledChanged(_ => updateState(), true);
                FinishTransforms(true);
            }

            protected override void UpdateAfterChildren()
            {
                base.UpdateAfterChildren();
                leftBox.Width = Math.Clamp(RangePadding + nub.DrawPosition.X, 0, Math.Max(0, DrawWidth)) / DrawWidth;
                rightBox.Width = Math.Clamp(DrawWidth - nub.DrawPosition.X - RangePadding, 0, Math.Max(0, DrawWidth)) / DrawWidth;
            }

            protected override bool OnDragStart(DragStartEvent e)
            {
                bool dragging = base.OnDragStart(e);
                IsDragging.Value = dragging;
                updateState();
                return dragging;
            }

            protected override void OnDragEnd(DragEndEvent e)
            {
                base.OnDragEnd(e);
                IsDragging.Value = false;
                updateState();
            }

            protected override bool OnHover(HoverEvent e)
            {
                updateState();
                return base.OnHover(e);
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                updateState();
                base.OnHoverLost(e);
            }

            protected override void OnFocus(FocusEvent e)
            {
                updateState();
                Focused.Value = true;
                base.OnFocus(e);
            }

            protected override void OnFocusLost(FocusLostEvent e)
            {
                updateState();
                Focused.Value = false;
                base.OnFocusLost(e);
            }

            private void updateState()
            {
                rightBox.Colour = colourProvider.Background5;

                Color4 leftColour = colourProvider.Light4;
                Color4 nubColour;

                if (IsHovered || HasFocus || IsDragged)
                    nubColour = colourProvider.Highlight1;
                else
                    nubColour = colourProvider.Highlight1.Darken(0.1f);

                if (Current.Disabled)
                {
                    nubColour = nubColour.Darken(0.4f);
                    leftColour = leftColour.Darken(0.4f);
                }

                leftBox.FadeColour(leftColour, 250, Easing.OutQuint);
                nub.FadeColour(nubColour, 250, Easing.OutQuint);
            }

            protected override void UpdateValue(float value)
            {
                nub.MoveToX(value, 250, Easing.OutElasticQuarter);
            }

            protected override bool Commit()
            {
                bool result = base.Commit();

                if (result)
                    OnCommit?.Invoke();

                return result;
            }

            protected sealed override LocalisableString GetTooltipText(T value) => TooltipFormat(value);
        }

        public partial class InnerSliderNub : Circle
        {
            public Action? ResetToDefault { get; set; }

            [BackgroundDependencyLoader]
            private void load()
            {
                CornerExponent = 2.5f;
                Width = InnerSlider.NUB_WIDTH;
                RelativeSizeAxes = Axes.Y;
                RelativePositionAxes = Axes.X;
                Origin = Anchor.TopCentre;
            }

            protected override bool OnClick(ClickEvent e) => true; // must be handled for double click handler to ever fire

            protected override bool OnDoubleClick(DoubleClickEvent e)
            {
                ResetToDefault?.Invoke();
                return true;
            }
        }

        public IEnumerable<LocalisableString> FilterTerms => new[] { Caption, HintText };

        public event Action? ValueChanged;

        public bool IsDefault => Current.IsDefault;

        public void SetDefault() => Current.SetDefault();

        public bool IsDisabled => Current.Disabled;

        public float MainDrawHeight => DrawHeight;
    }
}
