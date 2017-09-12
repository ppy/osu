// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Game.Rulesets.Judgements;

namespace osu.Game.Rulesets.Mania.UI
{
    internal class DrawableManiaJudgement : DrawableJudgement
    {
        public DrawableManiaJudgement(Judgement judgement)
        : base(judgement)
        {
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
