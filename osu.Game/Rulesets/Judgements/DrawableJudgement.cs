// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Pooling;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Judgements
{
    /// <summary>
    /// A drawable object which visualises the hit result of a <see cref="Judgements.Judgement"/>.
    /// </summary>
    public class DrawableJudgement : PoolableDrawable
    {
        private const float judgement_size = 128;

        public JudgementResult Result { get; private set; }

        public DrawableHitObject JudgedObject { get; private set; }

        protected Container JudgementBody { get; private set; }

        private SkinnableDrawable skinnableJudgement;

        /// <summary>
        /// Duration of initial fade in.
        /// </summary>
        [Obsolete("Apply any animations manually via ApplyHitAnimations / ApplyMissAnimations. Defaults were moved inside skinned components.")]
        protected virtual double FadeInDuration => 100;

        /// <summary>
        /// Duration to wait until fade out begins. Defaults to <see cref="FadeInDuration"/>.
        /// </summary>
        [Obsolete("Apply any animations manually via ApplyHitAnimations / ApplyMissAnimations. Defaults were moved inside skinned components.")]
        protected virtual double FadeOutDelay => FadeInDuration;

        /// <summary>
        /// Creates a drawable which visualises a <see cref="Judgements.Judgement"/>.
        /// </summary>
        /// <param name="result">The judgement to visualise.</param>
        /// <param name="judgedObject">The object which was judged.</param>
        public DrawableJudgement(JudgementResult result, DrawableHitObject judgedObject)
            : this()
        {
            Apply(result, judgedObject);
        }

        public DrawableJudgement()
        {
            Size = new Vector2(judgement_size);
            Origin = Anchor.Centre;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            prepareDrawables();
        }

        /// <summary>
        /// Apply top-level animations to the current judgement when successfully hit.
        /// Generally used for fading, defaulting to a simple fade out based on <see cref="FadeOutDelay"/>.
        /// This will be used to calculate the lifetime of the judgement.
        /// </summary>
        /// <remarks>
        /// For animating the actual "default skin" judgement itself, it is recommended to use <see cref="CreateDefaultJudgement"/>.
        /// This allows applying animations which don't affect custom skins.
        /// </remarks>
        protected virtual void ApplyHitAnimations()
        {
        }

        /// <summary>
        /// Apply top-level animations to the current judgement when missed.
        /// Generally used for fading, defaulting to a simple fade out based on <see cref="FadeOutDelay"/>.
        /// This will be used to calculate the lifetime of the judgement.
        /// </summary>
        /// <remarks>
        /// For animating the actual "default skin" judgement itself, it is recommended to use <see cref="CreateDefaultJudgement"/>.
        /// This allows applying animations which don't affect custom skins.
        /// </remarks>
        protected virtual void ApplyMissAnimations()
        {
        }

        /// <summary>
        /// Associate a new result / object with this judgement. Should be called when retrieving a judgement from a pool.
        /// </summary>
        /// <param name="result">The applicable judgement.</param>
        /// <param name="judgedObject">The drawable object.</param>
        public void Apply([NotNull] JudgementResult result, [CanBeNull] DrawableHitObject judgedObject)
        {
            Result = result;
            JudgedObject = judgedObject;
        }

        protected override void PrepareForUse()
        {
            base.PrepareForUse();

            Debug.Assert(Result != null);

            prepareDrawables();

            LifetimeStart = Result.TimeAbsolute;

            using (BeginAbsoluteSequence(Result.TimeAbsolute, true))
            {
                // not sure if this should remain going forward.
                skinnableJudgement.ResetAnimation();

                switch (Result.Type)
                {
                    case HitResult.None:
                        break;

                    case HitResult.Miss:
                        ApplyMissAnimations();
                        break;

                    default:
                        ApplyHitAnimations();
                        break;
                }

                if (skinnableJudgement.Drawable is IAnimatableJudgement animatable)
                {
                    var drawableAnimation = (Drawable)animatable;

                    drawableAnimation.ClearTransforms();

                    animatable.PlayAnimation();

                    drawableAnimation.Expire(true);

                    // a derived version of DrawableJudgement may be adjusting lifetime.
                    // if not adjusted (or the skinned portion requires greater bounds than calculated) use the skinned source's lifetime.
                    if (LifetimeEnd == double.MaxValue || drawableAnimation.LifetimeEnd > LifetimeEnd)
                        LifetimeEnd = drawableAnimation.LifetimeEnd;
                }
            }
        }

        private HitResult? currentDrawableType;

        private void prepareDrawables()
        {
            var type = Result?.Type ?? HitResult.Perfect; //TODO: better default type from ruleset

            // todo: this should be removed once judgements are always pooled.
            if (type == currentDrawableType)
                return;

            // sub-classes might have added their own children that would be removed here if .InternalChild was used.
            if (JudgementBody != null)
                RemoveInternal(JudgementBody);

            AddInternal(JudgementBody = new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Child = skinnableJudgement = new SkinnableDrawable(new GameplaySkinComponent<HitResult>(type), _ =>
                    CreateDefaultJudgement(type), confineMode: ConfineMode.NoScaling)
            });

            currentDrawableType = type;
        }

        protected virtual Drawable CreateDefaultJudgement(HitResult result) => new DefaultJudgementPiece(result);
    }
}
