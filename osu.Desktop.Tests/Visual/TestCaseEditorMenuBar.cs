// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.Edit.Menus;

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
    }
}
