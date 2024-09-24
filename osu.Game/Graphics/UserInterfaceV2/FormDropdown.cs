// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Graphics.UserInterfaceV2
{
    public partial class FormDropdown<T> : OsuDropdown<T>
    {
        /// <summary>
        /// Caption describing this slider bar, displayed on top of the controls.
        /// </summary>
        public LocalisableString Caption { get; init; }

        /// <summary>
        /// Hint text containing an extended description of this slider bar, displayed in a tooltip when hovering the caption.
        /// </summary>
        public LocalisableString HintText { get; init; }

        private FormDropdownHeader header = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.X;

            header.Caption = Caption;
            header.HintText = HintText;
        }

        protected override DropdownHeader CreateHeader() => header = new FormDropdownHeader
        {
            Dropdown = this,
        };

        protected override DropdownMenu CreateMenu() => new FormDropdownMenu();

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

            [Resolved]
            private OverlayColourProvider colourProvider { get; set; } = null!;

            [BackgroundDependencyLoader]
            private void load()
            {
                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.None;
                Height = 50;

                Masking = true;
                CornerRadius = 5;

                Foreground.AutoSizeAxes = Axes.None;
                Foreground.RelativeSizeAxes = Axes.Both;
                Foreground.Padding = new MarginPadding(9);
                Foreground.Children = new Drawable[]
                {
                    caption = new FormFieldCaption
                    {
                        Anchor = Anchor.TopLeft,
                        Origin = Anchor.TopLeft,
                        Caption = Caption,
                        TooltipText = HintText,
                    },
                    label = new OsuSpriteText
                    {
                        RelativeSizeAxes = Axes.X,
                        Anchor = Anchor.BottomLeft,
                        Origin = Anchor.BottomLeft,
                    },
                    chevron = new SpriteIcon
                    {
                        Icon = FontAwesome.Solid.ChevronDown,
                        Anchor = Anchor.CentreRight,
                        Origin = Anchor.CentreRight,
                        Size = new Vector2(16),
                    },
                };

                AddInternal(new HoverClickSounds());
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
                label.Alpha = string.IsNullOrEmpty(SearchBar.SearchTerm.Value) ? 1 : 0;

                caption.Colour = Dropdown.Current.Disabled ? colourProvider.Foreground1 : colourProvider.Content2;
                label.Colour = Dropdown.Current.Disabled ? colourProvider.Foreground1 : colourProvider.Content1;
                chevron.Colour = Dropdown.Current.Disabled ? colourProvider.Foreground1 : colourProvider.Content1;
                DisabledColour = Colour4.White;

                bool dropdownOpen = Dropdown.Menu.State == MenuState.Open;

                if (!Dropdown.Current.Disabled)
                {
                    BorderThickness = IsHovered || dropdownOpen ? 2 : 0;
                    BorderColour = dropdownOpen ? colourProvider.Highlight1 : colourProvider.Light4;

                    if (dropdownOpen)
                        Background.Colour = ColourInfo.GradientVertical(colourProvider.Background5, colourProvider.Dark3);
                    else if (IsHovered)
                        Background.Colour = ColourInfo.GradientVertical(colourProvider.Background5, colourProvider.Dark4);
                    else
                        Background.Colour = colourProvider.Background5;
                }
                else
                {
                    Background.Colour = colourProvider.Background4;
                }
            }

            private void updateChevron()
            {
                bool open = Dropdown.Menu.State == MenuState.Open;
                chevron.ScaleTo(open ? new Vector2(1f, -1f) : Vector2.One, 300, Easing.OutQuint);
            }
        }

        private partial class FormDropdownSearchBar : DropdownSearchBar
        {
            public FormTextBox.InnerTextBox TextBox { get; private set; } = null!;

            protected override void PopIn() => this.FadeIn();
            protected override void PopOut() => this.FadeOut();

            protected override TextBox CreateTextBox() => TextBox = new FormTextBox.InnerTextBox();

            [BackgroundDependencyLoader]
            private void load()
            {
                TextBox.Anchor = Anchor.BottomLeft;
                TextBox.Origin = Anchor.BottomLeft;
                TextBox.RelativeSizeAxes = Axes.X;
                TextBox.Margin = new MarginPadding(9);
            }
        }

        private partial class FormDropdownMenu : OsuDropdownMenu
        {
            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider)
            {
                ItemsContainer.Padding = new MarginPadding(9);
                Margin = new MarginPadding { Top = 5 };

                MaskingContainer.BorderThickness = 2;
                MaskingContainer.BorderColour = colourProvider.Highlight1;
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
