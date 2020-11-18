// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Animations;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;
using osuTK;

namespace osu.Game.Skinning
{
    public class LegacyJudgementPieceNew : CompositeDrawable, IAnimatableJudgement
    {
        private readonly HitResult result;

        private readonly LegacyJudgementPieceOld temporaryOldStyle;

        private readonly Drawable mainPiece;

        public LegacyJudgementPieceNew(HitResult result, Func<Drawable> createMainDrawable, Drawable particleDrawable)
        {
            this.result = result;

            AutoSizeAxes = Axes.Both;
            Origin = Anchor.Centre;

            InternalChildren = new[]
            {
                mainPiece = createMainDrawable().With(d =>
                {
                    d.Anchor = Anchor.Centre;
                    d.Origin = Anchor.Centre;
                })
            };

            if (result != HitResult.Miss)
            {
                //new judgement shows old as a temporary effect
                AddInternal(temporaryOldStyle = new LegacyJudgementPieceOld(result, createMainDrawable, 1.05f)
                {
                    Blending = BlendingParameters.Additive,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                });
            }
        }

        public void PlayAnimation()
        {
            var animation = mainPiece as IFramedAnimation;

            animation?.GotoFrame(0);

            this.RotateTo(0);
            this.MoveTo(Vector2.Zero);

            if (temporaryOldStyle != null)
            {
                temporaryOldStyle.PlayAnimation();

                temporaryOldStyle.Hide();
                temporaryOldStyle.Delay(-16)
                                 .FadeTo(0.5f, 56, Easing.Out).Then()
                                 .FadeOut(300);
            }

            // legacy judgements don't play any transforms if they are an animation.
            if (animation?.FrameCount > 1)
                return;

            switch (result)
            {
                case HitResult.Miss:
                    mainPiece.ScaleTo(1.6f);
                    mainPiece.ScaleTo(1, 100, Easing.In);

                    mainPiece.MoveToOffset(new Vector2(0, 100), 800, Easing.InQuint);

                    mainPiece.RotateTo(40, 800, Easing.InQuint);
                    break;

                default:
                    const double animation_length = 1100;

                    mainPiece.ScaleTo(0.9f);
                    mainPiece.ScaleTo(1.05f, animation_length);
                    break;
            }
        }
    }
}
