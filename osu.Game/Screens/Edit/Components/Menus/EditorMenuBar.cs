// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Screens.Edit.Components.Menus
{
    public partial class EditorMenuBar : OsuMenu
    {
        private const float heading_area = 114;

        public EditorMenuBar()
            : base(Direction.Horizontal, true)
        {
            RelativeSizeAxes = Axes.X;

            MaskingContainer.CornerRadius = 0;
            ItemsContainer.Padding = new MarginPadding();

            ContentContainer.Margin = new MarginPadding { Left = heading_area };
            ContentContainer.Masking = true;
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider, TextureStore textures)
        {
            BackgroundColour = colourProvider.Background3;

            TextFlowContainer text;

            AddRangeInternal(new[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.Y,
                    Width = heading_area,
                    Padding = new MarginPadding(8),
                    Children = new Drawable[]
                    {
                        new SpriteIcon
                        {
                            Size = new Vector2(26),
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Icon = OsuIcon.EditCircle,
                        },
                        text = new TextFlowContainer
                        {
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreRight,
                            AutoSizeAxes = Axes.Both,
                        }
                    }
                },
            });

            text.AddText(@"osu!", t => t.Font = OsuFont.TorusAlternate);
            text.AddText(@"editor", t =>
            {
                t.Font = OsuFont.TorusAlternate;
                t.Colour = colourProvider.Highlight1;
            });
        }

        protected override Framework.Graphics.UserInterface.Menu CreateSubMenu() => new SubMenu
        {
            MaxHeight = MaxHeight,
        };

        protected override DrawableMenuItem CreateDrawableMenuItem(MenuItem item) => new DrawableEditorBarMenuItem(item);

        internal partial class DrawableEditorBarMenuItem : DrawableMenuItem
        {
            private HoverClickSounds hoverClickSounds = null!;
            private TextContainer text = null!;

            public DrawableEditorBarMenuItem(MenuItem item)
                : base(item)
            {
            }

            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider)
            {
                ForegroundColour = colourProvider.Light3;
                BackgroundColour = colourProvider.Background2;
                ForegroundColourHover = colourProvider.Content1;
                BackgroundColourHover = colourProvider.Background1;

                AddInternal(hoverClickSounds = new HoverClickSounds(HoverSampleSet.MenuOpen));
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                Foreground.Anchor = Anchor.CentreLeft;
                Foreground.Origin = Anchor.CentreLeft;
                Item.Action.BindDisabledChanged(_ => updateState(), true);
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
                hoverClickSounds.Enabled.Value = IsActionable;
                Alpha = IsActionable ? 1 : 0.2f;

                if (IsHovered && IsActionable)
                {
                    text.BoldText.FadeIn(DrawableOsuMenuItem.TRANSITION_LENGTH, Easing.OutQuint);
                    text.NormalText.FadeOut(DrawableOsuMenuItem.TRANSITION_LENGTH, Easing.OutQuint);
                }
                else
                {
                    text.BoldText.FadeOut(DrawableOsuMenuItem.TRANSITION_LENGTH, Easing.OutQuint);
                    text.NormalText.FadeIn(DrawableOsuMenuItem.TRANSITION_LENGTH, Easing.OutQuint);
                }
            }

            protected override void UpdateBackgroundColour()
            {
                if (State == MenuItemState.Selected)
                    Background.FadeColour(BackgroundColourHover);
                else
                    base.UpdateBackgroundColour();
            }

            protected override void UpdateForegroundColour()
            {
                if (State == MenuItemState.Selected)
                    Foreground.FadeColour(ForegroundColourHover);
                else
                    base.UpdateForegroundColour();
            }

            protected sealed override Drawable CreateContent() => text = new TextContainer();
        }

        private partial class TextContainer : Container, IHasText
        {
            public LocalisableString Text
            {
                get => NormalText.Text;
                set
                {
                    NormalText.Text = value;
                    BoldText.Text = value;
                }
            }

            public readonly SpriteText NormalText;
            public readonly SpriteText BoldText;

            public TextContainer()
            {
                AutoSizeAxes = Axes.Both;

                Child = new Container
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,

                    AutoSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Horizontal = 17, Vertical = DrawableOsuMenuItem.MARGIN_VERTICAL, },

                    Children = new Drawable[]
                    {
                        NormalText = new OsuSpriteText
                        {
                            AlwaysPresent = true, // ensures that the menu item does not change width when switching between normal and bold text.
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Font = OsuFont.GetFont(size: DrawableOsuMenuItem.TEXT_SIZE),
                        },
                        BoldText = new OsuSpriteText
                        {
                            AlwaysPresent = true, // ensures that the menu item does not change width when switching between normal and bold text.
                            Alpha = 0,
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Font = OsuFont.GetFont(size: DrawableOsuMenuItem.TEXT_SIZE, weight: FontWeight.Bold),
                        }
                    }
                };
            }
        }

        private partial class SubMenu : OsuMenu
        {
            public SubMenu()
                : base(Direction.Vertical)
            {
                ItemsContainer.Padding = new MarginPadding();

                MaskingContainer.CornerRadius = 0;
            }

            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider)
            {
                BackgroundColour = colourProvider.Background2;
            }

            protected override Framework.Graphics.UserInterface.Menu CreateSubMenu() => new SubMenu
            {
                MaxHeight = MaxHeight,
            };

            protected override DrawableMenuItem CreateDrawableMenuItem(MenuItem item)
            {
                switch (item)
                {
                    case OsuMenuItemSpacer spacer:
                        return new DrawableSpacer(spacer);

                    case StatefulMenuItem stateful:
                        return new EditorStatefulMenuItem(stateful);

                    default:
                        return new EditorMenuItem(item);
                }
            }

            private partial class EditorStatefulMenuItem : DrawableStatefulMenuItem
            {
                public EditorStatefulMenuItem(StatefulMenuItem item)
                    : base(item)
                {
                }

                [BackgroundDependencyLoader]
                private void load(OverlayColourProvider colourProvider)
                {
                    BackgroundColour = colourProvider.Background2;
                    BackgroundColourHover = colourProvider.Background1;

                    Foreground.Padding = new MarginPadding { Vertical = 2 };
                }
            }

            private partial class EditorMenuItem : DrawableOsuMenuItem
            {
                public EditorMenuItem(MenuItem item)
                    : base(item)
                {
                }

                [BackgroundDependencyLoader]
                private void load(OverlayColourProvider colourProvider)
                {
                    BackgroundColour = colourProvider.Background2;
                    BackgroundColourHover = colourProvider.Background1;

                    Foreground.Padding = new MarginPadding { Vertical = 2 };
                }
            }
        }
    }
}
