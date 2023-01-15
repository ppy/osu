// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Catch.Skinning.Default;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Catch.Objects.Drawables
{
    public partial class DrawableDroplet : DrawablePalpableCatchHitObject
    {
        public DrawableDroplet()
            : this(null)
        {
        }

        public DrawableDroplet(CatchHitObject? h)
            : base(h)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            ScalingContainer.Child = new SkinnableDrawable(
                new CatchSkinComponentLookup(CatchSkinComponents.Droplet),
                _ => new DropletPiece());
        }

        protected override void UpdateInitialTransforms()
        {
            base.UpdateInitialTransforms();

            // roughly matches osu-stable
            float startRotation = RandomSingle(1) * 20;
            double duration = HitObject.TimePreempt + 2000;

            ScalingContainer.RotateTo(startRotation).RotateTo(startRotation + 720, duration);
        }
    }
}
