// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Game.Modes.Objects.Drawables;
using osu.Game.Modes.Osu.Judgements;
using OpenTK;
using osu.Game.Modes.Judgements;

namespace osu.Game.Modes.Osu.Objects.Drawables
{
    public class DrawableOsuJudgement : DrawableJudgement<OsuJudgement>
    {
        public DrawableOsuJudgement(OsuJudgement judgement) : base(judgement)
        {
        }

        protected override void LoadComplete()
        {
            if (Judgement.Result != HitResult.Miss)
                JudgementText.TransformSpacingTo(new Vector2(14, 0), 1800, EasingTypes.OutQuint);

            base.LoadComplete();
        }
    }
}