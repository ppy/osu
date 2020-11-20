// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Pooling;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Configuration;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects.Drawables.Connections;
using osu.Game.Rulesets.Osu.Scoring;
using osu.Game.Rulesets.Osu.UI.Cursor;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Osu.UI
{
    public class OsuPlayfield : Playfield
    {
        public readonly Func<DrawableHitObject, double, bool> CheckHittable;

        private readonly PlayfieldBorder playfieldBorder;
        private readonly ProxyContainer approachCircles;
        private readonly ProxyContainer spinnerProxies;
        private readonly JudgementContainer<DrawableOsuJudgement> judgementLayer;
        private readonly FollowPointRenderer followPoints;
        private readonly OrderedHitPolicy hitPolicy;

        public static readonly Vector2 BASE_SIZE = new Vector2(512, 384);

        protected override GameplayCursorContainer CreateCursor() => new OsuCursorContainer();

        private readonly Bindable<bool> playfieldBorderStyle = new BindableBool();

        private readonly IDictionary<HitResult, DrawablePool<DrawableOsuJudgement>> poolDictionary = new Dictionary<HitResult, DrawablePool<DrawableOsuJudgement>>();

        public OsuPlayfield()
        {
            InternalChildren = new Drawable[]
            {
                playfieldBorder = new PlayfieldBorder
                {
                    RelativeSizeAxes = Axes.Both,
                    Depth = 3
                },
                spinnerProxies = new ProxyContainer
                {
                    RelativeSizeAxes = Axes.Both
                },
                followPoints = new FollowPointRenderer
                {
                    RelativeSizeAxes = Axes.Both,
                    Depth = 2,
                },
                judgementLayer = new JudgementContainer<DrawableOsuJudgement>
                {
                    RelativeSizeAxes = Axes.Both,
                    Depth = 1,
                },
                // Todo: This should not exist, but currently helps to reduce LOH allocations due to unbinding skin source events on judgement disposal
                // Todo: Remove when hitobjects are properly pooled
                new SkinProvidingContainer(null)
                {
                    Child = HitObjectContainer,
                },
                approachCircles = new ProxyContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Depth = -1,
                },
            };

            hitPolicy = new OrderedHitPolicy(HitObjectContainer);
            CheckHittable = hitPolicy.IsHittable;

            var hitWindows = new OsuHitWindows();

            foreach (var result in Enum.GetValues(typeof(HitResult)).OfType<HitResult>().Where(r => r > HitResult.None && hitWindows.IsHitResultAllowed(r)))
                poolDictionary.Add(result, new DrawableJudgementPool(result));

            AddRangeInternal(poolDictionary.Values);

            NewResult += onNewResult;
        }

        [BackgroundDependencyLoader(true)]
        private void load(OsuRulesetConfigManager config)
        {
            config?.BindWith(OsuRulesetSetting.PlayfieldBorderStyle, playfieldBorder.PlayfieldBorderStyle);

            registerPool<HitCircle, DrawableHitCircle>(10, 100);

            registerPool<Slider, DrawableSlider>(10, 100);
            registerPool<SliderHeadCircle, DrawableSliderHead>(10, 100);
            registerPool<SliderTailCircle, DrawableSliderTail>(10, 100);
            registerPool<SliderTick, DrawableSliderTick>(10, 100);
            registerPool<SliderRepeat, DrawableSliderRepeat>(5, 50);

            registerPool<Spinner, DrawableSpinner>(2, 20);
            registerPool<SpinnerTick, DrawableSpinnerTick>(10, 100);
            registerPool<SpinnerBonusTick, DrawableSpinnerBonusTick>(10, 100);
        }

        private void registerPool<TObject, TDrawable>(int initialSize, int? maximumSize = null)
            where TObject : HitObject
            where TDrawable : DrawableHitObject, new()
            => RegisterPool<TObject, TDrawable>(CreatePool<TDrawable>(initialSize, maximumSize));

        protected virtual DrawablePool<TDrawable> CreatePool<TDrawable>(int initialSize, int? maximumSize = null)
            where TDrawable : DrawableHitObject, new()
            => new DrawableOsuPool<TDrawable>(CheckHittable, OnHitObjectLoaded, initialSize, maximumSize);

        protected override HitObjectLifetimeEntry CreateLifetimeEntry(HitObject hitObject) => new OsuHitObjectLifetimeEntry(hitObject);

        protected override void OnHitObjectAdded(HitObject hitObject)
        {
            base.OnHitObjectAdded(hitObject);
            followPoints.AddFollowPoints((OsuHitObject)hitObject);
        }

        protected override void OnHitObjectRemoved(HitObject hitObject)
        {
            base.OnHitObjectRemoved(hitObject);
            followPoints.RemoveFollowPoints((OsuHitObject)hitObject);
        }

        public void OnHitObjectLoaded(Drawable drawable)
        {
            switch (drawable)
            {
                case DrawableSliderHead _:
                case DrawableSliderTail _:
                case DrawableSliderTick _:
                case DrawableSliderRepeat _:
                case DrawableSpinnerTick _:
                    break;

                case DrawableSpinner _:
                    spinnerProxies.Add(drawable.CreateProxy());
                    break;

                case IDrawableHitObjectWithProxiedApproach approach:
                    approachCircles.Add(approach.ProxiedLayer.CreateProxy());
                    break;
            }
        }

        private void onNewResult(DrawableHitObject judgedObject, JudgementResult result)
        {
            // Hitobjects that block future hits should miss previous hitobjects if they're hit out-of-order.
            hitPolicy.HandleHit(judgedObject);

            if (!judgedObject.DisplayResult || !DisplayJudgements.Value)
                return;

            DrawableOsuJudgement explosion = poolDictionary[result.Type].Get(doj => doj.Apply(result, judgedObject));

            judgementLayer.Add(explosion);
        }

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => HitObjectContainer.ReceivePositionalInputAt(screenSpacePos);

        private class ProxyContainer : LifetimeManagementContainer
        {
            public void Add(Drawable proxy) => AddInternal(proxy);
        }

        private class DrawableJudgementPool : DrawablePool<DrawableOsuJudgement>
        {
            private readonly HitResult result;

            public DrawableJudgementPool(HitResult result)
                : base(10)
            {
                this.result = result;
            }

            protected override DrawableOsuJudgement CreateNewDrawable()
            {
                var judgement = base.CreateNewDrawable();

                // just a placeholder to initialise the correct drawable hierarchy for this pool.
                judgement.Apply(new JudgementResult(new HitObject(), new Judgement()) { Type = result }, null);

                return judgement;
            }
        }

        private class OsuHitObjectLifetimeEntry : HitObjectLifetimeEntry
        {
            public OsuHitObjectLifetimeEntry(HitObject hitObject)
                : base(hitObject)
            {
            }

            protected override double InitialLifetimeOffset => ((OsuHitObject)HitObject).TimePreempt;
        }
    }
}
