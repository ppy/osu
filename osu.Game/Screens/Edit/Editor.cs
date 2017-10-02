﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using osu.Framework.Screens;
using osu.Game.Screens.Backgrounds;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.Edit.Menus;
using osu.Game.Screens.Edit.Components.Timelines.Summary;
using OpenTK;
using osu.Framework.Allocation;

namespace osu.Game.Screens.Edit
{
    internal class Editor : OsuScreen
    {
        protected override BackgroundScreen CreateBackground() => new BackgroundScreenCustom(@"Backgrounds/bg4");

        internal override bool ShowOverlays => false;

        private readonly Box bottomBackground;

        public Editor()
        {
            Add(new Container
            {
                RelativeSizeAxes = Axes.X,
                Height = 40,
                Children = new Drawable[]
                {
                    new EditorMenuBar
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        RelativeSizeAxes = Axes.Both,
                        Items = new[]
                        {
                            new EditorMenuBarItem("File")
                            {
                                Items = new[]
                                {
                                    new EditorMenuItem("Clear all notes"),
                                    new EditorMenuItem("Open difficulty..."),
                                    new EditorMenuItem("Save"),
                                    new EditorMenuItem("Create new difficulty..."),
                                    new EditorMenuItemSpacer(),
                                    new EditorMenuItem("Revert to saved"),
                                    new EditorMenuItem("Revert to saved (full"),
                                    new EditorMenuItemSpacer(),
                                    new EditorMenuItem("Test beatmap"),
                                    new EditorMenuItem("Open AiMod"),
                                    new EditorMenuItemSpacer(),
                                    new EditorMenuItem("Upload Beatmap..."),
                                    new EditorMenuItem("Export package"),
                                    new EditorMenuItem("Export map package"),
                                    new EditorMenuItem("Import from..."),
                                    new EditorMenuItemSpacer(),
                                    new EditorMenuItem("Open song folder"),
                                    new EditorMenuItem("Open .osu in Notepad"),
                                    new EditorMenuItem("Open .osb in Notepad"),
                                    new EditorMenuItemSpacer(),
                                    new EditorMenuItem("Exit", MenuItemType.Standard, Exit)
                                }
                            },
                            new EditorMenuBarItem("Edit")
                            {
                                Items = new[]
                                {
                                    new EditorMenuItem("Undo"),
                                    new EditorMenuItem("Redo"),
                                    new EditorMenuItemSpacer(),
                                    new EditorMenuItem("Cut"),
                                    new EditorMenuItem("Copy"),
                                    new EditorMenuItem("Paste"),
                                    new EditorMenuItem("Delete"),
                                    new EditorMenuItemSpacer(),
                                    new EditorMenuItem("Select all"),
                                    new EditorMenuItem("Clone"),
                                    new EditorMenuItemSpacer(),
                                    new EditorMenuItem("Reverse selection"),
                                    new EditorMenuItem("Flip horizontally"),
                                    new EditorMenuItem("Flip vertically"),
                                    new EditorMenuItem("Rotate 90deg clockwise"),
                                    new EditorMenuItem("Rotate 90deg anticlockwise"),
                                    new EditorMenuItem("Rotate by..."),
                                    new EditorMenuItem("Scale by..."),
                                    new EditorMenuItemSpacer(),
                                    new EditorMenuItem("Reset selected objects' samples"),
                                    new EditorMenuItem("Reset all samples", MenuItemType.Destructive),
                                    new EditorMenuItem("Reset combo colours", MenuItemType.Destructive),
                                    new EditorMenuItem("Reset breaks", MenuItemType.Destructive),
                                    new EditorMenuItemSpacer(),
                                    new EditorMenuItem("Nudge backward"),
                                    new EditorMenuItem("Nudge forward")
                                }
                            },
                            new EditorMenuBarItem("View")
                            {
                                Items = new[]
                                {
                                    new EditorMenuItem("Compose"),
                                    new EditorMenuItem("Design"),
                                    new EditorMenuItem("Timing"),
                                    new EditorMenuItemSpacer(),
                                    new EditorMenuItem("Song setup..."),
                                    new EditorMenuItem("Timing setup..."),
                                    new EditorMenuItemSpacer(),
                                    new EditorMenuItem("Volume"),
                                    new EditorMenuItem("Grid level"),
                                    new EditorMenuItem("Show video"),
                                    new EditorMenuItem("Show sample name"),
                                    new EditorMenuItem("Snaking sliders"),
                                    new EditorMenuItem("Hit animations"),
                                    new EditorMenuItem("Follow points"),
                                    new EditorMenuItem("Stacking")
                                }
                            },
                            new EditorMenuBarItem("Compose")
                            {
                                Items = new[]
                                {
                                    new EditorMenuItem("Snap divisor"),
                                    new EditorMenuItem("Audio rate"),
                                    new EditorMenuItem("Grid snapping"),
                                    new EditorMenuItemSpacer(),
                                    new EditorMenuItem("Create polygon cricles..."),
                                    new EditorMenuItem("Convert slider to stream"),
                                    new EditorMenuItem("Enable live mapping mode"),
                                    new EditorMenuItem("Sample import")
                                }
                            },
                            new EditorMenuBarItem("Design")
                            {
                                Items = new[]
                                {
                                    new EditorMenuItem("Move all elements in time...")
                                }
                            },
                            new EditorMenuBarItem("Timing")
                            {
                                Items = new[]
                                {
                                    new EditorMenuItem("Time signature"),
                                    new EditorMenuItem("Metronome clicks"),
                                    new EditorMenuItemSpacer(),
                                    new EditorMenuItem("Add timing section"),
                                    new EditorMenuItem("Add inheriting section"),
                                    new EditorMenuItem("Reset current section"),
                                    new EditorMenuItem("Delete timing section"),
                                    new EditorMenuItem("Resnap current section"),
                                    new EditorMenuItemSpacer(),
                                    new EditorMenuItem("Timing setup..."),
                                    new EditorMenuItemSpacer(),
                                    new EditorMenuItem("Resnap all notes", MenuItemType.Destructive),
                                    new EditorMenuItem("Move all notes in time...", MenuItemType.Destructive),
                                    new EditorMenuItem("Recalculate slider lengths", MenuItemType.Destructive),
                                    new EditorMenuItem("Delete all timing sections", MenuItemType.Destructive),
                                    new EditorMenuItemSpacer(),
                                    new EditorMenuItem("Set current position as preview point")
                                }
                            },
                            new EditorMenuBarItem("Web")
                            {
                                Items = new[]
                                {
                                    new EditorMenuItem("This Beatmap's information page"),
                                    new EditorMenuItem("This Beatmap's thread"),
                                    new EditorMenuItem("Quick reply")
                                }
                            },
                            new EditorMenuBarItem("Help")
                            {
                                Items = new[]
                                {
                                    new EditorMenuItem("Show in-game help"),
                                    new EditorMenuItem("View FAQ")
                                }
                            }
                        }
                    }
                }
            });

