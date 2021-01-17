// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Overlays.BeatmapSet
{
    public class ExplicitContentBeatmapPill : CompositeDrawable
    {
        public ExplicitContentBeatmapPill()
        {
            AutoSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader(true)]
        private void load(OsuColour colours, OverlayColourProvider colourProvider)
        {
            InternalChild = new CircularContainer
            {
                Masking = true,
                AutoSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = colourProvider?.Background5 ?? colours.Gray2,
                    },
                    new OsuSpriteText
                    {
                        Margin = new MarginPadding { Horizontal = 10f, Vertical = 2f },
                        Text = "EXPLICIT",
                        Font = OsuFont.GetFont(size: 10, weight: FontWeight.SemiBold),
                        Colour = OverlayColourProvider.Orange.Colour2,
                    }
                }
            };
        }
    }
}
