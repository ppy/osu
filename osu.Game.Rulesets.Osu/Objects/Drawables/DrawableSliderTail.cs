// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Scoring;
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

            // TODO: This has no drawable content. Support for skins should be added.
        }

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            if (!userTriggered && timeOffset >= 0)
                ApplyResult(r => r.Type = Tracking ? HitResult.Great : HitResult.Miss);
        }

        private void updatePosition() => Position = HitObject.Position - slider.Position;
    }
}
