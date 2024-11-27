// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Graphics;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Mania.UI
{
    public partial class DrawableManiaJudgement : DrawableJudgement
    {
        protected override Drawable CreateDefaultJudgement(HitResult result) => new DefaultManiaJudgementPiece(result);

        private partial class DefaultManiaJudgementPiece : DefaultJudgementPiece
        {
            public DefaultManiaJudgementPiece(HitResult result)
                : base(result)
            {
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                JudgementText.Font = JudgementText.Font.With(size: 25);
            }

            public override void PlayAnimation()
            {
                switch (Result)
                {
                    case HitResult.None:
                    case HitResult.Miss:
                        base.PlayAnimation();
                        break;

                    default:
                        this.ScaleTo(0.8f);
                        this.ScaleTo(1, 250, Easing.OutElastic);

                        this.Delay(50)
                            .ScaleTo(0.75f, 250)
                            .FadeOut(200);

                        // osu!mania uses a custom fade length, so the base call is intentionally omitted.
                        break;
                }
            }
        }
    }
}
