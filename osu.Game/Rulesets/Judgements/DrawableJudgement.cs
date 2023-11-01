// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

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
    public partial class DrawableJudgement : PoolableDrawable
    {
        private const float judgement_size = 128;

        public JudgementResult Result { get; private set; }

        public DrawableHitObject JudgedObject { get; private set; }

        public override bool RemoveCompletedTransforms => false;

        protected SkinnableDrawable JudgementBody { get; private set; }

        private readonly Container aboveHitObjectsContent;

        private readonly Lazy<Drawable> proxiedAboveHitObjectsContent;
        public Drawable ProxiedAboveHitObjectsContent => proxiedAboveHitObjectsContent.Value;

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

            AddInternal(aboveHitObjectsContent = new Container
            {
                Depth = float.MinValue,
                RelativeSizeAxes = Axes.Both
            });

            proxiedAboveHitObjectsContent = new Lazy<Drawable>(() => aboveHitObjectsContent.CreateProxy());
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            prepareDrawables();
        }

        /// <summary>
        /// Apply top-level animations to the current judgement when successfully hit.
        /// If displaying components which require lifetime extensions, manually adjusting <see cref="Drawable.LifetimeEnd"/> is required.
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
        /// If displaying components which require lifetime extensions, manually adjusting <see cref="Drawable.LifetimeEnd"/> is required.
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
        public virtual void Apply([NotNull] JudgementResult result, [CanBeNull] DrawableHitObject judgedObject)
        {
            Result = result;
            JudgedObject = judgedObject;
        }

        protected override void PrepareForUse()
        {
            base.PrepareForUse();

            Debug.Assert(Result != null);

            runAnimation();
        }

        private void runAnimation()
        {
            // is a no-op if the drawables are already in a correct state.
            prepareDrawables();

            // undo any transforms applies in ApplyMissAnimations/ApplyHitAnimations to get a sane initial state.
            ApplyTransformsAt(double.MinValue, true);
            ClearTransforms(true);

            LifetimeStart = Result.TimeAbsolute;

            using (BeginAbsoluteSequence(Result.TimeAbsolute))
            {
                // not sure if this should remain going forward.
                JudgementBody.ResetAnimation();

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

                if (JudgementBody.Drawable is IAnimatableJudgement animatable)
                    animatable.PlayAnimation();

                // a derived version of DrawableJudgement may be proposing a lifetime.
                // if not adjusted (or the skinned portion requires greater bounds than calculated) use the skinned source's lifetime.
                double lastTransformTime = JudgementBody.Drawable.LatestTransformEndTime;
                if (LifetimeEnd == double.MaxValue || lastTransformTime > LifetimeEnd)
                    LifetimeEnd = lastTransformTime;
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
                RemoveInternal(JudgementBody, true);

            AddInternal(JudgementBody = new SkinnableDrawable(new GameplaySkinComponentLookup<HitResult>(type), _ =>
                CreateDefaultJudgement(type), confineMode: ConfineMode.NoScaling));

            JudgementBody.OnSkinChanged += () =>
            {
                // on a skin change, the child component will update but not get correctly triggered to play its animation (or proxy the newly created content).
                // we need to trigger a reinitialisation to make things right.
                proxyContent();
                runAnimation();
            };

            proxyContent();

            currentDrawableType = type;

            void proxyContent()
            {
                aboveHitObjectsContent.Clear();

                if (JudgementBody.Drawable is IAnimatableJudgement animatable)
                {
                    var proxiedContent = animatable.GetAboveHitObjectsProxiedContent();
                    if (proxiedContent != null)
                        aboveHitObjectsContent.Add(proxiedContent);
                }
            }
        }

        protected virtual Drawable CreateDefaultJudgement(HitResult result) => new DefaultJudgementPiece(result);
    }
}
