// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Animations;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;
using osuTK;

namespace osu.Game.Skinning
{
    public class LegacyJudgementPiece : CompositeDrawable, IAnimatableJudgement
    {
        private readonly HitResult result;

        private bool hasParticle;

        public LegacyJudgementPiece(HitResult result, Drawable drawable)
        {
            this.result = result;

            AutoSizeAxes = Axes.Both;
            Origin = Anchor.Centre;

            InternalChild = drawable;
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

            const double animation_length = 500;

            switch (result)
            {
                case HitResult.Miss:
                    this.ScaleTo(1.6f);
                    this.ScaleTo(1, 100, Easing.In);

                    this.MoveToOffset(new Vector2(0, 100), 800, Easing.InQuint);

                    this.RotateTo(40, 800, Easing.InQuint);
                    break;

                default:
                    if (!hasParticle)
                    {
                        this.ScaleTo(0.6f).Then()
                            .ScaleTo(1.1f, animation_length * 0.8f).Then()
                            .ScaleTo(0.9f, animation_length * 0.4f).Then()
                            .ScaleTo(1f, animation_length * 0.2f);
                    }
                    else
                    {
                        this.ScaleTo(0.9f);
                        this.ScaleTo(1.05f, animation_length);
                    }

                    break;
            }
        }
    }
}
