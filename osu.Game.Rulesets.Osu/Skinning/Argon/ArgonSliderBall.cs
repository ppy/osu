// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Skinning.Argon
{
    public class ArgonSliderBall : CircularContainer
    {
        private readonly Box fill;
        private readonly SpriteIcon icon;

        public ArgonSliderBall()
        {
            Size = new Vector2(ArgonMainCirclePiece.OUTER_GRADIENT_SIZE);

            Masking = true;

            BorderThickness = ArgonMainCirclePiece.BORDER_THICKNESS * 2;
            BorderColour = Color4.White;

            InternalChildren = new Drawable[]
            {
                fill = new Box
                {
                    Colour = ColourInfo.GradientVertical(Colour4.FromHex("FC618F"), Colour4.FromHex("BB1A41")),
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
                icon = new SpriteIcon
                {
                    Size = new Vector2(48),
                    Scale = new Vector2(0.6f, 0.8f),
                    Icon = FontAwesome.Solid.AngleRight,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                }
            };
        }

        protected override void Update()
        {
            base.Update();

            fill.Rotation = -Rotation;
        }
    }
}
