// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Animations;
using osu.Framework.Graphics.Containers;
using osu.Framework.Utils;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;
using osuTK;

namespace osu.Game.Skinning
{
    public partial class LegacyJudgementPieceOld : CompositeDrawable, IAnimatableJudgement
    {
        private readonly HitResult result;

        private readonly float finalScale;
        private readonly bool forceTransforms;

        protected virtual double FadeInLength => 120;
        protected virtual double FadeOutDelay => 500;
        protected virtual double FadeOutLength => 600;

        protected Drawable Sprite { get; private set; }

        [Resolved]
        private ISkinSource skin { get; set; } = null!;

        public LegacyJudgementPieceOld(HitResult result, Func<Drawable> createMainDrawable, float finalScale = 1f, bool forceTransforms = false)
        {
            this.result = result;
            this.finalScale = finalScale;
            this.forceTransforms = forceTransforms;

            AutoSizeAxes = Axes.Both;
            Origin = Anchor.Centre;

            InternalChild = Sprite = createMainDrawable();
        }

        public virtual void PlayAnimation()
        {
            var animation = Sprite as IFramedAnimation;

            animation?.GotoFrame(0);

            this.FadeInFromZero(FadeInLength);
            this.Delay(FadeOutDelay).FadeOut(FadeOutLength);

            // legacy judgements don't play any transforms if they are an animation.... UNLESS they are the temporary displayed judgement from new piece.
            if (animation?.FrameCount > 1 && !forceTransforms)
                return;

            switch (result)
            {
                case HitResult.Miss:
                    this.ScaleTo(1.6f);
                    this.ScaleTo(1, 100, Easing.In);

                    if (DropAnimationOnMiss)
                    {
                        this.MoveTo(new Vector2(0, -5));
                        this.MoveToOffset(new Vector2(0, 80), FadeOutDelay + FadeOutLength, Easing.In);
                    }

                    float rotation = RNG.NextSingle(-8.6f, 8.6f);

                    this.RotateTo(0);
                    this.RotateTo(rotation, FadeInLength)
                        .Then().RotateTo(rotation * 2, FadeOutDelay + FadeOutLength - FadeInLength, Easing.In);
                    break;

                default:

                    this.ScaleTo(0.6f).Then()
                        .ScaleTo(1.1f, FadeInLength * 0.8f).Then() // t = 0.8
                        .Delay(FadeInLength * 0.2f) // t = 1.0
                        .ScaleTo(0.9f, FadeInLength * 0.2f).Then() // t = 1.2

                        // stable dictates scale of 0.9->1 over time 1.0 to 1.4, but we are already at 1.2.
                        // so we need to force the current value to be correct at 1.2 (0.95) then complete the
                        // second half of the transform.
                        .ScaleTo(0.95f).ScaleTo(finalScale, FadeInLength * 0.2f); // t = 1.4
                    break;
            }
        }

        public Drawable GetAboveHitObjectsProxiedContent() => CreateProxy();

        protected virtual bool DropAnimationOnMiss
        {
            get
            {
                decimal? legacyVersion = skin.GetConfig<SkinConfiguration.LegacySetting, decimal>(SkinConfiguration.LegacySetting.Version)?.Value;

                return legacyVersion >= 2.0m;
            }
        }
    }
}
