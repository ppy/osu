// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.IO;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics.Containers;

namespace osu.Game.Graphics.UserInterfaceV2
{
    public partial class OsuDirectorySelector : DirectorySelector
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

        protected override Drawable CreateHiddenToggleButton() => new OsuDirectorySelectorHiddenToggle { Current = { BindTarget = ShowHiddenItems } };

        protected override DirectorySelectorDirectory CreateParentDirectoryItem(DirectoryInfo directory) => new OsuDirectorySelectorParentDirectory(directory);

        protected override DirectorySelectorDirectory CreateDirectoryItem(DirectoryInfo directory, string displayName = null) => new OsuDirectorySelectorDirectory(directory, displayName);

        protected override void NotifySelectionError() => this.FlashColour(Colour4.Red, 300);
    }
}
