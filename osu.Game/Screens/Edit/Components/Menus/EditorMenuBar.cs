// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Screens.Edit.Components.Menus
{
    public class EditorMenuBar : OsuMenu
    {
        public EditorMenuBar()
            : base(Direction.Horizontal, true)
        {
            RelativeSizeAxes = Axes.X;

            MaskingContainer.CornerRadius = 0;
            ItemsContainer.Padding = new MarginPadding { Left = 100 };
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            BackgroundColour = colourProvider.Background3;
        }

        protected override Framework.Graphics.UserInterface.Menu CreateSubMenu() => new SubMenu();

        protected override DrawableMenuItem CreateDrawableMenuItem(MenuItem item) => new DrawableEditorBarMenuItem(item);

        private class DrawableEditorBarMenuItem : DrawableOsuMenuItem
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

            private new class TextContainer : DrawableOsuMenuItem.TextContainer
            {
                public TextContainer()
                {
                    NormalText.Font = OsuFont.TorusAlternate;
                    BoldText.Font = OsuFont.TorusAlternate.With(weight: FontWeight.Bold);
                }
            }
        }

        private class SubMenu : OsuMenu
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
                    case EditorMenuItemSpacer spacer:
                        return new DrawableSpacer(spacer);

                    case StatefulMenuItem stateful:
                        return new EditorStatefulMenuItem(stateful);

                    default:
                        return new EditorMenuItem(item);
                }
            }

            private class EditorStatefulMenuItem : DrawableStatefulMenuItem
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

            private class EditorMenuItem : DrawableOsuMenuItem
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

            private class DrawableSpacer : DrawableOsuMenuItem
            {
                public DrawableSpacer(MenuItem item)
                    : base(item)
                {
                    Scale = new Vector2(1, 0.3f);
                }

                protected override bool OnHover(HoverEvent e) => true;

                protected override bool OnClick(ClickEvent e) => true;
            }
        }
    }
}
