// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.IO;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterfaceV2.FileSelection;
using osu.Game.Overlays;
using osu.Game.Utils;

namespace osu.Game.Graphics.UserInterfaceV2
{
    public partial class OsuFileSelector : FileSelector
    {
        private Box hiddenToggleBackground = null!;

        public OsuFileSelector(string? initialPath = null, string[]? validFileExtensions = null)
            : base(initialPath, validFileExtensions)
        {
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            AddInternal(new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = colourProvider.Background5,
                Depth = float.MaxValue,
            });

            hiddenToggleBackground.Colour = colourProvider.Background4;
        }

        protected override ScrollContainer<Drawable> CreateScrollContainer() => new OsuScrollContainer
        {
            Padding = new MarginPadding
            {
                Horizontal = 20,
                Vertical = 15,
            }
        };

        protected override DirectorySelectorBreadcrumbDisplay CreateBreadcrumb() => new OsuDirectorySelectorBreadcrumbDisplay();

        protected override Drawable CreateHiddenToggleButton() => new Container
        {
            RelativeSizeAxes = Axes.Y,
            AutoSizeAxes = Axes.X,
            Children = new Drawable[]
            {
                hiddenToggleBackground = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                },
                new HiddenFilesToggleCheckbox
                {
                    Current = { BindTarget = ShowHiddenItems },
                },
            }
        };

        protected override DirectorySelectorDirectory CreateParentDirectoryItem(DirectoryInfo directory) => new OsuDirectorySelectorParentDirectory(directory);

        protected override DirectorySelectorDirectory CreateDirectoryItem(DirectoryInfo directory, string? displayName = null) => new OsuDirectorySelectorDirectory(directory, displayName);

        protected override DirectoryListingFile CreateFileItem(FileInfo file) => new OsuDirectoryListingFile(file);

        protected override void NotifySelectionError() => this.FlashColour(Colour4.Red, 300);

        protected partial class OsuDirectoryListingFile : DirectoryListingFile
        {
            public OsuDirectoryListingFile(FileInfo file)
                : base(file)
            {
            }

            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider)
            {
                Flow.AutoSizeAxes = Axes.X;
                Flow.Height = OsuDirectorySelector.ITEM_HEIGHT;

                AddInternal(new BackgroundLayer());

                Colour = colourProvider.Light3;
            }

            protected override IconUsage? Icon
            {
                get
                {
                    string extension = File.Extension.ToLowerInvariant();

                    if (SupportedExtensions.VIDEO_EXTENSIONS.Contains(extension))
                        return FontAwesome.Regular.FileVideo;

                    if (SupportedExtensions.AUDIO_EXTENSIONS.Contains(extension))
                        return FontAwesome.Regular.FileAudio;

                    if (SupportedExtensions.IMAGE_EXTENSIONS.Contains(extension))
                        return FontAwesome.Regular.FileImage;

                    return FontAwesome.Regular.File;
                }
            }

            protected override SpriteText CreateSpriteText() => new OsuSpriteText().With(t => t.Font = OsuFont.Default.With(weight: FontWeight.SemiBold));
        }
    }
}
