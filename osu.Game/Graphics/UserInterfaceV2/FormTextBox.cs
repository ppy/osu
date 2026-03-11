// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Extensions.IEnumerableExtensions;
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
using osuTK;

namespace osu.Game.Graphics.UserInterfaceV2
{
    public partial class FormTextBox : CompositeDrawable, IHasCurrentValue<string>, IFormControl
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

        private FormControlBackground background = null!;
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
            AutoSizeAxes = Axes.Y;

            InternalChildren = new Drawable[]
            {
                background = new FormControlBackground(),
                flashLayer = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Colour4.Transparent,
                },
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Padding = new MarginPadding(9),
                    Spacing = new Vector2(0, 4),
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

            current.BindValueChanged(_ => ValueChanged?.Invoke());
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

            caption.Colour = disabled ? colourProvider.Background1 : colourProvider.Content2;
            textBox.Colour = disabled ? colourProvider.Foreground1 : colourProvider.Content1;

            if (Current.Disabled)
                background.VisualStyle = VisualStyle.Disabled;
            else if (textBox.Focused.Value)
                background.VisualStyle = VisualStyle.Focused;
            else if (IsHovered)
                background.VisualStyle = VisualStyle.Hovered;
            else
                background.VisualStyle = VisualStyle.Normal;
        }

        internal partial class InnerTextBox : OsuTextBox
        {
            public BindableBool Focused { get; } = new BindableBool();

            public Action? OnInputError { get; set; }

            protected override float LeftRightPadding => 0;

            public InnerTextBox()
            {
                DrawBorder = false;
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                Height = 16;
                TextContainer.Height = 1;
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

        public event Action? ValueChanged;

        public bool IsDefault => current.IsDefault;

        public void SetDefault() => current.SetDefault();

        public bool IsDisabled => current.Disabled;

        public IEnumerable<LocalisableString> FilterTerms => Caption.Yield();

        public float MainDrawHeight => DrawHeight;
    }
}
