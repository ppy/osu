// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Framework.Utils;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Graphics.UserInterfaceV2
{
    public partial class FormColourPicker : CompositeDrawable, IHasCurrentValue<Colour4>, IFormControl
    {
        public Bindable<Colour4> Current
        {
            get => current.Current;
            set
            {
                current.Current = value;

                // the above `Current` set could have disabled the instantaneous bindable too,
                // but we still need to copy out `Default` manually,
                // so lift that disable for a second and then restore it
                currentColourInstantaneous.Disabled = false;
                currentColourInstantaneous.Default = current.Default;
                currentColourInstantaneous.Disabled = current.Disabled;
            }
        }

        private readonly BindableWithCurrent<Colour4> current = new BindableWithCurrent<Colour4>(Colour4.White);

        private readonly Bindable<Colour4> currentColourInstantaneous = new Bindable<Colour4>();

        /// <summary>
        /// Whether changes to the value should instantaneously transfer to outside bindables.
        /// If <see langword="false"/>, the transfer will happen on text box commit (explicit, or implicit via focus loss), or on colour picker commit.
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

        /// <summary>
        /// Caption describing this color picker, displayed on top of the controls.
        /// </summary>
        public LocalisableString Caption { get; init; }

        /// <summary>
        /// Hint text containing an extended description of this color picker, displayed in a tooltip when hovering the caption.
        /// </summary>
        public LocalisableString HintText { get; init; }

        private FormControlBackground background = null!;
        private Box flashLayer = null!;
        private FormTextBox.InnerTextBox textBox = null!;
        private FormFieldCaption caption = null!;
        private IFocusManager focusManager = null!;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            InternalChildren = new Drawable[]
            {
                background = new FormControlBackground(),
                flashLayer = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Colour4.Transparent,
                },
                new GridContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Padding = new MarginPadding
                    {
                        Vertical = 5,
                        Left = 9,
                        Right = 5,
                    },
                    ColumnDimensions = new[]
                    {
                        new Dimension(),
                        new Dimension(GridSizeMode.Absolute, size: 90),
                    },
                    RowDimensions = new[]
                    {
                        new Dimension(GridSizeMode.AutoSize),
                    },
                    Content = new[]
                    {
                        new Drawable[]
                        {
                            new FillFlowContainer
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Direction = FillDirection.Vertical,
                                Spacing = new Vector2(0, 4),
                                Padding = new MarginPadding
                                {
                                    Right = 10,
                                    Vertical = 4,
                                },
                                Children = new Drawable[]
                                {
                                    caption = new FormFieldCaption
                                    {
                                        Anchor = Anchor.TopLeft,
                                        Origin = Anchor.TopLeft,
                                        Caption = Caption,
                                        TooltipText = HintText,
                                    },
                                    textBox = new FormTextBox.InnerTextBox
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        CommitOnFocusLost = true,
                                        SelectAllOnFocus = true,
                                        OnInputError = () =>
                                        {
                                            flashLayer.Colour = ColourInfo.GradientVertical(colours.Red3.Opacity(0), colours.Red3);
                                            flashLayer.FadeOutFromOne(200, Easing.OutQuint);
                                        },
                                        TabbableContentContainer = tabbableContentContainer,
                                    },
                                },
                            },
                            new InnerColourDisplay
                            {
                                Current = { BindTarget = currentColourInstantaneous },
                            },
                        },
                    }
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

            current.ValueChanged += e =>
            {
                currentColourInstantaneous.Value = e.NewValue;
                ValueChanged?.Invoke();
            };

            current.DisabledChanged += disabled =>
            {
                if (disabled)
                {
                    // revert any changes before disabling to make sure we are in a consistent state.
                    currentColourInstantaneous.Value = current.Value;
                }

                currentColourInstantaneous.Disabled = disabled;
                if (IsLoaded)
                    updateState();
            };

            current.CopyTo(currentColourInstantaneous);

            currentColourInstantaneous.BindDisabledChanged(_ => updateState());
            currentColourInstantaneous.BindValueChanged(e =>
            {
                if (!TransferValueOnCommit)
                    current.Value = e.NewValue;

                updateState();

                // Debounce updates from the color picker in order to prevent
                // the textbox from flickering when the color picker is dragged
                // very fast.
                if (!updatingFromTextBox)
                    Scheduler.Add(updateTextBox);
            }, true);
        }

        private bool updatingFromTextBox;

        private void textChanged(ValueChangedEvent<string> change)
        {
            tryUpdateColourFromTextBox();
        }

        private void textCommitted(TextBox t, bool isNew)
        {
            tryUpdateColourFromTextBox();
            // If the attempted update above failed, restore text box to match the slider.
            currentColourInstantaneous.TriggerChange();
            current.Value = currentColourInstantaneous.Value;

            background.Flash();
        }

        private void tryUpdateColourFromTextBox()
        {
            updatingFromTextBox = true;

            if (Colour4.TryParseHex(textBox.Current.Value, out var colour) && Precision.AlmostEquals(colour.A, 1))
                currentColourInstantaneous.Value = colour;

            updatingFromTextBox = false;
        }

        private void updateTextBox()
        {
            textBox.Text = currentColourInstantaneous.Value.ToHex();
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
            textBox.ReadOnly = currentColourInstantaneous.Disabled;
            textBox.Colour = currentColourInstantaneous.Disabled ? colourProvider.Foreground1 : colourProvider.Content1;

            caption.Colour = currentColourInstantaneous.Disabled ? colourProvider.Background1 : colourProvider.Content2;
            textBox.Colour = currentColourInstantaneous.Disabled ? colourProvider.Background1 : colourProvider.Content2;

            if (Current.Disabled)
                background.VisualStyle = VisualStyle.Disabled;
            else if (textBox.Focused.Value)
                background.VisualStyle = VisualStyle.Focused;
            else if (IsHovered)
                background.VisualStyle = VisualStyle.Hovered;
            else
                background.VisualStyle = VisualStyle.Normal;
        }

        public partial class InnerColourDisplay : CompositeDrawable, IHasCurrentValue<Colour4>, IHasPopover
        {
            private readonly BindableWithCurrent<Colour4> current = new BindableWithCurrent<Colour4>();

            public Bindable<Colour4> Current
            {
                get => current;
                set => current.Current = value;
            }

            private Box colourDisplay = null!;

            [BackgroundDependencyLoader]
            private void load()
            {
                RelativeSizeAxes = Axes.Both;
                Masking = true;
                CornerRadius = 5;
                InternalChild = colourDisplay = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                Current.BindValueChanged(e => colourDisplay.Colour = e.NewValue, true);
            }

            protected override bool OnClick(ClickEvent e)
            {
                this.ShowPopover();
                return true;
            }

            public Popover GetPopover() => new OsuPopover(false)
            {
                // Ensure the popover doesn't cover up the text input.
                AllowableAnchors = [Anchor.TopCentre, Anchor.BottomCentre],
                Child = new OsuHSVColourPicker
                {
                    Current = { BindTarget = Current },
                },
            };
        }

        public IEnumerable<LocalisableString> FilterTerms => new[] { Caption, HintText };

        public event Action? ValueChanged;

        public bool IsDefault => Current.IsDefault;

        public void SetDefault() => Current.SetDefault();

        public bool IsDisabled => Current.Disabled;

        public float MainDrawHeight => DrawHeight;
    }
}
