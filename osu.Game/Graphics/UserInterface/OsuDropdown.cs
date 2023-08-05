// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Input.Bindings;
using osu.Game.Overlays;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Graphics.UserInterface
{
    public partial class OsuDropdown<T> : Dropdown<T>
    {
        private const float corner_radius = 5;

        protected override DropdownHeader CreateHeader() => new OsuDropdownHeader();

        protected override DropdownMenu CreateMenu() => new OsuDropdownMenu();

        #region OsuDropdownMenu

        protected partial class OsuDropdownMenu : DropdownMenu, IKeyBindingHandler<GlobalAction>
        {
            public override bool HandleNonPositionalInput => State == MenuState.Open;

            private Sample? sampleOpen;
            private Sample? sampleClose;

            // todo: this uses the same styling as OsuMenu. hopefully we can just use OsuMenu in the future with some refactoring
            public OsuDropdownMenu()
            {
                CornerRadius = corner_radius;

                MaskingContainer.CornerRadius = corner_radius;
                Alpha = 0;

                // todo: this uses the same styling as OsuMenu. hopefully we can just use OsuMenu in the future with some refactoring
                ItemsContainer.Padding = new MarginPadding(5);
            }

            [BackgroundDependencyLoader(true)]
            private void load(OverlayColourProvider? colourProvider, OsuColour colours, AudioManager audio)
            {
                BackgroundColour = colourProvider?.Background5 ?? Color4.Black.Opacity(0.5f);
                HoverColour = colourProvider?.Light4 ?? colours.PinkDarker;
                SelectionColour = colourProvider?.Background3 ?? colours.PinkDarker.Opacity(0.5f);

                sampleOpen = audio.Samples.Get(@"UI/dropdown-open");
                sampleClose = audio.Samples.Get(@"UI/dropdown-close");
            }

            // todo: this shouldn't be required after https://github.com/ppy/osu-framework/issues/4519 is fixed.
            private bool wasOpened;

            // todo: this uses the same styling as OsuMenu. hopefully we can just use OsuMenu in the future with some refactoring
            protected override void AnimateOpen()
            {
                wasOpened = true;
                this.FadeIn(300, Easing.OutQuint);
                sampleOpen?.Play();
            }

            protected override void AnimateClose()
            {
                if (wasOpened)
                {
                    this.FadeOut(300, Easing.OutQuint);
                    sampleClose?.Play();
                }
            }

            // todo: this uses the same styling as OsuMenu. hopefully we can just use OsuMenu in the future with some refactoring
            protected override void UpdateSize(Vector2 newSize)
            {
                if (Direction == Direction.Vertical)
                {
                    Width = newSize.X;
                    this.ResizeHeightTo(newSize.Y, 300, Easing.OutQuint);
                }
                else
                {
                    Height = newSize.Y;
                    this.ResizeWidthTo(newSize.X, 300, Easing.OutQuint);
                }
            }

            private Color4 hoverColour;

            public Color4 HoverColour
            {
                get => hoverColour;
                set
                {
                    hoverColour = value;
                    foreach (var c in Children.OfType<DrawableOsuDropdownMenuItem>())
                        c.BackgroundColourHover = value;
                }
            }

            private Color4 selectionColour;

            public Color4 SelectionColour
            {
                get => selectionColour;
                set
                {
                    selectionColour = value;
                    foreach (var c in Children.OfType<DrawableOsuDropdownMenuItem>())
                        c.BackgroundColourSelected = value;
                }
            }

            protected override Menu CreateSubMenu() => new OsuMenu(Direction.Vertical);

            protected override DrawableDropdownMenuItem CreateDrawableDropdownMenuItem(MenuItem item) => new DrawableOsuDropdownMenuItem(item)
            {
                BackgroundColourHover = HoverColour,
                BackgroundColourSelected = SelectionColour
            };

            protected override ScrollContainer<Drawable> CreateScrollContainer(Direction direction) => new OsuScrollContainer(direction);

            #region DrawableOsuDropdownMenuItem

            public partial class DrawableOsuDropdownMenuItem : DrawableDropdownMenuItem
            {
                // IsHovered is used
                public override bool HandlePositionalInput => true;

                public new Color4 BackgroundColourHover
                {
                    get => base.BackgroundColourHover;
                    set
                    {
                        base.BackgroundColourHover = value;
                        updateColours();
                    }
                }

                public new Color4 BackgroundColourSelected
                {
                    get => base.BackgroundColourSelected;
                    set
                    {
                        base.BackgroundColourSelected = value;
                        updateColours();
                    }
                }

                private void updateColours()
                {
                    BackgroundColour = BackgroundColourHover.Opacity(0);

                    UpdateBackgroundColour();
                    UpdateForegroundColour();
                }

                public DrawableOsuDropdownMenuItem(MenuItem item)
                    : base(item)
                {
                    Foreground.Padding = new MarginPadding(2);

                    Masking = true;
                    CornerRadius = corner_radius;
                }

                [BackgroundDependencyLoader]
                private void load()
                {
                    AddInternal(new HoverSounds());
                }

                protected override void UpdateBackgroundColour()
                {
                    Background.FadeColour(IsPreSelected ? BackgroundColourHover : BackgroundColourSelected, 100, Easing.OutQuint);

                    if (IsPreSelected || IsSelected)
                        Background.FadeIn(100, Easing.OutQuint);
                    else
                        Background.FadeOut(600, Easing.OutQuint);
                }

                protected override void UpdateForegroundColour()
                {
                    base.UpdateForegroundColour();

                    if (Foreground.Children.FirstOrDefault() is Content content)
                        content.Hovering = IsHovered;
                }

                protected override Drawable CreateContent() => new Content();

                protected new partial class Content : CompositeDrawable, IHasText
                {
                    public LocalisableString Text
                    {
                        get => Label.Text;
                        set => Label.Text = value;
                    }

                    public readonly OsuSpriteText Label;
                    public readonly SpriteIcon Chevron;

                    private const float chevron_offset = -3;

                    public Content()
                    {
                        RelativeSizeAxes = Axes.X;
                        AutoSizeAxes = Axes.Y;

                        InternalChildren = new Drawable[]
                        {
                            Chevron = new SpriteIcon
                            {
                                Icon = FontAwesome.Solid.ChevronRight,
                                Size = new Vector2(8),
                                Alpha = 0,
                                X = chevron_offset,
                                Margin = new MarginPadding { Left = 3, Right = 3 },
                                Origin = Anchor.CentreLeft,
                                Anchor = Anchor.CentreLeft,
                            },
                            Label = new OsuSpriteText
                            {
                                X = 15,
                                Origin = Anchor.CentreLeft,
                                Anchor = Anchor.CentreLeft,
                            },
                        };
                    }

                    [BackgroundDependencyLoader(true)]
                    private void load(OverlayColourProvider? colourProvider)
                    {
                        Chevron.Colour = colourProvider?.Background5 ?? Color4.Black;
                    }

                    private bool hovering;

                    public bool Hovering
                    {
                        get => hovering;
                        set
                        {
                            if (value == hovering)
                                return;

                            hovering = value;

                            if (hovering)
                            {
                                Chevron.FadeIn(400, Easing.OutQuint);
                                Chevron.MoveToX(0, 400, Easing.OutQuint);
                            }
                            else
                            {
                                Chevron.FadeOut(200);
                                Chevron.MoveToX(chevron_offset, 200, Easing.In);
                            }
                        }
                    }
                }
            }

            #endregion

            public bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
            {
                if (e.Repeat) return false;

                if (e.Action == GlobalAction.Back)
                {
                    State = MenuState.Closed;
                    return true;
                }

                return false;
            }

            public void OnReleased(KeyBindingReleaseEvent<GlobalAction> e)
            {
            }
        }

        #endregion

        public partial class OsuDropdownHeader : DropdownHeader
        {
            protected readonly SpriteText Text;

            protected override LocalisableString Label
            {
                get => Text.Text;
                set => Text.Text = value;
            }

            protected readonly SpriteIcon Icon;

            public OsuDropdownHeader()
            {
                Foreground.Padding = new MarginPadding(10);

                AutoSizeAxes = Axes.None;
                Margin = new MarginPadding { Bottom = 4 };
                CornerRadius = corner_radius;
                Height = 40;

                Foreground.Child = new GridContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    RowDimensions = new[]
                    {
                        new Dimension(GridSizeMode.AutoSize),
                    },
                    ColumnDimensions = new[]
                    {
                        new Dimension(),
                        new Dimension(GridSizeMode.AutoSize),
                    },
                    Content = new[]
                    {
                        new Drawable[]
                        {
                            Text = new TruncatingSpriteText
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                RelativeSizeAxes = Axes.X,
                            },
                            Icon = new SpriteIcon
                            {
                                Icon = FontAwesome.Solid.ChevronDown,
                                Anchor = Anchor.CentreRight,
                                Origin = Anchor.CentreRight,
                                Size = new Vector2(16),
                            },
                        }
                    }
                };

                AddInternal(new HoverClickSounds());
            }

            [BackgroundDependencyLoader(true)]
            private void load(OverlayColourProvider? colourProvider, OsuColour colours)
            {
                BackgroundColour = colourProvider?.Background5 ?? Color4.Black.Opacity(0.5f);
                BackgroundColourHover = colourProvider?.Light4 ?? colours.PinkDarker;
            }
        }
    }
}
