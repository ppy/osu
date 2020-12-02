// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using JetBrains.Annotations;
using osu.Framework.Graphics;

namespace osu.Game.Rulesets.Catch.Objects.Drawables
{
    public class DrawableBanana : DrawableFruit
    {
        protected override FruitVisualRepresentation GetVisualRepresentation(int indexInBeatmap) => FruitVisualRepresentation.Banana;

        public DrawableBanana()
            : this(null)
        {
        }

        public DrawableBanana([CanBeNull] Banana h)
            : base(h)
        {
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

            ScaleContainer.ScaleTo(HitObject.Scale * (end_scale + random_scale_range * RandomSingle(3)))
                          .Then().ScaleTo(HitObject.Scale * end_scale, HitObject.TimePreempt);

            ScaleContainer.RotateTo(getRandomAngle(1))
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
