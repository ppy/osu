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
            const double animation_time = 120;

            (sprite as IFramedAnimation)?.GotoFrame(0);
            (strongSprite as IFramedAnimation)?.GotoFrame(0);

            this.FadeInFromZero(animation_time).Then().FadeOut(animation_time * 1.5);

            this.ScaleTo(0.6f)
                .Then().ScaleTo(1.1f, animation_time * 0.8)
                .Then().ScaleTo(0.9f, animation_time * 0.4)
                .Then().ScaleTo(1f, animation_time * 0.2);
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
