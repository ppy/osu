// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using JetBrains.Annotations;
using osuTK;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Pooling;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Taiko.UI
{
    /// <summary>
    /// A circle explodes from the hit target to indicate a hitobject has been hit.
    /// </summary>
    internal class HitExplosion : PoolableDrawable
    {
        public override bool RemoveWhenNotAlive => true;
        public override bool RemoveCompletedTransforms => false;

        private readonly HitResult result;

        [CanBeNull]
        public DrawableHitObject JudgedObject;

        private SkinnableDrawable skinnable;

        /// <summary>
        /// This constructor only exists to meet the <c>new()</c> type constraint of <see cref="DrawablePool{T}"/>.
        /// </summary>
        public HitExplosion()
            : this(HitResult.Great)
        {
        }

        public HitExplosion(HitResult result)
        {
            this.result = result;

            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            Size = new Vector2(TaikoHitObject.DEFAULT_SIZE);
            RelativeSizeAxes = Axes.Both;

            RelativePositionAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = skinnable = new SkinnableDrawable(new TaikoSkinComponent(getComponentName(result)), _ => new DefaultHitExplosion(result));
            skinnable.OnSkinChanged += runAnimation;
        }

        public void Apply([CanBeNull] DrawableHitObject drawableHitObject)
        {
            JudgedObject = drawableHitObject;
        }

        protected override void PrepareForUse()
        {
            base.PrepareForUse();
            runAnimation();
        }

        protected override void FreeAfterUse()
        {
            base.FreeAfterUse();

            // clean up transforms on free instead of on prepare as is usually the case
            // to avoid potentially overriding the effects of VisualiseSecondHit() in the case it is called before PrepareForUse().
            ApplyTransformsAt(double.MinValue, true);
            ClearTransforms(true);
        }

        private void runAnimation()
        {
            if (JudgedObject?.Result == null)
                return;

            double resultTime = JudgedObject.Result.TimeAbsolute;

            LifetimeStart = resultTime;

            using (BeginAbsoluteSequence(resultTime))
                (skinnable.Drawable as IHitExplosion)?.Animate(JudgedObject);

            LifetimeEnd = skinnable.Drawable.LatestTransformEndTime;
        }

        private static TaikoSkinComponents getComponentName(HitResult result)
        {
            switch (result)
            {
                case HitResult.Miss:
                    return TaikoSkinComponents.TaikoExplosionMiss;

                case HitResult.Ok:
                    return TaikoSkinComponents.TaikoExplosionOk;

                case HitResult.Great:
                    return TaikoSkinComponents.TaikoExplosionGreat;
            }

            throw new ArgumentOutOfRangeException(nameof(result), $"Invalid result type: {result}");
        }

        public void VisualiseSecondHit(JudgementResult judgementResult)
        {
            using (BeginAbsoluteSequence(judgementResult.TimeAbsolute))
            {
                this.ResizeTo(new Vector2(TaikoStrongableHitObject.DEFAULT_STRONG_SIZE), 50);
                (skinnable.Drawable as IHitExplosion)?.AnimateSecondHit();
            }
        }
    }
}
