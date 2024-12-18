// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osuTK;

namespace osu.Game.Rulesets.Osu.Skinning.Argon
{
    public partial class ArgonFollowPoint : CompositeDrawable
    {
        public ArgonFollowPoint()
        {
            Blending = BlendingParameters.Additive;

            Colour = ColourInfo.GradientVertical(Colour4.FromHex("FC618F"), Colour4.FromHex("BB1A41"));
            AutoSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                new SpriteIcon
                {
                    Icon = FontAwesome.Solid.ChevronRight,
                    Size = new Vector2(8),
                    Colour = OsuColour.Gray(0.2f),
                },
                new SpriteIcon
                {
                    Icon = FontAwesome.Solid.ChevronRight,
                    Size = new Vector2(8),
                    X = 4,
                },
            };
        }
    }
}
