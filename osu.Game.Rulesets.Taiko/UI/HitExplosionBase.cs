// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Pooling;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Taiko.UI
{
    /// <summary>
    /// A base class for taiko explosions from target hitting to indicate a hitobject has been hit.
    /// </summary>
    internal abstract partial class HitExplosionBase : PoolableDrawable
    {
        /// <summary>Creates <c>Skinnable</c>. Calls only on <c>load</c>.</summary>
        protected abstract SkinnableDrawable OnLoadSkinnableCreate();

        public override bool RemoveWhenNotAlive => true;
        public override bool RemoveCompletedTransforms => false;


        protected double? SecondHitTime;

        public DrawableHitObject? JudgedObject;

        protected SkinnableDrawable Skinnable = null!;

        public HitExplosionBase()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = Skinnable = OnLoadSkinnableCreate();
            Skinnable.OnSkinChanged += RunAnimation;
        }

        public void Apply(DrawableHitObject? drawableHitObject)
        {
            JudgedObject = drawableHitObject;
            SecondHitTime = null;
        }

        protected override void PrepareForUse()
        {
            base.PrepareForUse();
            RunAnimation();
        }

        protected void RunAnimation()
        {
            if (JudgedObject?.Result is null) return;

            double resultTime = JudgedObject.Result.TimeAbsolute;
            LifetimeStart = resultTime;

            // Clear transforms
            ApplyTransformsAt(double.MinValue, true);
            ClearTransforms(true);

            if (Skinnable.Drawable is IAnimatableHitExplosion animatable)
            {
                using (BeginAbsoluteSequence(resultTime))
                    animatable.Animate(JudgedObject);

                if (SecondHitTime != null)
                    using (BeginAbsoluteSequence(SecondHitTime.Value))
                        animatable.AnimateSecondHit();
            }

            LifetimeEnd = Skinnable.Drawable.LatestTransformEndTime;
        }

        public void VisualiseSecondHit(JudgementResult judgementResult)
        {
            SecondHitTime = judgementResult.TimeAbsolute;
            RunAnimation();
        }
    }
}
