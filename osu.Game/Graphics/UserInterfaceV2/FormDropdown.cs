// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osu.Game.Resources.Localisation.Web;
using osuTK;

namespace osu.Game.Graphics.UserInterfaceV2
{
    public partial class FormDropdown<T> : OsuDropdown<T>, IFormControl
    {
        /// <summary>
        /// Caption describing this slider bar, displayed on top of the controls.
        /// </summary>
        public LocalisableString Caption { get; init; }

        /// <summary>
        /// Hint text containing an extended description of this slider bar, displayed in a tooltip when hovering the caption.
        /// </summary>
        public LocalisableString HintText
        {
            get => header.HintText;
            set => header.HintText = value;
        }

        /// <summary>
        /// The maximum height of the dropdown's menu.
        /// By default, this is set to 200px high. Set to <see cref="float.PositiveInfinity"/> to remove such limit.
        /// </summary>
        public float MaxHeight { get; set; } = 200;

        private FormDropdownHeader header = null!;

        private const float header_menu_spacing = 5;

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.X;

            header.Caption = Caption;
            header.HintText = HintText;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Current.BindValueChanged(_ => ValueChanged?.Invoke());
        }

        public virtual IEnumerable<LocalisableString> FilterTerms
        {
            get
            {
                yield return Caption;

                foreach (var item in MenuItems)
                    yield return item.Text.Value;
            }
        }

        public event Action? ValueChanged;

        public bool IsDefault => Current.IsDefault;

        public void SetDefault() => Current.SetDefault();

        public bool IsDisabled => Current.Disabled;

        public float MainDrawHeight => header.DrawHeight;

        protected override DropdownHeader CreateHeader() => header = new FormDropdownHeader
        {
            Dropdown = this,
        };

        protected override DropdownMenu CreateMenu() => new FormDropdownMenu
        {
            MaxHeight = MaxHeight,
        };

        private partial class FormDropdownHeader : DropdownHeader
        {
            public FormDropdown<T> Dropdown { get; set; } = null!;

            protected override DropdownSearchBar CreateSearchBar() => SearchBar = new FormDropdownSearchBar();

            private LocalisableString captionText;
            private LocalisableString hintText;
            private LocalisableString labelText;

            public LocalisableString Caption
            {
                get => captionText;
                set
                {
                    captionText = value;

                    if (caption.IsNotNull())
                        caption.Caption = value;
                }
            }

            public LocalisableString HintText
            {
                get => hintText;
                set
                {
                    hintText = value;

                    if (caption.IsNotNull())
                        caption.TooltipText = value;
                }
            }

            protected override LocalisableString Label
            {
                get => labelText;
                set
                {
                    labelText = value;

                    if (label.IsNotNull())
                        label.Text = labelText;
                }
            }

            protected new FormDropdownSearchBar SearchBar { get; set; } = null!;

            private FormFieldCaption caption = null!;
            private OsuSpriteText label = null!;
            private SpriteIcon chevron = null!;
            private FormControlBackground background = null!;

            [Resolved]
            private OverlayColourProvider colourProvider { get; set; } = null!;

            [BackgroundDependencyLoader]
            private void load()
            {
                Masking = true;
                CornerRadius = 5;

                // We use our own background for more control.
                Background.Alpha = 0;

                Foreground.Children = new Drawable[]
                {
                    background = new FormControlBackground(),
                    new Container
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Padding = new MarginPadding(9),
                        Children = new Drawable[]
                        {
                            new FillFlowContainer
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Direction = FillDirection.Vertical,
                                Spacing = new Vector2(0, 4),
                                Children = new Drawable[]
                                {
                                    caption = new FormFieldCaption
                                    {
                                        Caption = Caption,
                                        TooltipText = HintText,
                                    },
                                    label = new TruncatingSpriteText
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        Padding = new MarginPadding { Right = 25 },
                                        AlwaysPresent = true,
                                    },
                                }
                            },
                            chevron = new SpriteIcon
                            {
                                Icon = FontAwesome.Solid.ChevronDown,
                                Anchor = Anchor.BottomRight,
                                Origin = Anchor.BottomRight,
                                Size = new Vector2(16),
                                Margin = new MarginPadding { Right = 5 },
                            },
                        }
                    },
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                Dropdown.Current.BindDisabledChanged(_ => updateState());
                SearchBar.SearchTerm.BindValueChanged(_ => updateState(), true);
                Dropdown.Menu.StateChanged += _ =>
                {
                    updateState();
                    updateChevron();
                };
                SearchBar.TextBox.OnCommit += (_, _) =>
                {
                    Background.FlashColour(ColourInfo.GradientVertical(colourProvider.Background5, colourProvider.Dark2), 800, Easing.OutQuint);
                };
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

