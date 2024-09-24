// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
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
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;

namespace osu.Game.Graphics.UserInterfaceV2
{
    public partial class FormSliderBar<T> : CompositeDrawable, IHasCurrentValue<T>
        where T : struct, INumber<T>, IMinMaxValue<T>
    {
        public Bindable<T> Current
        {
            get => current.Current;
            set => current.Current = value;
        }

        private bool instantaneous = true;

        /// <summary>
        /// Whether changes to the slider should instantaneously transfer to the text box (and vice versa).
        /// If <see langword="false"/>, the transfer will happen on text box commit (explicit, or implicit via focus loss), or on slider drag end.
        /// </summary>
        public bool Instantaneous
        {
            get => instantaneous;
            set
            {
                instantaneous = value;

                if (slider.IsNotNull())
                    slider.TransferValueOnCommit = !instantaneous;
            }
        }

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

        private readonly BindableNumberWithCurrent<T> current = new BindableNumberWithCurrent<T>();

        /// <summary>
        /// Caption describing this slider bar, displayed on top of the controls.
        /// </summary>
        public LocalisableString Caption { get; init; }

        /// <summary>
        /// Hint text containing an extended description of this slider bar, displayed in a tooltip when hovering the caption.
        /// </summary>
        public LocalisableString HintText { get; init; }

        private Box background = null!;
        private Box flashLayer = null!;
        private FormTextBox.InnerTextBox textBox = null!;
        private Slider slider = null!;
        private FormFieldCaption caption = null!;
        private IFocusManager focusManager = null!;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            RelativeSizeAxes = Axes.X;
            Height = 50;

            Masking = true;
            CornerRadius = 5;

            InternalChildren = new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colourProvider.Background5,
                },
                flashLayer = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Colour4.Transparent,
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding(9),
                    Children = new Drawable[]
                    {
                        caption = new FormFieldCaption
                        {
                            Anchor = Anchor.TopLeft,
                            Origin = Anchor.TopLeft,
                            Caption = Caption,
                            TooltipText = HintText,
                        },
                        textBox = new FormNumberBox.InnerNumberBox
                        {
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            RelativeSizeAxes = Axes.X,
                            Width = 0.5f,
                            CommitOnFocusLost = true,
                            SelectAllOnFocus = true,
                            AllowDecimals = true,
                            OnInputError = () =>
                            {
                                flashLayer.Colour = ColourInfo.GradientVertical(colours.Red3.Opacity(0), colours.Red3);
                                flashLayer.FadeOutFromOne(200, Easing.OutQuint);
                            },
                            TabbableContentContainer = tabbableContentContainer,
                        },
                        slider = new Slider
                        {
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreRight,
                            RelativeSizeAxes = Axes.X,
                            Width = 0.5f,
                            Current = Current,
                            TransferValueOnCommit = !instantaneous,
                        }
                    },
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            focusManager = GetContainingFocusManager()!;

            textBox.Focused.BindValueChanged(_ => updateState());
            textBox.OnCommit += textCommitted;
            textBox.Current.BindValueChanged(textChanged);

            slider.IsDragging.BindValueChanged(_ => updateState());

            current.BindValueChanged(_ =>
            {
                updateState();
                updateTextBoxFromSlider();
            }, true);
        }

        private bool updatingFromTextBox;

        private void textChanged(ValueChangedEvent<string> change)
        {
            if (!instantaneous) return;

            tryUpdateSliderFromTextBox();
        }

        private void textCommitted(TextBox t, bool isNew)
        {
            tryUpdateSliderFromTextBox();

            // If the attempted update above failed, restore text box to match the slider.
            Current.TriggerChange();

            flashLayer.Colour = ColourInfo.GradientVertical(colourProvider.Dark2.Opacity(0), colourProvider.Dark2);
            flashLayer.FadeOutFromOne(800, Easing.OutQuint);
        }

        private void tryUpdateSliderFromTextBox()
        {
            updatingFromTextBox = true;

            try
            {
                switch (Current)
                {
                    case Bindable<int> bindableInt:
                        bindableInt.Value = int.Parse(textBox.Current.Value);
                        break;

                    case Bindable<double> bindableDouble:
                        bindableDouble.Value = double.Parse(textBox.Current.Value);
                        break;

                    default:
                        Current.Parse(textBox.Current.Value, CultureInfo.CurrentCulture);
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
            focusManager.ChangeFocus(textBox);
            return true;
        }

        private void updateState()
        {
            textBox.Alpha = 1;

            background.Colour = Current.Disabled ? colourProvider.Background4 : colourProvider.Background5;
            caption.Colour = Current.Disabled ? colourProvider.Foreground1 : colourProvider.Content2;
            textBox.Colour = Current.Disabled ? colourProvider.Foreground1 : colourProvider.Content1;

            BorderThickness = IsHovered || textBox.Focused.Value || slider.IsDragging.Value ? 2 : 0;
            BorderColour = textBox.Focused.Value ? colourProvider.Highlight1 : colourProvider.Light4;

            if (textBox.Focused.Value)
                background.Colour = ColourInfo.GradientVertical(colourProvider.Background5, colourProvider.Dark3);
            else if (IsHovered || slider.IsDragging.Value)
                background.Colour = ColourInfo.GradientVertical(colourProvider.Background5, colourProvider.Dark4);
            else
                background.Colour = colourProvider.Background5;
        }

        private void updateTextBoxFromSlider()
        {
            if (updatingFromTextBox) return;

            textBox.Text = slider.GetDisplayableValue(Current.Value).ToString();
        }

        private partial class Slider : OsuSliderBar<T>
        {
            public BindableBool IsDragging { get; set; } = new BindableBool();

            private Box leftBox = null!;
            private Box rightBox = null!;
            private Circle nub = null!;
            private const float nub_width = 10;

            [Resolved]
            private OverlayColourProvider colourProvider { get; set; } = null!;

            [BackgroundDependencyLoader]
            private void load()
            {
                Height = 40;
                RelativeSizeAxes = Axes.X;
                RangePadding = nub_width / 2;
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
                        Child = nub = new Circle
                        {
                            Width = nub_width,
                            RelativeSizeAxes = Axes.Y,
                            RelativePositionAxes = Axes.X,
                            Origin = Anchor.TopCentre,
                        }
                    },
                    new HoverClickSounds()
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                updateState();
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

            private void updateState()
            {
                rightBox.Colour = colourProvider.Background6;
                leftBox.Colour = IsHovered || IsDragged ? colourProvider.Highlight1.Opacity(0.5f) : colourProvider.Dark2;
                nub.Colour = IsHovered || IsDragged ? colourProvider.Highlight1 : colourProvider.Light4;
            }

            protected override void UpdateValue(float value)
            {
                nub.MoveToX(value, 200, Easing.OutPow10);
            }
        }
    }
}
