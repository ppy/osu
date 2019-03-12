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

        protected override float InitialHitScale => 0.8f;

        protected override double HitFadeOutDuration => 200;

        protected override float HitScaleDuration => 250;

        protected override void LoadComplete()
        {
            if (Result.IsHit)
                JudgementBody.Delay(FadeInDuration).ScaleTo(0.75f, HitScaleDuration);

            base.LoadComplete();
        }
    }
}
