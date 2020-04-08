// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Utils;
using osu.Game.Rulesets.Catch.Objects.Drawables.Pieces;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Catch.Objects.Drawables
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

        protected override void UpdateInitialTransforms()
        {
            base.UpdateInitialTransforms();

            // roughly matches osu-stable
            float startRotation = RNG.NextSingle() * 20;
            double duration = HitObject.TimePreempt + 2000;

            ScaleContainer.RotateTo(startRotation).RotateTo(startRotation + 720, duration);
        }
    }
}
