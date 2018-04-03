// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Game.Rulesets.Osu.Judgements;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Osu.Objects.Drawables
{
    public class DrawableSliderTail : DrawableOsuHitObject, IRequireTracking
    {
        /// <summary>
        /// The judgement text is provided by the <see cref="DrawableSlider"/>.
        /// </summary>
        public override bool DisplayJudgement => false;

        public bool Tracking { get; set; }

        public DrawableSliderTail(Slider slider, HitCircle hitCircle)
            : base(hitCircle)
        {
            Origin = Anchor.Centre;

            RelativeSizeAxes = Axes.Both;
            FillMode = FillMode.Fit;

            AlwaysPresent = true;

            Position = HitObject.Position - slider.Position;
        }

        protected override void CheckForJudgements(bool userTriggered, double timeOffset)
        {
            if (!userTriggered && timeOffset >= 0)
                AddJudgement(new OsuSliderTailJudgement { Result = Tracking ? HitResult.Great : HitResult.Miss });
        }
    }
}
