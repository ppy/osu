// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Graphics;
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

        protected override void AddMissTransforms()
        {
            ScaleTo(1.6f);
            ScaleTo(1, 100, EasingTypes.In);

            MoveToOffset(new Vector2(0, -100), 800, EasingTypes.InQuint);
            RotateTo(40, 800, EasingTypes.InQuint);

            Delay(600);
            FadeOut(200);
        }

        protected override void AddHitTransforms()
        {
            base.AddHitTransforms();

            JudgementText.TransformSpacingTo(new Vector2(14, 0), 1800, EasingTypes.OutQuint);
        }
    }
}