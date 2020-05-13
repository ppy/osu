// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.Platform;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osuTK;

namespace osu.Game.Graphics.UserInterfaceV2
{
    public class DirectorySelector : CompositeDrawable
    {
        private FillFlowContainer directoryFlow;

        [Resolved]
        private GameHost host { get; set; }

        [Cached]
        private readonly Bindable<DirectoryInfo> currentDirectory = new Bindable<DirectoryInfo>();

        public DirectorySelector(string initialPath = null)
        {
            currentDirectory.Value = new DirectoryInfo(initialPath ??= Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Padding = new MarginPadding(10);

            InternalChildren = new Drawable[]
            {
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        new CurrentDirectoryDisplay
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = 50,
                        },
                        new OsuScrollContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            Child = directoryFlow = new FillFlowContainer
                            {
                                AutoSizeAxes = Axes.Y,
                                RelativeSizeAxes = Axes.X,
                                Direction = FillDirection.Vertical,
                                Spacing = new Vector2(2),
                            }
                        }
                    }
                },
            };

            currentDirectory.BindValueChanged(updateDisplay, true);
        }

        private void updateDisplay(ValueChangedEvent<DirectoryInfo> directory)
        {
            directoryFlow.Clear();

            if (directory.NewValue == null)
            {
                var drives = DriveInfo.GetDrives();

                foreach (var drive in drives)
                    directoryFlow.Add(new DirectoryRow(drive.RootDirectory));
            }
            else
            {
                directoryFlow.Add(new ParentDirectoryRow(currentDirectory.Value.Parent));

                foreach (var dir in currentDirectory.Value.GetDirectories().OrderBy(d => d.Name))
                {
                    if ((dir.Attributes & FileAttributes.Hidden) == 0)
                        directoryFlow.Add(new DirectoryRow(dir));
                }
            }
        }

        public class CurrentDirectoryDisplay : CompositeDrawable
        {
            [Resolved]
            private Bindable<DirectoryInfo> currentDirectory { get; set; }

            [BackgroundDependencyLoader]
            private void load()
            {
                FillFlowContainer flow;

                InternalChildren = new Drawable[]
                {
                    flow = new FillFlowContainer
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.X,
                        Spacing = new Vector2(5),
                        Height = DirectoryRow.HEIGHT,
                        Direction = FillDirection.Horizontal,
                    },
                };

                currentDirectory.BindValueChanged(dir =>
                {
                    flow.Clear();

                    flow.Add(new OsuSpriteText
                    {
                        Text = "Current Directory: ",
                        Font = OsuFont.Default.With(size: DirectoryRow.HEIGHT),
                    });

                    flow.Add(new ComputerRow());

                    List<DirectoryRow> traversalRows = new List<DirectoryRow>();

                    DirectoryInfo traverse = dir.NewValue;

                    while (traverse != null)
                    {
                        traversalRows.Insert(0, new CurrentDisplayRow(traverse));
                        traverse = traverse.Parent;
                    }

                    flow.AddRange(traversalRows);
                }, true);
            }

            private class ComputerRow : CurrentDisplayRow
            {
                public override IconUsage? Icon => null;

                public ComputerRow()
                    : base(null, "Computer")
                {
                }
            }

            private class CurrentDisplayRow : DirectoryRow
            {
                public CurrentDisplayRow(DirectoryInfo directory, string displayName = null)
                    : base(directory, displayName)
                {
                }

                [BackgroundDependencyLoader]
                private void load()
                {
                    Flow.Add(new SpriteIcon
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Icon = FontAwesome.Solid.ChevronRight,
                        Size = new Vector2(FONT_SIZE / 2)
                    });
                }
            }
        }

        private class ParentDirectoryRow : DirectoryRow
        {
            public override IconUsage? Icon => FontAwesome.Solid.Folder;

            public ParentDirectoryRow(DirectoryInfo directory)
                : base(directory, "..")
            {
            }
        }

        private class DirectoryRow : CompositeDrawable
        {
            public const float HEIGHT = 20;

            protected const float FONT_SIZE = 16;

            private readonly DirectoryInfo directory;
            private readonly string displayName;

            protected FillFlowContainer Flow;

            [Resolved]
            private Bindable<DirectoryInfo> currentDirectory { get; set; }

            public DirectoryRow(DirectoryInfo directory, string displayName = null)
            {
                this.directory = directory;
                this.displayName = displayName;
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                AutoSizeAxes = Axes.Both;

                Masking = true;
                CornerRadius = 5;

                InternalChildren = new Drawable[]
                {
                    new Box
                    {
                        Colour = colours.GreySeafoamDarker,
                        RelativeSizeAxes = Axes.Both,
                    },
                    Flow = new FillFlowContainer
                    {
                        AutoSizeAxes = Axes.X,
                        Height = 20,
                        Margin = new MarginPadding { Vertical = 2, Horizontal = 5 },
                        Direction = FillDirection.Horizontal,
                        Spacing = new Vector2(5),
                    }
                };

                if (Icon.HasValue)
                {
                    Flow.Add(new SpriteIcon
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Icon = Icon.Value,
                        Size = new Vector2(FONT_SIZE)
                    });
                }

                Flow.Add(new OsuSpriteText
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Text = displayName ?? directory.Name,
                    Font = OsuFont.Default.With(size: FONT_SIZE)
                });
            }

            protected override bool OnClick(ClickEvent e)
            {
                currentDirectory.Value = directory;
                return true;
            }

            public virtual IconUsage? Icon => FontAwesome.Regular.Folder;
        }
    }
}
