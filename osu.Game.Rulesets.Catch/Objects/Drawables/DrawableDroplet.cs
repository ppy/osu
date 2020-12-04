// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Catch.Objects.Drawables.Pieces;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Catch.Objects.Drawables
{
    public class DrawableDroplet : DrawablePalpableCatchHitObject
    {
        public override bool StaysOnPlate => false;

        public DrawableDroplet()
            : this(null)
        {
        }

        public DrawableDroplet([CanBeNull] CatchHitObject h)
            : base(h)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            HyperDash.BindValueChanged(_ => updatePiece(), true);
        }

        private void updatePiece()
        {
            ScaleContainer.Child = new SkinnableDrawable(
                new CatchSkinComponent(CatchSkinComponents.Droplet),
                _ => new DropletPiece
                {
                    HyperDash = { BindTarget = HyperDash }
                });
        }

        protected override void UpdateInitialTransforms()
        {
            base.UpdateInitialTransforms();

            // roughly matches osu-stable
            float startRotation = RandomSingle(1) * 20;
            double duration = HitObject.TimePreempt + 2000;

            ScaleContainer.RotateTo(startRotation).RotateTo(startRotation + 720, duration);
        }
    }
}
