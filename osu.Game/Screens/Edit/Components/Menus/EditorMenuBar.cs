// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics;
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

        protected override Framework.Graphics.UserInterface.Menu CreateSubMenu() => new SubMenu();

        protected override DrawableMenuItem CreateDrawableMenuItem(MenuItem item) => new DrawableEditorBarMenuItem(item);

        private partial class DrawableEditorBarMenuItem : DrawableOsuMenuItem
        {
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
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                Foreground.Anchor = Anchor.CentreLeft;
                Foreground.Origin = Anchor.CentreLeft;
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

            protected override DrawableOsuMenuItem.TextContainer CreateTextContainer() => new TextContainer();

            private new partial class TextContainer : DrawableOsuMenuItem.TextContainer
            {
                public TextContainer()
                {
                    NormalText.Font = OsuFont.TorusAlternate;
                    BoldText.Font = OsuFont.TorusAlternate.With(weight: FontWeight.Bold);
                }
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

            protected override Framework.Graphics.UserInterface.Menu CreateSubMenu() => new SubMenu();

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

                private bool hasSubmenu => Item.Items.Any();

                protected override TextContainer CreateTextContainer() => base.CreateTextContainer().With(c =>
                {
                    c.Padding = new MarginPadding
                    {
                        // Add some padding for the chevron below.
                        Right = hasSubmenu ? 5 : 0,
                    };
                });

                [BackgroundDependencyLoader]
                private void load(OverlayColourProvider colourProvider)
                {
                    BackgroundColour = colourProvider.Background2;
                    BackgroundColourHover = colourProvider.Background1;

                    Foreground.Padding = new MarginPadding { Vertical = 2 };

                    if (hasSubmenu)
                    {
                        AddInternal(new SpriteIcon
                        {
                            Margin = new MarginPadding(6),
                            Size = new Vector2(8),
                            Icon = FontAwesome.Solid.ChevronRight,
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreRight,
                        });
                    }
                }
            }
        }
    }
}
