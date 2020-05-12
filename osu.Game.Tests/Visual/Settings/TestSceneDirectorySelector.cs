// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.IO;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Platform;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
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
        private Storage root;
        private FillFlowContainer directoryFlow;
        private CurrentDirectoryDisplay current;

        [Resolved]
        private GameHost host { get; set; }

        private readonly Bindable<string> currentDirectory = new Bindable<string>();

        public DirectorySelector(Storage root = null)
        {
            this.root = root;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Padding = new MarginPadding(10);

            if (root == null)
                root = host.GetStorage("/Users/");

            InternalChildren = new Drawable[]
            {
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        current = new CurrentDirectoryDisplay
                        {
                            CurrentDirectory = { BindTarget = currentDirectory },
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

            currentDirectory.BindValueChanged(updateDisplay);
            currentDirectory.Value = root.GetFullPath(string.Empty);
        }

        private void updateDisplay(ValueChangedEvent<string> directory)
        {
            root = host.GetStorage(directory.NewValue);

            directoryFlow.Clear();

            directoryFlow.Add(new ParentDirectoryRow(getParentPath()) { CurrentDirectory = { BindTarget = currentDirectory }, });

            foreach (var dir in root.GetDirectories(string.Empty))
                directoryFlow.Add(new DirectoryRow(dir, root.GetFullPath(dir)) { CurrentDirectory = { BindTarget = currentDirectory }, });
        }

        private string getParentPath() => Path.GetFullPath(Path.Combine(root.GetFullPath(string.Empty), ".."));

        public class CurrentDirectoryDisplay : CompositeDrawable
        {
            public readonly Bindable<string> CurrentDirectory = new Bindable<string>();

            public CurrentDirectoryDisplay()
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

                CurrentDirectory.BindValueChanged(dir =>
                {
                    flow.Clear();

                    flow.Add(new OsuSpriteText { Text = "Current Directory: " });

                    var pieces = dir.NewValue.Split(Path.DirectorySeparatorChar);

                    pieces[0] = "/";

                    for (int i = 0; i < pieces.Length; i++)
                    {
                        flow.Add(new DirectoryRow(pieces[i], Path.Combine(pieces.Take(i + 1).ToArray()))
                        {
                            CurrentDirectory = { BindTarget = CurrentDirectory },
                            RelativeSizeAxes = Axes.Y,
                            Size = new Vector2(100, 1)
                        });
                    }
                });
            }
        }

        private class ParentDirectoryRow : DirectoryRow
        {
            public override IconUsage Icon => FontAwesome.Solid.Folder;

            public ParentDirectoryRow(string fullPath)
                : base("..", fullPath)
            {
            }
        }

        private class DirectoryRow : OsuButton
        {
            private readonly string fullPath;

            public readonly Bindable<string> CurrentDirectory = new Bindable<string>();

            public DirectoryRow(string display, string fullPath)
            {
                this.fullPath = fullPath;

                RelativeSizeAxes = Axes.X;
                Height = 20;

                BackgroundColour = OsuColour.Gray(0.1f);

                AddRange(new Drawable[]
                {
                    new SpriteIcon
                    {
                        Icon = Icon,
                        Size = new Vector2(20)
                    },
                    new OsuSpriteText
                    {
                        X = 25,
                        Text = display,
                        Font = OsuFont.Default.With(size: 20)
                    }
                });

                Action = PerformDirectoryTraversal;
            }

            protected virtual void PerformDirectoryTraversal()
            {
                CurrentDirectory.Value = fullPath;
            }

            public virtual IconUsage Icon => FontAwesome.Regular.Folder;
        }
    }
}
