// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE


using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using OpenTK;

namespace osu.Game.Modes.Taiko.Objects.Drawables.Pieces.Ring
{
    public class CentreHitRingPiece : RingPiece
    {
        protected override Drawable CreateInnerPiece()
        {
            return new CircularContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,

                Size = new Vector2(45f),

                Children = new[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,

                        Alpha = 1
                    }
                }
            };
        }
    }
}
