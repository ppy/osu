// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using OpenTK;
using osu.Framework.Graphics.Shapes;

namespace osu.Game.Rulesets.Osu.Objects.Drawables.Pieces
{
    public class FlashPiece : Container
    {
        public FlashPiece()
        {
            Size = new Vector2(128);

            Masking = true;
            CornerRadius = Size.X / 2;

            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            Blending = BlendingMode.Additive;
            Alpha = 0;

            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both
                }
            };
        }
    }
}