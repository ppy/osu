// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Utils;
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

        private float startRotation;

        protected override void UpdateInitialTransforms()
        {
            base.UpdateInitialTransforms();

            // Important to have this in UpdateInitialTransforms() to it is re-triggered by RefreshStateTransforms().
            startRotation = RandomSingle(1) * 20;
        }

        protected override void Update()
        {
            base.Update();

            // No clamping for droplets. They should be considered indefinitely spinning regardless of time.
            // They also never end up on the plate, so they shouldn't stop spinning when caught.
            double preemptProgress = (Time.Current - (HitObject.StartTime - InitialLifetimeOffset)) / (HitObject.TimePreempt + 2000);
            ScalingContainer.Rotation = (float)Interpolation.Lerp(startRotation, startRotation + 720, preemptProgress);
        }
    }
}