            private void updateState()
            {
                caption.Colour = Dropdown.Current.Disabled ? colourProvider.Background1 : colourProvider.Content2;
                label.Colour = Dropdown.Current.Disabled ? colourProvider.Background1 : colourProvider.Content1;
                chevron.Colour = Dropdown.Current.Disabled ? colourProvider.Background1 : colourProvider.Content1;
                DisabledColour = Colour4.White;

                bool dropdownOpen = Dropdown.Menu.State == MenuState.Open;

                if (dropdownOpen)
                    label.Alpha = AlwaysShowSearchBar || !string.IsNullOrEmpty(SearchBar.SearchTerm.Value) ? 0 : 1;
                else
                    label.Alpha = 1;

                if (Dropdown.Current.Disabled)
                    background.VisualStyle = VisualStyle.Disabled;
                else if (dropdownOpen)
                    background.VisualStyle = VisualStyle.Focused;
                else if (IsHovered)
                    background.VisualStyle = VisualStyle.Hovered;
                else
                    background.VisualStyle = VisualStyle.Normal;
            }

            private void updateChevron()
            {
                bool open = Dropdown.Menu.State == MenuState.Open;
                chevron.ScaleTo(open ? new Vector2(1f, -1f) : Vector2.One, 300, Easing.OutQuint);
                chevron.MoveToY(open ? -chevron.DrawHeight : 0, 300, Easing.OutQuint);
            }
        }

        private partial class FormDropdownSearchBar : DropdownSearchBar
        {
            public FormTextBox.InnerTextBox TextBox { get; private set; } = null!;

            protected override void PopIn() => this.FadeIn();
            protected override void PopOut() => this.FadeOut();

            protected override TextBox CreateTextBox() => TextBox = new FormTextBox.InnerTextBox
            {
                PlaceholderText = HomeStrings.SearchPlaceholder,
            };

            [BackgroundDependencyLoader]
            private void load()
            {
                TextBox.Anchor = Anchor.BottomLeft;
                TextBox.Origin = Anchor.BottomLeft;
                TextBox.RelativeSizeAxes = Axes.X;
                Padding = new MarginPadding { Left = 9, Bottom = 9, Right = 34 };
            }
        }

        private partial class FormDropdownMenu : OsuDropdownMenu
        {
            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider)
            {
                ItemsContainer.Padding = new MarginPadding(9);

                MaskingContainer.BorderThickness = FormControlBackground.BORDER_THICKNESS;
                MaskingContainer.CornerExponent = FormControlBackground.CORNER_EXPONENT;
                MaskingContainer.BorderColour = colourProvider.Highlight1;
            }

            protected override void AnimateOpen()
            {
                base.AnimateOpen();

                this.TransformTo(nameof(Margin), new MarginPadding
                {
                    Top = header_menu_spacing,
                }, 300, Easing.OutQuint);
            }

            protected override void AnimateClose()
            {
                base.AnimateClose();
                this.TransformTo(nameof(Margin), new MarginPadding(), 300, Easing.OutQuint);
            }
        }
    }

    public partial class FormEnumDropdown<T> : FormDropdown<T>
        where T : struct, Enum
    {
        public FormEnumDropdown()
        {
            Items = Enum.GetValues<T>();
        }
    }
}
