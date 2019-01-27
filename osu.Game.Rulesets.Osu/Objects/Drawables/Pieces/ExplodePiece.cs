// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Skinning;
using osuTK;

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

            Child = new SkinnableDrawable("Play/osu/hitcircle-explode", _ => new TrianglesPiece
            {
                Blending = BlendingMode.Additive,
                RelativeSizeAxes = Axes.Both,
                Alpha = 0.2f,
            }, s => s.GetTexture("Play/osu/hitcircle") == null);
        }
    }
}
