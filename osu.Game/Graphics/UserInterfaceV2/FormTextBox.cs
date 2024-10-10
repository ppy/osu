// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;

namespace osu.Game.Graphics.UserInterfaceV2
{
    public partial class FormTextBox : CompositeDrawable, IHasCurrentValue<string>
    {
        public Bindable<string> Current
        {
            get => current.Current;
            set => current.Current = value;
        }

        private bool readOnly;

        public bool ReadOnly
        {
            get => readOnly;
            set
            {
                readOnly = value;

                if (textBox.IsNotNull())
                    updateState();
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

        public event TextBox.OnCommitHandler? OnCommit;

        private readonly BindableWithCurrent<string> current = new BindableWithCurrent<string>();

        /// <summary>
        /// Caption describing this slider bar, displayed on top of the controls.
        /// </summary>
        public LocalisableString Caption { get; init; }

        /// <summary>
        /// Hint text containing an extended description of this slider bar, displayed in a tooltip when hovering the caption.
        /// </summary>
        public LocalisableString HintText { get; init; }

        /// <summary>
        /// Text displayed in the text box when its contents are empty.
        /// </summary>
        public LocalisableString PlaceholderText { get; init; }

        private Box background = null!;
        private Box flashLayer = null!;
        private InnerTextBox textBox = null!;
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
            CornerExponent = 2.5f;

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
                        textBox = CreateTextBox().With(t =>
                        {
                            t.Anchor = Anchor.BottomRight;
                            t.Origin = Anchor.BottomRight;
                            t.RelativeSizeAxes = Axes.X;
                            t.Width = 1;
                            t.PlaceholderText = PlaceholderText;
                            t.Current = Current;
                            t.CommitOnFocusLost = true;
                            t.OnCommit += (textBox, newText) =>
                            {
                                OnCommit?.Invoke(textBox, newText);

                                if (!current.Disabled && !ReadOnly)
                                {
                                    flashLayer.Colour = ColourInfo.GradientVertical(colourProvider.Dark2.Opacity(0), colourProvider.Dark2);
                                    flashLayer.FadeOutFromOne(800, Easing.OutQuint);
                                }
                            };
                            t.OnInputError = () =>
                            {
                                flashLayer.Colour = ColourInfo.GradientVertical(colours.Red3.Opacity(0), colours.Red3);
                                flashLayer.FadeOutFromOne(200, Easing.OutQuint);
                            };
                            t.TabbableContentContainer = tabbableContentContainer;
                        }),
                    },
                },
            };
        }

        internal virtual InnerTextBox CreateTextBox() => new InnerTextBox();

        protected override void LoadComplete()
        {
            base.LoadComplete();

            focusManager = GetContainingFocusManager()!;
            textBox.Focused.BindValueChanged(_ => updateState());
            current.BindDisabledChanged(_ => updateState(), true);
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
            bool disabled = Current.Disabled || ReadOnly;

            textBox.ReadOnly = disabled;
            textBox.Alpha = 1;

            caption.Colour = disabled ? colourProvider.Foreground1 : colourProvider.Content2;
            textBox.Colour = disabled ? colourProvider.Foreground1 : colourProvider.Content1;

            if (!disabled)
            {
                BorderThickness = IsHovered || textBox.Focused.Value ? 2 : 0;
                BorderColour = textBox.Focused.Value ? colourProvider.Highlight1 : colourProvider.Light4;

                if (textBox.Focused.Value)
                    background.Colour = ColourInfo.GradientVertical(colourProvider.Background5, colourProvider.Dark3);
                else if (IsHovered)
                    background.Colour = ColourInfo.GradientVertical(colourProvider.Background5, colourProvider.Dark4);
                else
                    background.Colour = colourProvider.Background5;
            }
            else
            {
                BorderThickness = 0;
                background.Colour = colourProvider.Background4;
            }
        }

        internal partial class InnerTextBox : OsuTextBox
        {
            public BindableBool Focused { get; } = new BindableBool();

            public Action? OnInputError { get; set; }

            protected override float LeftRightPadding => 0;

            [BackgroundDependencyLoader]
            private void load()
            {
                Height = 16;
                TextContainer.Height = 1;
                Masking = false;
                BackgroundUnfocused = BackgroundFocused = BackgroundCommit = Colour4.Transparent;
            }

            protected override SpriteText CreatePlaceholder() => base.CreatePlaceholder().With(t => t.Margin = default);

            protected override void OnFocus(FocusEvent e)
            {
                base.OnFocus(e);

                Focused.Value = true;
            }

            protected override void OnFocusLost(FocusLostEvent e)
            {
                base.OnFocusLost(e);

                Focused.Value = false;
            }

            protected override void NotifyInputError()
            {
                PlayFeedbackSample(FeedbackSampleType.TextInvalid);
                // base call intentionally suppressed
                OnInputError?.Invoke();
            }
        }
    }
}
