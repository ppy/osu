// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Taiko.Objects.Drawables;

namespace osu.Game.Rulesets.Taiko.Skinning.Legacy
{
    public class LegacyHitExplosion : CompositeDrawable
    {
        private readonly Drawable sprite;
        private readonly Drawable strongSprite;

        private DrawableStrongNestedHit nestedStrongHit;
        private bool switchedToStrongSprite;

        /// <summary>
        /// Creates a new legacy hit explosion.
        /// </summary>
        /// <remarks>
        /// Contrary to stable's, this implementation doesn't require a frame-perfect hit
        /// for the strong sprite to be displayed.
        /// </remarks>
        /// <param name="sprite">The normal legacy explosion sprite.</param>
        /// <param name="strongSprite">The strong legacy explosion sprite.</param>
        public LegacyHitExplosion(Drawable sprite, Drawable strongSprite = null)
        {
            this.sprite = sprite;
            this.strongSprite = strongSprite;
        }

        [BackgroundDependencyLoader]
        private void load(DrawableHitObject judgedObject)
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

            if (judgedObject is DrawableHit hit)
                nestedStrongHit = hit.NestedHitObjects.SingleOrDefault() as DrawableStrongNestedHit;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            const double animation_time = 120;

            this.FadeInFromZero(animation_time).Then().FadeOut(animation_time * 1.5);

            this.ScaleTo(0.6f)
                .Then().ScaleTo(1.1f, animation_time * 0.8)
                .Then().ScaleTo(0.9f, animation_time * 0.4)
                .Then().ScaleTo(1f, animation_time * 0.2);

            Expire(true);
        }

        protected override void Update()
        {
            base.Update();

            if (shouldSwitchToStrongSprite() && !switchedToStrongSprite)
            {
                sprite.FadeOut(50, Easing.OutQuint);
                strongSprite.FadeIn(50, Easing.OutQuint);
                switchedToStrongSprite = true;
            }
        }

        private bool shouldSwitchToStrongSprite()
        {
            if (nestedStrongHit == null || strongSprite == null)
                return false;

            return nestedStrongHit.IsHit;
        }
    }
}
