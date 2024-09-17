// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Utils;
using osu.Game.Rulesets.Catch.Skinning.Default;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Catch.Objects.Drawables
{
    public partial class DrawableBanana : DrawablePalpableCatchHitObject
    {
        public DrawableBanana()
            : this(null)
        {
        }

        public DrawableBanana(Banana? h)
            : base(h)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            ScalingContainer.Child = new SkinnableDrawable(
                new CatchSkinComponentLookup(CatchSkinComponents.Banana),
                _ => new BananaPiece());
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // start time affects the random seed which is used to determine the banana colour
            StartTimeBindable.BindValueChanged(_ => UpdateComboColour());
        }

        private float startScale;
        private float endScale;

        private float startAngle;
        private float endAngle;

        protected override void UpdateInitialTransforms()
        {
            base.UpdateInitialTransforms();

            // Important to have this in UpdateInitialTransforms() to it is re-triggered by RefreshStateTransforms().
            const float end_scale = 0.6f;
            const float random_scale_range = 1.6f;

            startScale = end_scale + random_scale_range * RandomSingle(3);
            endScale = end_scale;

            startAngle = getRandomAngle(1);
            endAngle = getRandomAngle(2);

            float getRandomAngle(int series) => 180 * (RandomSingle(series) * 2 - 1);
        }

        protected override void Update()
        {
            base.Update();

            double preemptProgress = (Time.Current - (HitObject.StartTime - InitialLifetimeOffset)) / HitObject.TimePreempt;

            // Clamp scale and rotation at the point of bananas being caught, else let them freely extrapolate.
            if (Result.IsHit)
                preemptProgress = Math.Min(1, preemptProgress);

            ScalingContainer.Scale = new Vector2(HitObject.Scale * (float)Interpolation.Lerp(startScale, endScale, preemptProgress));
            ScalingContainer.Rotation = (float)Interpolation.Lerp(startAngle, endAngle, preemptProgress);
        }

        public override void PlaySamples()
        {
            base.PlaySamples();
            if (Samples != null)
                Samples.Frequency.Value = 0.77f + ((Banana)HitObject).BananaIndex * 0.006f;
        }
    }
}
