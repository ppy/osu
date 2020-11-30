// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Objects.Drawables;
using osuTK;

namespace osu.Game.Rulesets.Catch.Objects.Drawables.Pieces
{
    public class DropletPiece : CompositeDrawable
    {
        public readonly Bindable<bool> HyperDash = new Bindable<bool>();

        public DropletPiece()
        {
            Size = new Vector2(CatchHitObject.OBJECT_RADIUS / 2);
        }

        [BackgroundDependencyLoader]
        private void load(DrawableHitObject drawableObject)
        {
            InternalChild = new Pulp
            {
                RelativeSizeAxes = Axes.Both,
                AccentColour = { BindTarget = drawableObject.AccentColour }
            };

            if (HyperDash.Value)
            {
                AddInternal(new HyperDropletBorderPiece());
            }
        }
    }
}
