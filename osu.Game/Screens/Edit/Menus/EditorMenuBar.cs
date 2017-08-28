// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Screens.Edit.Menus
{
    public class EditorMenuBar : MenuBar
    {
        protected override DrawableMenuBarItem CreateDrawableMenuBarItem(MenuItem item) => new DrawableEditorMenuBarItem(item);

        private class DrawableEditorMenuBarItem : DrawableMenuBarItem
        {
            private const int fade_duration = 250;
            private const float text_size = 14;

            private readonly Container background;

            private Color4 normalColour;

            public DrawableEditorMenuBarItem(MenuItem item)
                : base(item)
            {
                Text.Padding = new MarginPadding(8);

                AddInternal(background = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    Depth = float.MaxValue,
                    Alpha = 0,
                    Child = new Container<Box>
                    {
                        // The following is done so we can have top rounded corners but not bottom corners
                        RelativeSizeAxes = Axes.Both,
                        Height = 2,
                        Masking = true,
                        CornerRadius = 5,
                        Child = new Box { RelativeSizeAxes = Axes.Both }
                    }
                });

                Menu.OnOpen += menuOpen;
                Menu.OnClose += menuClose;
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                background.Colour = colours.Gray3;
                Text.Colour = normalColour = colours.BlueLight;
            }

            private void menuOpen()
            {
                background.FadeIn(fade_duration, Easing.OutQuint);
                Text.FadeColour(Color4.White, fade_duration, Easing.OutQuint);
            }

            private void menuClose()
            {
                background.FadeOut(fade_duration, Easing.OutQuint);
                Text.FadeColour(normalColour, fade_duration, Easing.OutQuint);
            }

            protected override SpriteText CreateText() => new OsuSpriteText { TextSize = text_size };

            protected override Framework.Graphics.UserInterface.Menu CreateMenu() => new EditorMenu();

            private class EditorMenu : OsuMenu
            {
                public EditorMenu()
                {
                    Anchor = Anchor.BottomLeft;
                    BypassAutoSizeAxes = Axes.Both;
                    OriginPosition = new Vector2(8, 0);
                }

                [BackgroundDependencyLoader]
                private void load(OsuColour colours)
                {
                    BackgroundColour = colours.Gray3;
                }

                protected override MarginPadding ItemFlowContainerPadding => new MarginPadding { Top = 5, Bottom = 5 };

                protected override DrawableMenuItem CreateDrawableMenuItem(MenuItem item) => new DrawableEditorMenuItem(item);

                private class DrawableEditorMenuItem : DrawableOsuMenuItem
                {
                    public override bool HandleInput => !isSpacer;
                    private readonly bool isSpacer;

                    public DrawableEditorMenuItem(MenuItem item)
                        : base(item)
                    {
                        isSpacer = item is EditorMenuSpacer;
                    }

                    [BackgroundDependencyLoader]
                    private void load(OsuColour colours)
                    {
                        BackgroundColour = colours.Gray3;
                        BackgroundColourHover = colours.Gray2;
                    }

                    protected override TextContainer CreateTextContainer() => new EditorTextContainer();

                    private class EditorTextContainer : TextContainer
                    {
                        public EditorTextContainer()
                        {
                            BoldText.TextSize = text_size;
                            NormalText.TextSize = text_size;
                        }
                    }
                }
            }
        }
    }
}
