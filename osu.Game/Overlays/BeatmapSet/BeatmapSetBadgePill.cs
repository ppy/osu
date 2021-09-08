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
    public class BeatmapSetBadgePill : CircularContainer
    {
        public BeatmapSetBadgePill()
        {
            AutoSizeAxes = Axes.Both;
            Masking = true;
        }

        [BackgroundDependencyLoader(true)]
        private void load([CanBeNull] OsuColour colours, [CanBeNull] OverlayColourProvider colourProvider)
        {
            Add(new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = colourProvider?.Background5 ?? colours?.Gray2 ?? Color4.DarkGray,
            });
        }
    }
}
