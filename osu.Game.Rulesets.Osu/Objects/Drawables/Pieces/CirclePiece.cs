// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Osu.Objects.Drawables.Pieces
{
    public class CirclePiece : CompositeDrawable
    {
        public CirclePiece()
        {
            Size = new Vector2(OsuHitObject.OBJECT_RADIUS * 2);
            Masking = true;
            CornerRadius = Size.X / 2;

            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            InternalChild = new SkinnableDrawable("Play/osu/hitcircle", _ => new DefaultCirclePiece());
        }
    }
}
