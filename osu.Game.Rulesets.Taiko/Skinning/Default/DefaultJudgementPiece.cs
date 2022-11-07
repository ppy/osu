// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Taiko.Skinning.Default
{
    public class DefaultJudgementPiece : Rulesets.Judgements.DefaultJudgementPiece
    {
        public DefaultJudgementPiece(HitResult result)
            : base(result)
        {
        }

        public override void PlayAnimation()
        {
            if (Result != HitResult.Miss)
            {
                this
                    .MoveToY(0)
                    .MoveToY(-100, 500);

                JudgementText
                    .ScaleTo(0.9f)
                    .ScaleTo(1, 500, Easing.OutElastic);
            }

            base.PlayAnimation();
        }
    }
}
