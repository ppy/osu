// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using OpenTK;

namespace osu.Game.Rulesets.Osu.Objects.Drawables.Pieces
{
    public class ExplodePiece : Container
    {
        public ExplodePiece()
        {
            Size = new Vector2(128);

            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            Blending = BlendingMode.Additive;
            Alpha = 0;

            Children = new Drawable[]
            {
                new TrianglesPiece
                {
                    Blending = BlendingMode.Additive,
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0.2f,
                }
            };
        }
    }
}