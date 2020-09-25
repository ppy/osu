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
using osuTK.Graphics;

namespace osu.Game.Graphics.UserInterfaceV2
{
    public class DirectorySelector : CompositeDrawable
    {
        private FillFlowContainer directoryFlow;

        [Resolved]
        private GameHost host { get; set; }

        [Cached]
        public readonly Bindable<DirectoryInfo> CurrentPath = new Bindable<DirectoryInfo>();

        public DirectorySelector(string initialPath = null)
        {
            CurrentPath.Value = new DirectoryInfo(initialPath ?? Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Padding = new MarginPadding(10);

            InternalChild = new GridContainer
            {
                RelativeSizeAxes = Axes.Both,
                RowDimensions = new[]
                {
                    new Dimension(GridSizeMode.Absolute, 50),
                    new Dimension(),
                },
                Content = new[]
                {
                    new Drawable[]
                    {
                        new CurrentDirectoryDisplay
                        {
                            RelativeSizeAxes = Axes.Both,
                        },
                    },
                    new Drawable[]
                    {
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
                }
            };

            CurrentPath.BindValueChanged(updateDisplay, true);
        }

        private void updateDisplay(ValueChangedEvent<DirectoryInfo> directory)
        {
            directoryFlow.Clear();

            try
            {
                if (directory.NewValue == null)
                {
                    var drives = DriveInfo.GetDrives();

                    foreach (var drive in drives)
                        directoryFlow.Add(new DirectoryPiece(drive.RootDirectory));
                }
                else
                {
                    directoryFlow.Add(new ParentDirectoryPiece(CurrentPath.Value.Parent));

                    directoryFlow.AddRange(GetEntriesForPath(CurrentPath.Value));
                }
            }
            catch (Exception)
            {
                CurrentPath.Value = directory.OldValue;
                this.FlashColour(Color4.Red, 300);
            }
        }

        protected virtual IEnumerable<DisplayPiece> GetEntriesForPath(DirectoryInfo path)
        {
            foreach (var dir in path.GetDirectories().OrderBy(d => d.Name))
            {
                if ((dir.Attributes & FileAttributes.Hidden) == 0)
                    yield return new DirectoryPiece(dir);
            }
        }

        private class CurrentDirectoryDisplay : CompositeDrawable
        {
            [Resolved]
            private Bindable<DirectoryInfo> currentDirectory { get; set; }

            private FillFlowContainer flow;

            [BackgroundDependencyLoader]
            private void load()
            {
                InternalChildren = new Drawable[]
                {
                    flow = new FillFlowContainer
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.X,
                        Spacing = new Vector2(5),
                        Height = DisplayPiece.HEIGHT,
                        Direction = FillDirection.Horizontal,
                    },
                };

                currentDirectory.BindValueChanged(updateDisplay, true);
            }

            private void updateDisplay(ValueChangedEvent<DirectoryInfo> dir)
            {
                flow.Clear();

                List<DirectoryPiece> pathPieces = new List<DirectoryPiece>();

                DirectoryInfo ptr = dir.NewValue;

                while (ptr != null)
                {
                    pathPieces.Insert(0, new CurrentDisplayPiece(ptr));
                    ptr = ptr.Parent;
                }

                flow.ChildrenEnumerable = new Drawable[]
                {
                    new OsuSpriteText { Text = "Current Directory: ", Font = OsuFont.Default.With(size: DisplayPiece.HEIGHT), },
                    new ComputerPiece(),
                }.Concat(pathPieces);
            }

            private class ComputerPiece : CurrentDisplayPiece
            {
                protected override IconUsage? Icon => null;

                public ComputerPiece()
                    : base(null, "Computer")
                {
                }
            }

            private class CurrentDisplayPiece : DirectoryPiece
            {
                public CurrentDisplayPiece(DirectoryInfo directory, string displayName = null)
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

                protected override IconUsage? Icon => Directory.Name.Contains(Path.DirectorySeparatorChar) ? base.Icon : null;
            }
        }

        private class ParentDirectoryPiece : DirectoryPiece
        {
            protected override IconUsage? Icon => FontAwesome.Solid.Folder;

            public ParentDirectoryPiece(DirectoryInfo directory)
                : base(directory, "..")
            {
            }
        }

        protected class DirectoryPiece : DisplayPiece
        {
            protected readonly DirectoryInfo Directory;

            [Resolved]
            private Bindable<DirectoryInfo> currentDirectory { get; set; }

            public DirectoryPiece(DirectoryInfo directory, string displayName = null)
                : base(displayName)
            {
                Directory = directory;
            }

            protected override bool OnClick(ClickEvent e)
            {
                currentDirectory.Value = Directory;
                return true;
            }

            protected override string FallbackName => Directory.Name;

            protected override IconUsage? Icon => Directory.Name.Contains(Path.DirectorySeparatorChar)
                ? FontAwesome.Solid.Database
                : FontAwesome.Regular.Folder;
        }

        protected abstract class DisplayPiece : CompositeDrawable
        {
            public const float HEIGHT = 20;

            protected const float FONT_SIZE = 16;

            private readonly string displayName;

            protected FillFlowContainer Flow;

            protected DisplayPiece(string displayName = null)
            {
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
                    Text = displayName ?? FallbackName,
                    Font = OsuFont.Default.With(size: FONT_SIZE)
                });
            }

            protected abstract string FallbackName { get; }

            protected abstract IconUsage? Icon { get; }
        }
    }
}
