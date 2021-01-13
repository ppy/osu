// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.BeatmapSet
{
    public class ExplicitBeatmapPill : CompositeDrawable
    {
        public ExplicitBeatmapPill()
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
                        Font = OsuFont.GetFont(size: 10, weight: FontWeight.Bold),
                        // todo: this is --hsl-orange-2 from the new palette in https://github.com/ppy/osu-web/blob/8ceb46f/resources/assets/less/colors.less#L128-L151,
                        // should probably take the whole palette from there onto OsuColour for a nicer look in code.
                        Colour = Color4.FromHsl(new Vector4(45f / 360, 0.8f, 0.6f, 1f)),
                    }
                }
            };
        }
    }
}
