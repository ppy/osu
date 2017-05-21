// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Rulesets.Osu.Objects.Drawables.Pieces
{
    public class RingPiece : Container
    {
        public RingPiece()
        {
            Size = new Vector2(128);

            Masking = true;
            CornerRadius = Size.X / 2;

            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            BorderThickness = 10;
            BorderColour = Color4.White;

            Children = new Drawable[]
            {
                new Box
                {
                    AlwaysPresent = true,
                    Alpha = 0,
                    RelativeSizeAxes = Axes.Both
                }
            };
        }
    }
}