// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Testing;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;

namespace osu.Desktop.Tests.Visual
{
    public class TestCaseEditorMenuBar : TestCase
    {
        public TestCaseEditorMenuBar()
        {
            Add(new EditorMenuBar
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                Y = 50,
                Items = new[]
                {
                    new EditorMenuBarItem("File")
                    {
                        Items = new[]
                        {
                            new EditorMenuItem("Clear All Notes"),
                            new EditorMenuItem("Open Difficulty..."),
                            new EditorMenuItem("Save"),
                            new EditorMenuItem("Create a new Difficulty..."),
                            new EditorMenuSpacer(),
                            new EditorMenuItem("Revert to Saved"),
                            new EditorMenuItem("Revert to Saved (Full)"),
                            new EditorMenuSpacer(),
                            new EditorMenuItem("Test Beatmap"),
                            new EditorMenuItem("Open AiMod"),
                            new EditorMenuSpacer(),
                            new EditorMenuItem("Upload Beatmap..."),
                            new EditorMenuItem("Export Package"),
                            new EditorMenuItem("Export Map Package"),
                            new EditorMenuItem("Import from..."),
                            new EditorMenuSpacer(),
                            new EditorMenuItem("Open Song Folder"),
                            new EditorMenuItem("Open .osu in Notepad"),
                            new EditorMenuItem("Open .osb in Notepad"),
                            new EditorMenuSpacer(),
                            new EditorMenuItem("Exit"),
                        }
                    },
                    new EditorMenuBarItem("Timing")
                    {
                        Items = new[]
                        {
                            new EditorMenuItem("Time Signature"),
                            new EditorMenuItem("Metronome Clicks"),
                            new EditorMenuSpacer(),
                            new EditorMenuItem("Add Timing Section"),
                            new EditorMenuItem("Add Inheriting Section"),
                            new EditorMenuItem("Reset Current Section"),
                            new EditorMenuItem("Delete Timing Section"),
                            new EditorMenuItem("Resnap Current Section"),
                            new EditorMenuSpacer(),
                            new EditorMenuItem("Timing Setup"),
                            new EditorMenuSpacer(),
                            new EditorMenuItem("Resnap All Notes", MenuItemType.Destructive),
                            new EditorMenuItem("Move all notes in time...", MenuItemType.Destructive),
                            new EditorMenuItem("Recalculate Slider Lengths", MenuItemType.Destructive),
                            new EditorMenuItem("Delete All Timing Sections", MenuItemType.Destructive),
                            new EditorMenuSpacer(),
                            new EditorMenuItem("Set Current Position as Preview Point"),
                        }
                    },
                    new EditorMenuBarItem("Testing")
                    {
                        Items = new[]
                        {
                            new EditorMenuItem("Item 1"),
                            new EditorMenuItem("Item 2"),
                            new EditorMenuItem("Item 3"),
                        }
                    },
                }
            });
        }

        private class EditorMenuBar : MenuBar
        {
            protected override DrawableMenuBarItem CreateDrawableMenuBarItem(MenuItem item) => new DrawableEditorMenuBarItem(item);

            private class DrawableEditorMenuBarItem : DrawableMenuBarItem
            {
                private const int fade_duration = 250;
                private const float text_size = 17;

                private readonly Container background;

                private Color4 normalColour;

                public DrawableEditorMenuBarItem(MenuItem item)
                    : base(item)
                {
                    if (!(item is EditorMenuBarItem))
                        throw new ArgumentException($"{nameof(item)} must be a {nameof(EditorMenuBarItem)}.");

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

                protected override Menu CreateMenu() => new EditorMenu();

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

                    protected override DrawableMenuItem CreateDrawableMenuItem(MenuItem item) => new DrawableEditorMenuItem(item);

                    private class DrawableEditorMenuItem : DrawableOsuMenuItem
                    {
                        public override bool HandleInput => !isSpacer;
                        private readonly bool isSpacer;

                        public DrawableEditorMenuItem(MenuItem item)
                            : base(item)
                        {
                            if (!(item is EditorMenuItem))
                                throw new ArgumentException($"{nameof(item)} must be a {nameof(EditorMenuItem)}.");

                            isSpacer = item is EditorMenuSpacer;
                        }

                        [BackgroundDependencyLoader]
                        private void load(OsuColour colours)
                        {
                            BackgroundColour = colours.Gray3;
                            BackgroundColourHover = colours.Gray2;
                        }
                    }
                }
            }
        }

        private class EditorMenuBarItem : MenuItem
        {
            public EditorMenuBarItem(string text)
                : base(text)
            {
            }
        }

        private class EditorMenuItem : OsuMenuItem
        {
            private const int min_text_length = 40;

            public EditorMenuItem(string text, MenuItemType type = MenuItemType.Standard)
                : base(text.PadRight(min_text_length), type)
            {
            }

            public EditorMenuItem(string text, MenuItemType type, Action action)
                : base(text.PadRight(min_text_length), type, action)
            {
            }
        }

        private class EditorMenuSpacer : EditorMenuItem
        {
            public EditorMenuSpacer()
                : base(" ")
            {
            }
        }
    }
}
