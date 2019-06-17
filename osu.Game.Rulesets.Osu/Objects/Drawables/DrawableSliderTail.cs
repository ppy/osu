// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.MathUtils;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Play;
using osuTK;

namespace osu.Game.Rulesets.Osu.Objects.Drawables
{
    public class DrawableSliderTail : DrawableOsuHitObject, IRequireTracking
    {
        private readonly Slider slider;

        /// <summary>
        /// The judgement text is provided by the <see cref="DrawableSlider"/>.
        /// </summary>
        public override bool DisplayResult => false;

        public bool Tracking { get; set; }

        private readonly IBindable<Vector2> positionBindable = new Bindable<Vector2>();
        private readonly IBindable<SliderPath> pathBindable = new Bindable<SliderPath>();

        public DrawableSliderTail(Slider slider, SliderTailCircle hitCircle)
            : base(hitCircle)
        {
            this.slider = slider;

            Origin = Anchor.Centre;

            RelativeSizeAxes = Axes.Both;
            FillMode = FillMode.Fit;

            AlwaysPresent = true;

            positionBindable.BindTo(hitCircle.PositionBindable);
            pathBindable.BindTo(slider.PathBindable);

            positionBindable.BindValueChanged(_ => updatePosition());
            pathBindable.BindValueChanged(_ => updatePosition(), true);
        }

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            if (!userTriggered && timeOffset >= 0)
                ApplyResult(r => r.Type = Tracking ? HitResult.Great : HitResult.Miss);
        }

        private void updatePosition()
        {
            if (!HasFailed.Value)
                Position = HitObject.Position - slider.Position;
        }

        public override void Fail()
        {
            if (HasFailed.Value)
                return;

            this.RotateTo(RNG.NextSingle(-90, 90), FailAnimation.FAIL_DURATION);
            this.ScaleTo(Scale * 0.5f, FailAnimation.FAIL_DURATION);
            this.MoveToOffset(new Vector2(0, 400), FailAnimation.FAIL_DURATION);

            base.Fail();
        }
    }
}
