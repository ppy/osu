// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Taiko.Edit.Blueprints
{
    public partial class LengthPiece : CompositeDrawable
    {
        public LengthPiece()
        {
            Origin = Anchor.CentreLeft;

            InternalChild = new Container
            {
                Masking = true,
                Colour = Color4.Yellow,
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = 8,
                    },
                    new Box
                    {
                        Origin = Anchor.BottomLeft,
                        Anchor = Anchor.BottomLeft,
                        RelativeSizeAxes = Axes.X,
                        Height = 8,
                    }
                }
            };
        }
    }
}
