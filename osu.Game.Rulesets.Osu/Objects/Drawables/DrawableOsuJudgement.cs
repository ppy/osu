// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Game.Rulesets.Osu.Judgements;
using OpenTK;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Osu.Objects.Drawables
{
    public class DrawableOsuJudgement : DrawableJudgement
    {
        public DrawableOsuJudgement(OsuJudgement judgement)
            : base(judgement)
        {
        }

        protected override void LoadComplete()
        {
            if (Judgement.Result != HitResult.Miss)
                JudgementText.TransformSpacingTo(new Vector2(14, 0), 1800, Easing.OutQuint);

            base.LoadComplete();
        }
    }
}
