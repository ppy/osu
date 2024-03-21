// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Taiko.Edit.Blueprints
{
    public partial class HitPiece : CompositeDrawable
    {
        public HitPiece()
        {
            Origin = Anchor.Centre;

            InternalChild = new CircularContainer
            {
                Masking = true,
                BorderThickness = 10,
                BorderColour = Color4.Yellow,
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
