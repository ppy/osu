// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
using osuTK;

namespace osu.Game.Graphics.UserInterfaceV2.FileSelection
{
    internal partial class OsuDirectorySelectorBreadcrumbDisplay : DirectorySelectorBreadcrumbDisplay
    {
        public const float HEIGHT = 45;
        public const float HORIZONTAL_PADDING = 20;

        protected override Drawable CreateCaption() => Empty().With(d =>
        {
            d.Origin = Anchor.CentreLeft;
            d.Anchor = Anchor.CentreLeft;
            d.Alpha = 0;
        });

        protected override DirectorySelectorDirectory CreateRootDirectoryItem() => new OsuBreadcrumbDisplayComputer();

        protected override DirectorySelectorDirectory CreateDirectoryItem(DirectoryInfo directory, string? displayName = null) => new OsuBreadcrumbDisplayDirectory(directory, displayName);

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            ((FillFlowContainer)InternalChild).Padding = new MarginPadding
            {
                Horizontal = HORIZONTAL_PADDING,
                Vertical = 10,
            };

            AddInternal(new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = colourProvider.Background4,
                Depth = 1,
            });
        }

        private partial class OsuBreadcrumbDisplayComputer : OsuBreadcrumbDisplayDirectory
        {
            protected override IconUsage? Icon => null;

            public OsuBreadcrumbDisplayComputer()
                : base(null, "Computer")
            {
            }
        }

        private partial class OsuBreadcrumbDisplayDirectory : DirectorySelectorDirectory
        {
            public OsuBreadcrumbDisplayDirectory(DirectoryInfo? directory, string? displayName = null)
                : base(directory, displayName)
            {
            }

            [Resolved]
            private OverlayColourProvider colourProvider { get; set; } = null!;

            [BackgroundDependencyLoader]
            private void load()
            {
                Anchor = Anchor.CentreLeft;
                Origin = Anchor.CentreLeft;

                Flow.AutoSizeAxes = Axes.X;
                Flow.Height = 25;
                Flow.Margin = new MarginPadding { Horizontal = 10, };

                AddRangeInternal(new Drawable[]
                {
                    new BackgroundLayer(0.5f)
                    {
                        Depth = 1
                    },
                    new HoverClickSounds(),
                });

                Flow.Add(new SpriteIcon
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Icon = FontAwesome.Solid.ChevronRight,
                    Size = new Vector2(FONT_SIZE / 2),
                    Margin = new MarginPadding { Left = 5, },
                });
                Flow.Colour = colourProvider.Light3;
            }

            protected override SpriteText CreateSpriteText() => new OsuSpriteText().With(t => t.Font = OsuFont.Default.With(weight: FontWeight.SemiBold));

            protected override IconUsage? Icon => Directory.Name.Contains(Path.DirectorySeparatorChar) ? FontAwesome.Solid.Database : null;
        }
    }
}
