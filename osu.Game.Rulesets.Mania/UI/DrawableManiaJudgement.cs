// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.Mania.UI
{
    public class DrawableManiaJudgement : DrawableJudgement
    {
        public DrawableManiaJudgement(JudgementResult result, DrawableHitObject judgedObject)
            : base(result, judgedObject)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            if (JudgementText != null)
                JudgementText.Font = JudgementText.Font.With(size: 25);
        }

        protected override double FadeInDuration => 50;

        protected override void ApplyHitAnimations()
        {
            JudgementBody.ScaleTo(0.8f);
            JudgementBody.ScaleTo(1, 250, Easing.OutElastic);

            JudgementBody.Delay(FadeInDuration).ScaleTo(0.75f, 250);
            this.Delay(FadeInDuration).FadeOut(200);
        }
    }
}
