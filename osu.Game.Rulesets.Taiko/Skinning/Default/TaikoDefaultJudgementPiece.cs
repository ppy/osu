// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Taiko.Skinning.Default
{
    public partial class TaikoDefaultJudgementPiece : Rulesets.Judgements.DefaultJudgementPiece
    {
        public TaikoDefaultJudgementPiece(HitResult result)
            : base(result)
        {
            RelativePositionAxes = Axes.Both;
        }

        public override void PlayAnimation()
        {
            if (Result != HitResult.Miss)
            {
                this
                    .MoveToY(-0.6f)
                    .MoveToY(-1.5f, 500);

                JudgementText
                    .ScaleTo(0.9f)
                    .ScaleTo(1, 500, Easing.OutElastic);

                this.FadeOutFromOne(800, Easing.OutQuint);
            }
            else
                base.PlayAnimation();
        }
    }
}
