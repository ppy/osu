// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Animations;
using osu.Framework.Graphics.Containers;
using osu.Framework.Utils;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;

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

            const double fade_in_length = 120;
            const double fade_out_delay = 500;
            const double fade_out_length = 600;

            this.FadeInFromZero(fade_in_length);
            this.Delay(fade_out_delay).FadeOut(fade_out_length);

            // legacy judgements don't play any transforms if they are an animation.
            if (animation?.FrameCount > 1)
                return;

            switch (result)
            {
                case HitResult.Miss:
                    this.ScaleTo(1.6f);
                    this.ScaleTo(1, 100, Easing.In);

                    float rotation = RNG.NextSingle(-8.6f, 8.6f);

                    this.RotateTo(0);
                    this.RotateTo(rotation, fade_in_length)
                        .Then().RotateTo(rotation * 2, fade_out_delay + fade_out_length - fade_in_length, Easing.In);
                    break;

                default:

                    this.ScaleTo(0.6f).Then()
                        .ScaleTo(1.1f, fade_in_length * 0.8f).Then()
                        // this is actually correct to match stable; there were overlapping transforms.
                        .ScaleTo(0.9f).Delay(fade_in_length * 0.2f)
                        .ScaleTo(1.1f).ScaleTo(0.9f, fade_in_length * 0.2f).Then()
                        .ScaleTo(0.95f).ScaleTo(finalScale, fade_in_length * 0.2f);
                    break;
            }
        }

        public Drawable GetAboveHitObjectsProxiedContent() => CreateProxy();
    }
}
