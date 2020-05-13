// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Screens;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Screens;

namespace osu.Game.Overlays.Settings.Sections.Maintenance
{
    public class MigrationSelectScreen : OsuScreen
    {
        private DirectorySelector directorySelector;

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = new GridContainer
            {
                RelativeSizeAxes = Axes.Both,
                RowDimensions = new[]
                {
                    new Dimension(GridSizeMode.Relative, 0.8f),
                    new Dimension(),
                },
                Content = new[]
                {
                    new Drawable[] { directorySelector = new DirectorySelector { RelativeSizeAxes = Axes.Both } },
                    new Drawable[]
                    {
                        new OsuButton
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Width = 300,
                            Text = "Start",
                            Action = start
                        },
                    }
                }
            };
        }

        private void start()
        {
            var target = directorySelector.CurrentDirectory.Value;
            if (target.GetDirectories().Length > 0 || target.GetFiles().Length > 0)
                target = target.CreateSubdirectory("osu-lazer");

            ValidForResume = false;
            this.Push(new MigrationRunScreen(target));
        }
    }
}
