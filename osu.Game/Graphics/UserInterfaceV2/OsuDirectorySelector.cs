// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.IO;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osuTK;

namespace osu.Game.Graphics.UserInterfaceV2
{
    public class OsuDirectorySelector : DirectorySelector
    {
        public const float ITEM_HEIGHT = 20;

        public OsuDirectorySelector(string initialPath = null)
            : base(initialPath)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Padding = new MarginPadding(10);
        }

        protected override ScrollContainer<Drawable> CreateScrollContainer() => new OsuScrollContainer();

        protected override DirectorySelectorBreadcrumbDisplay CreateBreadcrumb() => new OsuDirectorySelectorBreadcrumbDisplay();

        protected override DirectorySelectorDirectory CreateParentDirectoryItem(DirectoryInfo directory) => new OsuDirectorySelectorParentDirectory(directory);

        protected override DirectorySelectorDirectory CreateDirectoryItem(DirectoryInfo directory, string displayName = null) => new OsuDirectorySelectorDirectory(directory, displayName);

        protected override void NotifySelectionError() => this.FlashColour(Colour4.Red, 300);

        internal class OsuDirectorySelectorBreadcrumbDisplay : DirectorySelectorBreadcrumbDisplay
        {
            protected override DirectorySelectorDirectory CreateRootDirectoryItem() => new OsuBreadcrumbDisplayComputer();
            protected override DirectorySelectorDirectory CreateDirectoryItem(DirectoryInfo directory, string displayName = null) => new OsuBreadcrumbDisplayDirectory(directory, displayName);

            [BackgroundDependencyLoader]
            private void load()
            {
                Height = 50;
            }

            private class OsuBreadcrumbDisplayComputer : OsuBreadcrumbDisplayDirectory
            {
                protected override IconUsage? Icon => null;

                public OsuBreadcrumbDisplayComputer()
                    : base(null, "Computer")
                {
                }
            }

            private class OsuBreadcrumbDisplayDirectory : OsuDirectorySelectorDirectory
            {
                public OsuBreadcrumbDisplayDirectory(DirectoryInfo directory, string displayName = null)
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

        internal class OsuDirectorySelectorParentDirectory : OsuDirectorySelectorDirectory
        {
            protected override IconUsage? Icon => FontAwesome.Solid.Folder;

            public OsuDirectorySelectorParentDirectory(DirectoryInfo directory)
                : base(directory, "..")
            {
            }
        }

        internal class OsuDirectorySelectorDirectory : DirectorySelectorDirectory
        {
            public OsuDirectorySelectorDirectory(DirectoryInfo directory, string displayName = null)
                : base(directory, displayName)
            {
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                Flow.AutoSizeAxes = Axes.X;
                Flow.Height = ITEM_HEIGHT;

                AddInternal(new OsuDirectorySelectorItemBackground
                {
                    Depth = 1
                });
            }

            protected override SpriteText CreateSpriteText() => new OsuSpriteText();

            protected override IconUsage? Icon => Directory.Name.Contains(Path.DirectorySeparatorChar)
                ? FontAwesome.Solid.Database
                : FontAwesome.Regular.Folder;
        }

        internal class OsuDirectorySelectorItemBackground : CompositeDrawable
        {
            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                RelativeSizeAxes = Axes.Both;

                Masking = true;
                CornerRadius = 5;

                InternalChild = new Box
                {
                    Colour = colours.GreySeafoamDarker,
                    RelativeSizeAxes = Axes.Both,
                };
            }
        }
    }
}
