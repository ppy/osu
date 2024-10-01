// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.IO;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterfaceV2.FileSelection;
using osu.Game.Overlays;

namespace osu.Game.Graphics.UserInterfaceV2
{
    public partial class OsuDirectorySelector : DirectorySelector
    {
        public const float ITEM_HEIGHT = 16;

        private Box hiddenToggleBackground = null!;

        public OsuDirectorySelector(string? initialPath = null)
            : base(initialPath)
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

        protected override void NotifySelectionError() => this.FlashColour(Colour4.Red, 300);
    }
}
