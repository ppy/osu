// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.IO;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics.Sprites;
using osuTK;

namespace osu.Game.Graphics.UserInterfaceV2
{
    internal partial class OsuDirectorySelectorBreadcrumbDisplay : DirectorySelectorBreadcrumbDisplay
    {
        protected override Drawable CreateCaption() => new OsuSpriteText
        {
            Text = "Current Directory: ",
            Font = OsuFont.Default.With(size: OsuDirectorySelector.ITEM_HEIGHT),
        };

        protected override DirectorySelectorDirectory CreateRootDirectoryItem() => new OsuBreadcrumbDisplayComputer();

        protected override DirectorySelectorDirectory CreateDirectoryItem(DirectoryInfo directory, string displayName = null) => new OsuBreadcrumbDisplayDirectory(directory, displayName);

        public OsuDirectorySelectorBreadcrumbDisplay()
        {
            Padding = new MarginPadding(15);
        }

        private partial class OsuBreadcrumbDisplayComputer : OsuBreadcrumbDisplayDirectory
        {
            protected override IconUsage? Icon => null;

            public OsuBreadcrumbDisplayComputer()
                : base(null, "Computer")
            {
            }
        }

        private partial class OsuBreadcrumbDisplayDirectory : OsuDirectorySelectorDirectory
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
}
