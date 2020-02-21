// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Game.Rulesets.Catch.Objects.Drawable.Pieces;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Catch.Objects.Drawable
{
    public class DrawableDroplet : PalpableCatchHitObject<Droplet>
    {
        public override bool StaysOnPlate => false;

        public DrawableDroplet(Droplet h)
            : base(h)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            ScaleContainer.Child = new SkinnableDrawable(new CatchSkinComponent(CatchSkinComponents.Droplet), _ => new Pulp
            {
                Size = Size / 4,
                AccentColour = { BindTarget = AccentColour }
            });
        }
    }
}