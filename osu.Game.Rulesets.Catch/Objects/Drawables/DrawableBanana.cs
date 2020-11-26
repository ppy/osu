// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Utils;

namespace osu.Game.Rulesets.Catch.Objects.Drawables
{
    public class DrawableBanana : DrawableFruit
    {
        public DrawableBanana(Banana h)
            : base(h)
        {
        }

        protected override void UpdateInitialTransforms()
        {
            base.UpdateInitialTransforms();

            const float end_scale = 0.6f;
            const float random_scale_range = 1.6f;

            ScaleContainer.ScaleTo(HitObject.Scale * (end_scale + random_scale_range * RNG.NextSingle()))
                          .Then().ScaleTo(HitObject.Scale * end_scale, HitObject.TimePreempt);

            ScaleContainer.RotateTo(getRandomAngle())
                          .Then()
                          .RotateTo(getRandomAngle(), HitObject.TimePreempt);

            float getRandomAngle() => 180 * (RNG.NextSingle() * 2 - 1);
        }

        public override void PlaySamples()
        {
            base.PlaySamples();
            if (Samples != null)
                Samples.Frequency.Value = 0.77f + ((Banana)HitObject).BananaIndex * 0.006f;
        }
    }
}
