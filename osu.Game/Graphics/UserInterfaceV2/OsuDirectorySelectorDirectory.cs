// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.IO;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;

namespace osu.Game.Graphics.UserInterfaceV2
{
    internal partial class OsuDirectorySelectorDirectory : DirectorySelectorDirectory
    {
        public OsuDirectorySelectorDirectory(DirectoryInfo directory, string displayName = null)
            : base(directory, displayName)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Flow.AutoSizeAxes = Axes.X;
            Flow.Height = OsuDirectorySelector.ITEM_HEIGHT;

            AddRangeInternal(new Drawable[]
            {
                new Background
                {
                    Depth = 1
                },
                new HoverClickSounds()
            });
        }

        protected override SpriteText CreateSpriteText() => new OsuSpriteText();

        protected override IconUsage? Icon => Directory.Name.Contains(Path.DirectorySeparatorChar)
            ? FontAwesome.Solid.Database
            : FontAwesome.Regular.Folder;

        internal partial class Background : CompositeDrawable
        {
            [BackgroundDependencyLoader(true)]
            private void load(OverlayColourProvider overlayColourProvider, OsuColour colours)
            {
                RelativeSizeAxes = Axes.Both;

                Masking = true;
                CornerRadius = 5;

                InternalChild = new Box
                {
                    Colour = overlayColourProvider?.Background5 ?? colours.GreySeaFoamDarker,
                    RelativeSizeAxes = Axes.Both,
                };
            }
        }
    }
}
