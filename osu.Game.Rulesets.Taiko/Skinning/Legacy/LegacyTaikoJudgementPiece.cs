// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Animations;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko.UI;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Taiko.Skinning.Legacy
{
    public partial class LegacyTaikoJudgementPiece : LegacyJudgementPieceOld, IAnimatableHitExplosion
    {
        private readonly Drawable? strongSprite;

        /// <summary>
        /// Creates a new legacy hit explosion.
        /// </summary>
        /// <remarks>
        /// Contrary to stable's, this implementation doesn't require a frame-perfect hit
        /// for the strong sprite to be displayed.
        /// </remarks>
        /// <param name="hitResult"></param>
        /// <param name="sprite">The normal legacy explosion sprite.</param>
        /// <param name="strongSprite">The strong legacy explosion sprite.</param>
        public LegacyTaikoJudgementPiece(HitResult hitResult, Drawable sprite, Drawable? strongSprite = null)
            : base(hitResult, () => sprite)
        {
            this.strongSprite = strongSprite;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
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

        protected override double FadeOutDelay => FadeInLength;
        protected override double FadeOutLength => FadeInLength;
        protected override bool DropAnimationOnMiss => false;

        public override void PlayAnimation()
        {
            base.PlayAnimation();

            (strongSprite as IFramedAnimation)?.GotoFrame(0);
        }

        public void Animate(DrawableHitObject drawableHitObject) => PlayAnimation();

        public void AnimateSecondHit()
        {
            if (strongSprite == null)
                return;

            Sprite.FadeOut(50, Easing.OutQuint);
            strongSprite.FadeIn(50, Easing.OutQuint);
        }
    }
}
