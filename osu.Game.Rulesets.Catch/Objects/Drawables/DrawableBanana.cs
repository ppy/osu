// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Catch.Skinning.Default;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Catch.Objects.Drawables
{
    public class DrawableBanana : DrawablePalpableCatchHitObject
    {
        public DrawableBanana()
            : this(null)
        {
        }

        public DrawableBanana([CanBeNull] Banana h)
            : base(h)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            ScalingContainer.Child = new SkinnableDrawable(
                new CatchSkinComponent(CatchSkinComponents.Banana),
                _ => new BananaPiece());
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // start time affects the random seed which is used to determine the banana colour
            StartTimeBindable.BindValueChanged(_ => UpdateComboColour());
        }

        protected override void UpdateInitialTransforms()
        {
            base.UpdateInitialTransforms();

            const float end_scale = 0.6f;
            const float random_scale_range = 1.6f;

            ScalingContainer.ScaleTo(HitObject.Scale * (end_scale + random_scale_range * RandomSingle(3)))
                            .Then().ScaleTo(HitObject.Scale * end_scale, HitObject.TimePreempt);

            ScalingContainer.RotateTo(getRandomAngle(1))
                            .Then()
                            .RotateTo(getRandomAngle(2), HitObject.TimePreempt);

            float getRandomAngle(int series) => 180 * (RandomSingle(series) * 2 - 1);
        }

        public override void PlaySamples()
        {
            base.PlaySamples();
            if (Samples != null)
                Samples.Frequency.Value = 0.77f + ((Banana)HitObject).BananaIndex * 0.006f;
        }
    }
}
