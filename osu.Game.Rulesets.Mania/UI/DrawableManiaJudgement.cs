// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Mania.Judgements;
using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.Mania.UI
{
    public class DrawableManiaJudgement : DrawableJudgement<ManiaJudgement>
    {
        public DrawableManiaJudgement(ManiaJudgement judgement)
            : base(judgement)
        {
        }

        protected override void LoadComplete()
        {
            switch (Judgement.Result)
            {
                case HitResult.Hit:
                    JudgementText.TransformSpacingTo(new Vector2(14, 0), 1800, EasingTypes.OutQuint);
                    break;
            }

            base.LoadComplete();
        }
    }
}