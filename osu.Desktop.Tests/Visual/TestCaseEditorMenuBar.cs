// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

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
            Add(new MenuBar
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
                            new EditorContextMenuItem("Clear All Notes"),
                            new EditorContextMenuItem("Open Difficulty..."),
                            new EditorContextMenuItem("Save"),
                            new EditorContextMenuItem("Create a new Difficulty..."),
                            new EditorContextMenuSpacer(),
                            new EditorContextMenuItem("Revert to Saved"),
                            new EditorContextMenuItem("Revert to Saved (Full)"),
                            new EditorContextMenuSpacer(),
                            new EditorContextMenuItem("Test Beatmap"),
                            new EditorContextMenuItem("Open AiMod"),
                            new EditorContextMenuSpacer(),
                            new EditorContextMenuItem("Upload Beatmap..."),
                            new EditorContextMenuItem("Export Package"),
                            new EditorContextMenuItem("Export Map Package"),
                            new EditorContextMenuItem("Import from..."),
                            new EditorContextMenuSpacer(),
                            new EditorContextMenuItem("Open Song Folder"),
                            new EditorContextMenuItem("Open .osu in Notepad"),
                            new EditorContextMenuItem("Open .osb in Notepad"),
                            new EditorContextMenuSpacer(),
                            new EditorContextMenuItem("Exit"),
                        }
                    },
                    new EditorMenuBarItem("Timing")
                    {
                        Items = new[]
                        {
                            new EditorContextMenuItem("Time Signature"),
                            new EditorContextMenuItem("Metronome Clicks"),
                            new EditorContextMenuSpacer(),
                            new EditorContextMenuItem("Add Timing Section"),
                            new EditorContextMenuItem("Add Inheriting Section"),
                            new EditorContextMenuItem("Reset Current Section"),
                            new EditorContextMenuItem("Delete Timing Section"),
                            new EditorContextMenuItem("Resnap Current Section"),
                            new EditorContextMenuSpacer(),
                            new EditorContextMenuItem("Timing Setup"),
                            new EditorContextMenuSpacer(),
                            new EditorContextMenuItem("Resnap All Notes", MenuItemType.Destructive),
                            new EditorContextMenuItem("Move all notes in time...", MenuItemType.Destructive),
                            new EditorContextMenuItem("Recalculate Slider Lengths", MenuItemType.Destructive),
                            new EditorContextMenuItem("Delete All Timing Sections", MenuItemType.Destructive),
                            new EditorContextMenuSpacer(),
                            new EditorContextMenuItem("Set Current Position as Preview Point"),
                        }
                    },
                    new EditorMenuBarItem("Testing")
                    {
                        Items = new[]
                        {
                            new EditorContextMenuItem("Item 1"),
                            new EditorContextMenuItem("Item 2"),
                            new EditorContextMenuItem("Item 3"),
                        }
                    },
                }
            });
        }

        private class EditorMenuBarItem : MenuBarItem
        {
            private const int fade_duration = 250;
            private const float text_size = 17;

            private readonly Container background;

            private Color4 normalColour;

            public EditorMenuBarItem(string title)
                : base(title)
            {
                Content.Padding = new MarginPadding(8);

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
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                background.Colour = colours.Gray3;
                TitleText.Colour = normalColour = colours.BlueLight;
            }

            public override void Open()
            {
                base.Open();

                background.FadeIn(fade_duration, Easing.OutQuint);
                TitleText.FadeColour(Color4.White, fade_duration, Easing.OutQuint);
            }

            public override void Close()
            {
                base.Close();

                background.FadeOut(fade_duration, Easing.OutQuint);
                TitleText.FadeColour(normalColour, fade_duration, Easing.OutQuint);
            }

            protected override SpriteText CreateTitleText() => new OsuSpriteText { TextSize = text_size };

            protected override ContextMenu<ContextMenuItem> CreateContextMenu() => new EditorContextMenu
            {
                OriginPosition = new Vector2(8, 0)
            };
        }

        private class EditorContextMenu : OsuContextMenu<ContextMenuItem>
        {
            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                Menu.Background.Colour = colours.Gray3;
            }
        }

        private class EditorContextMenuSpacer : EditorContextMenuItem
        {
            public override bool HandleInput => false;

            public EditorContextMenuSpacer()
                : base(" ")
            {
            }
        }

        private class EditorContextMenuItem : OsuContextMenuItem
        {
            private const int min_text_length = 40;

            public EditorContextMenuItem(string title, MenuItemType type = MenuItemType.Standard)
                : base(title.PadRight(min_text_length), type)
            {
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
