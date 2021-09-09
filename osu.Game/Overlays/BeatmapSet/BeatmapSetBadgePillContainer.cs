// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osuTK.Graphics;

namespace osu.Game.Overlays.BeatmapSet
{
    public class BeatmapSetBadgePillContainer : CircularContainer
    {
        protected override Container<Drawable> Content => contentContainer;

        private readonly Box background;
        private readonly Container contentContainer;

        public BeatmapSetBadgePillContainer()
        {
            Masking = true;
            AutoSizeAxes = Axes.Both;
            InternalChildren = new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                },
                contentContainer = new Container
                {
                    AutoSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Horizontal = 10f, Vertical = 2f },
                }
            };
        }

        [BackgroundDependencyLoader(true)]
        private void load([CanBeNull] OsuColour colours, [CanBeNull] OverlayColourProvider colourProvider)
        {
            background.Colour = colourProvider?.Background5 ?? colours?.Gray2 ?? Color4.DarkGray;
        }
    }
}
