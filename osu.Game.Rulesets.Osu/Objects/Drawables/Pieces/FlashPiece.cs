// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;
using osu.Framework.Graphics.Shapes;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Osu.Objects.Drawables.Pieces
{
    public class FlashPiece : Container
    {
        public FlashPiece()
        {
            Size = new Vector2(128);

            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            Blending = BlendingMode.Additive;
            Alpha = 0;

            Child = new SkinnableDrawable("Play/osu/hitcircle-flash", name => new CircularContainer
            {
                Masking = true,
                RelativeSizeAxes = Axes.Both,
                Child = new Box
                {
                    RelativeSizeAxes = Axes.Both
                }
            }, s => s.GetTexture("Play/osu/hitcircle") == null);
        }
    }
}
