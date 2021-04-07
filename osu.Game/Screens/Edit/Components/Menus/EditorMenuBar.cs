// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Edit.Components.Menus
{
    public class EditorMenuBar : OsuMenu
    {
        public readonly Bindable<EditorScreenMode> Mode = new Bindable<EditorScreenMode>();

        public EditorMenuBar()
            : base(Direction.Horizontal, true)
        {
            RelativeSizeAxes = Axes.X;

            MaskingContainer.CornerRadius = 0;
            ItemsContainer.Padding = new MarginPadding { Left = 100 };
            BackgroundColour = Color4Extensions.FromHex("111");

            ScreenSelectionTabControl tabControl;
            AddRangeInternal(new Drawable[]
            {
                tabControl = new ScreenSelectionTabControl
                {
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.BottomRight,
                    X = -15
                }
            });

            Mode.BindTo(tabControl.Current);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Mode.TriggerChange();
        }

        protected override Framework.Graphics.UserInterface.Menu CreateSubMenu() => new SubMenu();

        protected override DrawableMenuItem CreateDrawableMenuItem(MenuItem item) => new DrawableEditorBarMenuItem(item);

        private class DrawableEditorBarMenuItem : DrawableOsuMenuItem
        {
            private BackgroundBox background;

            public DrawableEditorBarMenuItem(MenuItem item)
                : base(item)
            {
                Anchor = Anchor.CentreLeft;
                Origin = Anchor.CentreLeft;

                StateChanged += stateChanged;
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                ForegroundColour = colours.BlueLight;
                BackgroundColour = Color4.Transparent;
                ForegroundColourHover = Color4.White;
                BackgroundColourHover = colours.Gray3;
            }

            public override void SetFlowDirection(Direction direction)
            {
                AutoSizeAxes = Axes.Both;
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

            private void stateChanged(MenuItemState newState)
            {
                if (newState == MenuItemState.Selected)
                    background.Expand();
                else
                    background.Contract();
            }

            protected override Drawable CreateBackground() => background = new BackgroundBox();
            protected override DrawableOsuMenuItem.TextContainer CreateTextContainer() => new TextContainer();

            private new class TextContainer : DrawableOsuMenuItem.TextContainer
            {
                public TextContainer()
                {
                    NormalText.Font = NormalText.Font.With(size: 14);
                    BoldText.Font = BoldText.Font.With(size: 14);
                    NormalText.Margin = BoldText.Margin = new MarginPadding { Horizontal = 10, Vertical = MARGIN_VERTICAL };
                }
            }

            private class BackgroundBox : CompositeDrawable
            {
                private readonly Container innerBackground;

                public BackgroundBox()
                {
                    RelativeSizeAxes = Axes.Both;
                    Masking = true;
                    InternalChild = innerBackground = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Masking = true,
                        CornerRadius = 4,
                        Child = new Box { RelativeSizeAxes = Axes.Both }
                    };
                }

                /// <summary>
                /// Expands the background such that it doesn't show the bottom corners.
                /// </summary>
                public void Expand() => innerBackground.Height = 2;

                /// <summary>
                /// Contracts the background such that it shows the bottom corners.
                /// </summary>
                public void Contract() => innerBackground.Height = 1;
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

            protected override DrawableMenuItem CreateDrawableMenuItem(MenuItem item)
            {
                switch (item)
                {
                    case EditorMenuItemSpacer spacer:
                        return new DrawableSpacer(spacer);
                }

                return base.CreateDrawableMenuItem(item);
            }

            private class DrawableSpacer : DrawableOsuMenuItem
            {
                public DrawableSpacer(MenuItem item)
                    : base(item)
                {
                }

                protected override bool OnHover(HoverEvent e) => true;

                protected override bool OnClick(ClickEvent e) => true;
            }
        }
    }
}