            SummaryTimeline summaryTimeline;
            Add(new Container
            {
                Anchor = Anchor.BottomLeft,
                Origin = Anchor.BottomLeft,
                RelativeSizeAxes = Axes.X,
                Height = 60,
                Children = new Drawable[]
                {
                    bottomBackground = new Box { RelativeSizeAxes = Axes.Both },
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Padding = new MarginPadding { Top = 5, Bottom = 5, Left = 10, Right = 10 },
                        Child = new FillFlowContainer
                        {
                            Name = "Bottom bar",
                            RelativeSizeAxes = Axes.Both,
                            Direction = FillDirection.Horizontal,
                            Spacing = new Vector2(10, 0),
                            Children = new[]
                            {
                                summaryTimeline = new SummaryTimeline
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    RelativeSizeAxes = Axes.Both,
                                    Width = 0.65f
                                }
                            }
                        }
                    }
                }
            });

            summaryTimeline.Beatmap.BindTo(Beatmap);
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            bottomBackground.Colour = colours.Gray2;
        }

        protected override void OnResuming(Screen last)
        {
            Beatmap.Value.Track?.Stop();
            base.OnResuming(last);
        }

        protected override void OnEntering(Screen last)
        {
            base.OnEntering(last);
            Background.FadeColour(Color4.DarkGray, 500);
            Beatmap.Value.Track?.Stop();
        }

        protected override bool OnExiting(Screen next)
        {
            Background.FadeColour(Color4.White, 500);
            Beatmap.Value.Track?.Start();
            return base.OnExiting(next);
        }
    }
}
