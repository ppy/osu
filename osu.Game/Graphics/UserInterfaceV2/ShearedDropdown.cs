// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Input.Bindings;
using osu.Game.Overlays;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Graphics.UserInterfaceV2
{
    public partial class ShearedDropdown<T> : Dropdown<T>, IKeyBindingHandler<GlobalAction>
    {
        protected override DropdownHeader CreateHeader() => new ShearedDropdownHeader();

        protected override DropdownMenu CreateMenu() => new ShearedDropdownMenu();

        public ShearedDropdown(LocalisableString label)
        {
            if (Header is ShearedDropdownHeader osuHeader)
            {
                osuHeader.Dropdown = this;
                osuHeader.LeftSideLabel = label;
            }
        }

        protected override void Update()
        {
            base.Update();

            var header = (ShearedDropdownHeader)Header;
            var menu = (ShearedDropdownMenu)Menu;

            menu.Padding = new MarginPadding { Left = header.LabelContainer.DrawWidth - 10f, Right = 6f };
        }

        public bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            if (e.Repeat) return false;

            if (e.Action == GlobalAction.Back)
                return Back();

            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<GlobalAction> e)
        {
        }

        protected partial class ShearedDropdownMenu : OsuDropdown<T>.OsuDropdownMenu
        {
            private readonly Vector2 shear = new Vector2(OsuGame.SHEAR, 0);

            public new MarginPadding Padding
            {
                get => base.Padding;
                set => base.Padding = value;
            }

            public ShearedDropdownMenu()
            {
                Shear = shear;
                Margin = new MarginPadding { Top = 5f };
            }

            protected override DrawableDropdownMenuItem CreateDrawableDropdownMenuItem(MenuItem item) => new ShearedMenuItem(item)
            {
                BackgroundColourHover = HoverColour,
                BackgroundColourSelected = SelectionColour
            };

            public partial class ShearedMenuItem : DrawableOsuDropdownMenuItem
            {
                private readonly Vector2 shear = new Vector2(OsuGame.SHEAR, 0);

                public ShearedMenuItem(MenuItem item)
                    : base(item)
                {
                    Foreground.Shear = -shear;
                }
            }
        }

        public partial class ShearedDropdownHeader : DropdownHeader
        {
            private const float corner_radius = 5f;

            private LocalisableString label;

            protected override LocalisableString Label
            {
                get => label;
                set
                {
                    label = value;
                    valueText.Text = value;
                }
            }

            public LocalisableString LeftSideLabel
            {
                set => labelText.Text = value;
            }

            private readonly OsuSpriteText labelText;
            private readonly OsuSpriteText valueText;
            private readonly Box labelBox;
            private readonly SpriteIcon chevron;

            public Container LabelContainer { get; }

            public ShearedDropdown<T> Dropdown = null!;
            private ShearedDropdownSearchBar searchBar = null!;

            private readonly Vector2 shear = new Vector2(OsuGame.SHEAR, 0);

            [Resolved]
            private OverlayColourProvider colourProvider { get; set; } = null!;

            public ShearedDropdownHeader()
            {
                Shear = shear;
                CornerRadius = corner_radius;
                Masking = true;

                Foreground.Children = new Drawable[]
                {
                    new GridContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        RowDimensions = new[] { new Dimension(GridSizeMode.AutoSize) },
                        ColumnDimensions = new[]
                        {
                            new Dimension(GridSizeMode.AutoSize),
                            new Dimension()
                        },
                        Content = new[]
                        {
                            new[]
                            {
                                LabelContainer = new Container
                                {
                                    CornerRadius = corner_radius,
                                    Masking = true,
                                    AutoSizeAxes = Axes.Both,
                                    Children = new Drawable[]
                                    {
                                        labelBox = new Box
                                        {
                                            RelativeSizeAxes = Axes.Both
                                        },
                                        labelText = new OsuSpriteText
                                        {
                                            Margin = new MarginPadding { Horizontal = 10f, Vertical = 8f },
                                            Font = OsuFont.Torus.With(size: 16.8f, weight: FontWeight.SemiBold),
                                            Shear = -shear,
                                        },
                                    },
                                },
                                new Container
                                {
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Padding = new MarginPadding { Horizontal = 10f },
                                    Shear = -shear,
                                    Children = new Drawable[]
                                    {
                                        valueText = new TruncatingSpriteText
                                        {
                                            Anchor = Anchor.CentreLeft,
                                            Origin = Anchor.CentreLeft,
                                            Padding = new MarginPadding { Right = 15f },
                                            Font = OsuFont.Torus.With(size: 16.8f, weight: FontWeight.SemiBold),
                                            RelativeSizeAxes = Axes.X,
                                        },
                                        chevron = new SpriteIcon
                                        {
                                            Anchor = Anchor.CentreRight,
                                            Origin = Anchor.CentreRight,
                                            Y = 1f,
                                            Icon = FontAwesome.Solid.ChevronDown,
                                            Size = new Vector2(10f),
                                        }
                                    },
                                },
                            }
                        }
                    },
                };

                AddInternal(LabelContainer.CreateProxy());
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                labelBox.Colour = colourProvider.Background3;
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                Dropdown.Menu.StateChanged += _ => updateChevron();
                SearchBar.State.ValueChanged += _ => updateColour();
                Enabled.BindValueChanged(_ => updateColour());
                updateColour();
            }

            protected override void Update()
            {
                base.Update();
                searchBar.Padding = new MarginPadding { Left = LabelContainer.DrawWidth };

                // By limiting the width we avoid this box showing up as an outline around the drawables that are on top of it.
                Background.Padding = new MarginPadding { Left = LabelContainer.DrawWidth - corner_radius };
            }

            protected override bool OnHover(HoverEvent e)
            {
                updateColour();
                return false;
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                updateColour();
            }

            private void updateColour()
            {
                bool hovered = Enabled.Value && IsHovered;
                var hoveredColour = colourProvider.Light4;
                var unhoveredColour = colourProvider.Background5;

                Colour = Color4.White;
                Alpha = Enabled.Value ? 1 : 0.3f;

                if (SearchBar.State.Value == Visibility.Visible)
                {
                    chevron.Colour = hovered ? hoveredColour.Lighten(0.5f) : Colour4.White;
                    Background.Colour = unhoveredColour;
                }
                else
                {
                    chevron.Colour = Color4.White;
                    Background.Colour = hovered ? hoveredColour : unhoveredColour;
                }
            }

            private void updateChevron()
            {
                Debug.Assert(Dropdown != null);
                bool open = Dropdown.Menu.State == MenuState.Open;
                chevron.ScaleTo(open ? new Vector2(1f, -1f) : Vector2.One, 300, Easing.OutQuint);
            }

            protected override DropdownSearchBar CreateSearchBar() => searchBar = new ShearedDropdownSearchBar();

            private partial class ShearedDropdownSearchBar : DropdownSearchBar
            {
                protected override void PopIn() => this.FadeIn();

                protected override void PopOut() => this.FadeOut();

                protected override TextBox CreateTextBox() => new DropdownSearchTextBox
                {
                    FontSize = OsuFont.Default.Size,
                };

                private partial class DropdownSearchTextBox : OsuTextBox
                {
                    private readonly Vector2 shear = new Vector2(OsuGame.SHEAR, 0);

                    [BackgroundDependencyLoader]
                    private void load(OverlayColourProvider? colourProvider)
                    {
                        TextContainer.Shear = -shear;
                        BackgroundUnfocused = colourProvider?.Background5 ?? new Color4(10, 10, 10, 255);
                        BackgroundFocused = colourProvider?.Background5 ?? new Color4(10, 10, 10, 255);
                    }

                    protected override void OnFocus(FocusEvent e)
                    {
                        base.OnFocus(e);
                        BorderThickness = 0;
                    }
                }
            }
        }
    }
}
