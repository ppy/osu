// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.Platform;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osuTK;

namespace osu.Game.Tests.Visual.Settings
{
    public class TestSceneDirectorySelector : OsuTestScene
    {
        [BackgroundDependencyLoader]
        private void load(GameHost host)
        {
            Add(new DirectorySelector { RelativeSizeAxes = Axes.Both });
        }
    }

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
                // var drives = DriveInfo.GetDrives();
                //
                // foreach (var drive in drives)
                //     directoryFlow.Add(new DirectoryRow(drive.RootDirectory));
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
                        Spacing = new Vector2(10),
                        Height = 20,
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

                    flow.Add(new DirectoryRow(null, "Computer"));

                    DirectoryInfo traverse = dir.NewValue;

                    while (traverse != null)
                    {
                        flow.Add(new DirectoryRow(traverse));
                        traverse = traverse.Parent;
                    }
                }, true);
            }
        }

        private class ParentDirectoryRow : DirectoryRow
        {
            public override IconUsage Icon => FontAwesome.Solid.Folder;

            public ParentDirectoryRow(DirectoryInfo directory)
                : base(directory, "..")
            {
            }
        }

        private class DirectoryRow : CompositeDrawable
        {
            public const float HEIGHT = 20;

            private readonly DirectoryInfo directory;

            [Resolved]
            private Bindable<DirectoryInfo> currentDirectory { get; set; }

            public DirectoryRow(DirectoryInfo directory, string display = null)
            {
                this.directory = directory;

                AutoSizeAxes = Axes.X;
                Height = HEIGHT;

                AddRangeInternal(new Drawable[]
                {
                    new SpriteIcon
                    {
                        Icon = Icon,
                        Size = new Vector2(HEIGHT)
                    },
                    new OsuSpriteText
                    {
                        X = HEIGHT + 5,
                        Text = display ?? directory.Name,
                        Font = OsuFont.Default.With(size: HEIGHT)
                    }
                });
            }

            protected override bool OnClick(ClickEvent e)
            {
                currentDirectory.Value = directory;
                return true;
            }

            public virtual IconUsage Icon => FontAwesome.Regular.Folder;
        }
    }
}
