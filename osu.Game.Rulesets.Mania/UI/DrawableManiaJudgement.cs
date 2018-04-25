// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.Mania.UI
{
    internal class DrawableManiaJudgement : DrawableJudgement
    {
        public DrawableManiaJudgement(Judgement judgement, DrawableHitObject judgedObject)
            : base(judgement, judgedObject)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            if (JudgementText != null)
                JudgementText.TextSize = 25;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            this.FadeInFromZero(50, Easing.OutQuint);

            if (Judgement.IsHit)
            {
                this.ScaleTo(0.8f);
                this.ScaleTo(1, 250, Easing.OutElastic);

                this.Delay(50).FadeOut(200).ScaleTo(0.75f, 250);
            }

            Expire();
        }
    }
}
