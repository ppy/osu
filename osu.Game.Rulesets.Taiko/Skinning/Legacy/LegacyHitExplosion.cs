// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Animations;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Taiko.UI;

namespace osu.Game.Rulesets.Taiko.Skinning.Legacy
{
    public partial class LegacyHitExplosion : CompositeDrawable, IAnimatableHitExplosion
    {
        private readonly Drawable sprite;

        private readonly Drawable? strongSprite;

        /// <summary>
        /// Creates a new legacy hit explosion.
        /// </summary>
        /// <remarks>
        /// Contrary to stable's, this implementation doesn't require a frame-perfect hit
        /// for the strong sprite to be displayed.
        /// </remarks>
        /// <param name="sprite">The normal legacy explosion sprite.</param>
        /// <param name="strongSprite">The strong legacy explosion sprite.</param>
        public LegacyHitExplosion(Drawable sprite, Drawable? strongSprite = null)
        {
            this.sprite = sprite;
            this.strongSprite = strongSprite;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            AutoSizeAxes = Axes.Both;

            AddInternal(sprite.With(s =>
            {
                s.Anchor = Anchor.Centre;
                s.Origin = Anchor.Centre;
            }));

            if (strongSprite != null)
            {
                AddInternal(strongSprite.With(s =>
                {
                    s.Alpha = 0;
                    s.Anchor = Anchor.Centre;
                    s.Origin = Anchor.Centre;
                }));
            }
        }

        public void Animate(DrawableHitObject drawableHitObject)
        {
            const double fade_in_length = 120;

            var animation = sprite as IFramedAnimation;

            animation?.GotoFrame(0);
            (strongSprite as IFramedAnimation)?.GotoFrame(0);

            bool shouldAnimate = animation?.FrameCount > 1;

            if (shouldAnimate)
            {
                // This logic matches LegacyJudgementPieceOld. So why aren't we just using that class? I guess for the adjusted fadeout length below.
                this
                    .ScaleTo(0.6f)
                    .Then().ScaleTo(1.1f, fade_in_length * 0.8f)
                    .Then() // t = 0.8
                    .Delay(fade_in_length * 0.2f) // t = 1.0
                    .ScaleTo(0.9f, fade_in_length * 0.2f).Then() // t = 1.2

                    // stable dictates scale of 0.9->1 over time 1.0 to 1.4, but we are already at 1.2.
                    // so we need to force the current value to be correct at 1.2 (0.95) then complete the
                    // second half of the transform.
                    .ScaleTo(0.95f).ScaleTo(1, fade_in_length * 0.2f); // t = 1.4
            }

            this.FadeInFromZero(fade_in_length)
                .Then().FadeOut(fade_in_length * 1.5);
        }

        public void AnimateSecondHit()
        {
            if (strongSprite == null)
                return;

            sprite.FadeOut(50, Easing.OutQuint);
            strongSprite.FadeIn(50, Easing.OutQuint);
        }
    }
}
