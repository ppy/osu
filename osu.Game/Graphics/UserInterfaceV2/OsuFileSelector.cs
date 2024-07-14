// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.IO;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Graphics.UserInterfaceV2
{
    public partial class OsuFileSelector : FileSelector
    {
        public OsuFileSelector(string initialPath = null, string[] validFileExtensions = null)
            : base(initialPath, validFileExtensions)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Padding = new MarginPadding(10);
        }

        protected override ScrollContainer<Drawable> CreateScrollContainer() => new OsuScrollContainer();

        protected override DirectorySelectorBreadcrumbDisplay CreateBreadcrumb() => new OsuDirectorySelectorBreadcrumbDisplay();

        protected override Drawable CreateHiddenToggleButton() => new OsuDirectorySelectorHiddenToggle { Current = { BindTarget = ShowHiddenItems } };

        protected override DirectorySelectorDirectory CreateParentDirectoryItem(DirectoryInfo directory) => new OsuDirectorySelectorParentDirectory(directory);

        protected override DirectorySelectorDirectory CreateDirectoryItem(DirectoryInfo directory, string displayName = null) => new OsuDirectorySelectorDirectory(directory, displayName);

        protected override DirectoryListingFile CreateFileItem(FileInfo file) => new OsuDirectoryListingFile(file);

        protected override void NotifySelectionError() => this.FlashColour(Colour4.Red, 300);

        protected partial class OsuDirectoryListingFile : DirectoryListingFile
        {
            public OsuDirectoryListingFile(FileInfo file)
                : base(file)
            {
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                Flow.AutoSizeAxes = Axes.X;
                Flow.Height = OsuDirectorySelector.ITEM_HEIGHT;

                AddRangeInternal(new Drawable[]
                {
                    new OsuDirectorySelectorDirectory.Background
                    {
                        Depth = 1
                    },
                    new HoverClickSounds()
                });
            }

            protected override IconUsage? Icon
            {
                get
                {
                    if (OsuGameBase.VIDEO_EXTENSIONS.Contains(File.Extension.ToLowerInvariant()))
                        return FontAwesome.Regular.FileVideo;

                    switch (File.Extension)
                    {
                        case @".ogg":
                        case @".mp3":
                        case @".wav":
                            return FontAwesome.Regular.FileAudio;

                        case @".jpg":
                        case @".jpeg":
                        case @".png":
                            return FontAwesome.Regular.FileImage;

                        default:
                            return FontAwesome.Regular.File;
                    }
                }
            }

            protected override SpriteText CreateSpriteText() => new OsuSpriteText();
        }
    }
}
