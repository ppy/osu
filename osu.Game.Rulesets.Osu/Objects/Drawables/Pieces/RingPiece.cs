// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;
using osuTK.Graphics;
using osu.Framework.Graphics.Shapes;

namespace osu.Game.Rulesets.Osu.Objects.Drawables.Pieces
{
    public class RingPiece : Container
    {
        public RingPiece()
        {
            Size = new Vector2(OsuHitObject.OBJECT_RADIUS * 2);

            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            InternalChild = new Container
            {
                Masking = true,
                CornerRadius = Size.X / 2,
                BorderThickness = 10,
                BorderColour = Color4.White,
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    new Box
                    {
                        AlwaysPresent = true,
                        Alpha = 0,
                        RelativeSizeAxes = Axes.Both
                    }
                }
            };
        }
    }
}
