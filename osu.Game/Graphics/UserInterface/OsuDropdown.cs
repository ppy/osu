// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osuTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Localisation;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osuTK;

namespace osu.Game.Graphics.UserInterface
{
    public class OsuDropdown<T> : Dropdown<T>, IHasAccentColour
    {
        private const float corner_radius = 4;

        private Color4 accentColour;

        public Color4 AccentColour
        {
            get => accentColour;
            set
            {
                accentColour = value;
                updateAccentColour();
            }
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            if (accentColour == default)
                accentColour = colours.PinkDarker;
            updateAccentColour();
        }

        private void updateAccentColour()
        {
            if (Header is IHasAccentColour header) header.AccentColour = accentColour;

            if (Menu is IHasAccentColour menu) menu.AccentColour = accentColour;
        }

        protected override DropdownHeader CreateHeader() => new OsuDropdownHeader();

        protected override DropdownMenu CreateMenu() => new OsuDropdownMenu();

        #region OsuDropdownMenu

        protected class OsuDropdownMenu : DropdownMenu, IHasAccentColour
        {
            public override bool HandleNonPositionalInput => State == MenuState.Open;

            private Sample sampleOpen;
            private Sample sampleClose;

            // todo: this uses the same styling as OsuMenu. hopefully we can just use OsuMenu in the future with some refactoring
            public OsuDropdownMenu()
            {
                CornerRadius = corner_radius;
                BackgroundColour = Color4.Black.Opacity(0.5f);

                MaskingContainer.CornerRadius = corner_radius;
                Alpha = 0;

                // todo: this uses the same styling as OsuMenu. hopefully we can just use OsuMenu in the future with some refactoring
                ItemsContainer.Padding = new MarginPadding(5);
            }

            [BackgroundDependencyLoader]
            private void load(AudioManager audio)
            {
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

            private Color4 accentColour;

            public Color4 AccentColour
            {
                get => accentColour;
                set
                {
                    accentColour = value;
                    foreach (var c in Children.OfType<IHasAccentColour>())
                        c.AccentColour = value;
                }
            }

            protected override Menu CreateSubMenu() => new OsuMenu(Direction.Vertical);

            protected override DrawableDropdownMenuItem CreateDrawableDropdownMenuItem(MenuItem item) => new DrawableOsuDropdownMenuItem(item) { AccentColour = accentColour };

            protected override ScrollContainer<Drawable> CreateScrollContainer(Direction direction) => new OsuScrollContainer(direction);

            #region DrawableOsuDropdownMenuItem

            public class DrawableOsuDropdownMenuItem : DrawableDropdownMenuItem, IHasAccentColour
            {
                // IsHovered is used
                public override bool HandlePositionalInput => true;

                private Color4? accentColour;

                public Color4 AccentColour
                {
                    get => accentColour ?? nonAccentSelectedColour;
                    set
                    {
                        accentColour = value;
                        updateColours();
                    }
                }

                private void updateColours()
                {
                    BackgroundColourHover = accentColour ?? nonAccentHoverColour;
                    BackgroundColourSelected = accentColour ?? nonAccentSelectedColour;
                    UpdateBackgroundColour();
                    UpdateForegroundColour();
                }

                private Color4 nonAccentHoverColour;
                private Color4 nonAccentSelectedColour;

                public DrawableOsuDropdownMenuItem(MenuItem item)
                    : base(item)
                {
                    Foreground.Padding = new MarginPadding(2);

                    Masking = true;
                    CornerRadius = corner_radius;
                }

                [BackgroundDependencyLoader]
                private void load(OsuColour colours)
                {
                    BackgroundColour = Color4.Transparent;

                    nonAccentHoverColour = colours.PinkDarker;
                    nonAccentSelectedColour = Color4.Black.Opacity(0.5f);
                    updateColours();

                    AddInternal(new HoverSounds());
                }

                protected override void UpdateForegroundColour()
                {
                    base.UpdateForegroundColour();

                    if (Foreground.Children.FirstOrDefault() is Content content) content.Chevron.Alpha = IsHovered ? 1 : 0;
                }

                protected override Drawable CreateContent() => new Content();

                protected new class Content : FillFlowContainer, IHasText
                {
                    public LocalisableString Text
                    {
                        get => Label.Text;
                        set => Label.Text = value;
                    }

                    public readonly OsuSpriteText Label;
                    public readonly SpriteIcon Chevron;

                    public Content()
                    {
                        RelativeSizeAxes = Axes.X;
                        AutoSizeAxes = Axes.Y;
                        Direction = FillDirection.Horizontal;

                        Children = new Drawable[]
                        {
                            Chevron = new SpriteIcon
                            {
                                AlwaysPresent = true,
                                Icon = FontAwesome.Solid.ChevronRight,
                                Colour = Color4.Black,
                                Alpha = 0.5f,
                                Size = new Vector2(8),
                                Margin = new MarginPadding { Left = 3, Right = 3 },
                                Origin = Anchor.CentreLeft,
                                Anchor = Anchor.CentreLeft,
                            },
                            Label = new OsuSpriteText
                            {
                                Origin = Anchor.CentreLeft,
                                Anchor = Anchor.CentreLeft,
                            },
                        };
                    }
                }
            }

            #endregion
        }

        #endregion

        public class OsuDropdownHeader : DropdownHeader, IHasAccentColour
        {
            protected readonly SpriteText Text;

            protected override LocalisableString Label
            {
                get => Text.Text;
                set => Text.Text = value;
            }

            protected readonly SpriteIcon Icon;

            private Color4 accentColour;

            public virtual Color4 AccentColour
            {
                get => accentColour;
                set
                {
                    accentColour = value;
                    BackgroundColourHover = accentColour;
                }
            }

            public OsuDropdownHeader()
            {
                Foreground.Padding = new MarginPadding(4);

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
                            Text = new OsuSpriteText
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                RelativeSizeAxes = Axes.X,
                                Truncate = true,
                            },
                            Icon = new SpriteIcon
                            {
                                Icon = FontAwesome.Solid.ChevronDown,
                                Anchor = Anchor.CentreRight,
                                Origin = Anchor.CentreRight,
                                Margin = new MarginPadding { Horizontal = 5 },
                                Size = new Vector2(12),
                            },
                        }
                    }
                };

                AddInternal(new HoverClickSounds());
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                BackgroundColour = Color4.Black.Opacity(0.5f);
                BackgroundColourHover = colours.PinkDarker;
            }
        }
    }
}
