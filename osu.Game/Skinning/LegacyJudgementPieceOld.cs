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
    public class LegacyJudgementPieceOld : CompositeDrawable, IAnimatableJudgement
    {
        private readonly HitResult result;

        private readonly float finalScale;

        public LegacyJudgementPieceOld(HitResult result, Func<Drawable> createMainDrawable, float finalScale = 1f)
        {
            this.result = result;
            this.finalScale = finalScale;

            AutoSizeAxes = Axes.Both;
            Origin = Anchor.Centre;

            InternalChild = createMainDrawable();
        }

        public virtual void PlayAnimation()
        {
            var animation = InternalChild as IFramedAnimation;

            animation?.GotoFrame(0);

            this.RotateTo(0);
            this.MoveTo(Vector2.Zero);

            // legacy judgements don't play any transforms if they are an animation.
            if (animation?.FrameCount > 1)
                return;

            switch (result)
            {
                case HitResult.Miss:
                    this.ScaleTo(1.6f);
                    this.ScaleTo(1, 100, Easing.In);

                    this.MoveToOffset(new Vector2(0, 100), 800, Easing.InQuint);

                    this.RotateTo(40, 800, Easing.InQuint);
                    break;

                default:
                    const double animation_length = 120;

                    this.ScaleTo(0.6f).Then()
                        .ScaleTo(1.1f, animation_length * 0.8f).Then()
                        // this is actually correct to match stable; there were overlapping transforms.
                        .ScaleTo(0.9f).Delay(animation_length * 0.2f)
                        .ScaleTo(1.1f).ScaleTo(0.9f, animation_length * 0.2f).Then()
                        .ScaleTo(0.95f).ScaleTo(finalScale, animation_length * 0.2f);
                    break;
            }
        }
    }
}
