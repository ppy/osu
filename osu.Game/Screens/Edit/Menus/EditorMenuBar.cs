// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Screens.Edit.Menus
{
    public class EditorMenuBar : OsuMenu
    {
        public EditorMenuBar()
            : base(Direction.Horizontal, true)
        {
            RelativeSizeAxes = Axes.X;

            ItemsContainer.Padding = new MarginPadding { Left = 100 };
            BackgroundColour = OsuColour.FromHex("111");

            AddRangeInternal(new Drawable[]
            {
                new ScreenSelectionTabControl
                {
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.BottomRight,
                    X = -15
                }
            });
        }

        protected override Framework.Graphics.UserInterface.Menu CreateSubMenu() => new SubMenu();

        protected override DrawableMenuItem CreateDrawableMenuItem(MenuItem item) => new DrawableEditorBarMenuItem(item);

        private class DrawableEditorBarMenuItem : DrawableOsuMenuItem
        {
            private Color4 openedForegroundColour;
            private Color4 openedBackgroundColour;


            public DrawableEditorBarMenuItem(MenuItem item)
                : base(item)
            {
                Anchor = Anchor.CentreLeft;
                Origin = Anchor.CentreLeft;
            }

            public override void SetFlowDirection(Direction direction)
            {
                AutoSizeAxes = Axes.Both;
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                ForegroundColour = ForegroundColourHover = colours.BlueLight;
                BackgroundColour = BackgroundColourHover = Color4.Transparent;
                openedForegroundColour = Color4.White;
                openedBackgroundColour = colours.Gray3;
            }

            protected override void UpdateBackgroundColour()
            {
                if (State == MenuItemState.Selected)
                    Background.FadeColour(openedBackgroundColour);
                else
                    base.UpdateBackgroundColour();
            }

            protected override void UpdateForegroundColour()
            {
                if (State == MenuItemState.Selected)
                    Foreground.FadeColour(openedForegroundColour);
                else
                    base.UpdateForegroundColour();
            }

            protected override void Update()
            {
                base.Update();
            }

            protected override Drawable CreateBackground() => new Container
            {
                RelativeSizeAxes = Axes.Both,
                Masking = true,
                Child = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Height = 2,
                    Masking = true,
                    CornerRadius = 4,
                    Child = new Box { RelativeSizeAxes = Axes.Both }
                }
            };

            protected override DrawableOsuMenuItem.TextContainer CreateTextContainer() => new TextContainer();

            private new class TextContainer : DrawableOsuMenuItem.TextContainer
            {
                public TextContainer()
                {
                    NormalText.TextSize = BoldText.TextSize = 14;
                    NormalText.Margin = BoldText.Margin = new MarginPadding { Horizontal = 10, Vertical = MARGIN_VERTICAL };
                }
            }
        }

        private class SubMenu : OsuMenu
        {
            public SubMenu()
                : base(Direction.Vertical)
            {
                OriginPosition = new Vector2(5, 1);
                ItemsContainer.Padding = new MarginPadding { Top = 5, Bottom = 5 };
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                BackgroundColour = colours.Gray3;
            }

            protected override Framework.Graphics.UserInterface.Menu CreateSubMenu() => new SubMenu();

            protected override DrawableMenuItem CreateDrawableMenuItem(MenuItem item) => new DrawableSubMenuItem(item);

            private class DrawableSubMenuItem : DrawableOsuMenuItem
            {
                public DrawableSubMenuItem(MenuItem item)
                    : base(item)
                {
                }

                protected override bool OnHover(InputState state)
                {
                    if (Item is EditorMenuItemSpacer)
                        return true;
                    return base.OnHover(state);
                }

                protected override bool OnClick(InputState state)
                {
                    if (Item is EditorMenuItemSpacer)
                        return true;
                    return base.OnClick(state);
                }
            }
        }
    }
}
