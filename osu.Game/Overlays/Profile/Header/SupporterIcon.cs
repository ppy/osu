// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Backgrounds;
using osuTK;

namespace osu.Game.Overlays.Profile.Header
{
    public class SupporterIcon : CircularContainer, IHasTooltip
    {
        private readonly Box background;

        public string TooltipText => "osu!supporter";

        public SupporterIcon()
        {
            Masking = true;
            Children = new Drawable[]
            {
                new Box { RelativeSizeAxes = Axes.Both },
                new CircularContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Scale = new Vector2(0.8f),
                    Masking = true,
                    Children = new Drawable[]
                    {
                        background = new Box { RelativeSizeAxes = Axes.Both },
                        new Triangles
                        {
                            TriangleScale = 0.2f,
                            ColourLight = OsuColour.FromHex(@"ff7db7"),
                            ColourDark = OsuColour.FromHex(@"de5b95"),
                            RelativeSizeAxes = Axes.Both,
                            Velocity = 0.3f,
                        },
                    }
                },
                new SpriteIcon
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Icon = FontAwesome.Solid.Heart,
                    Scale = new Vector2(0.45f),
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            background.Colour = colours.Pink;
        }
    }
}
