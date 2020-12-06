// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
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
using osuTK;

namespace osu.Game.Rulesets.Osu.UI
{
    public class OsuPlayfield : Playfield
    {
        private readonly PlayfieldBorder playfieldBorder;
        private readonly ProxyContainer approachCircles;
        private readonly ProxyContainer spinnerProxies;
        private readonly JudgementContainer<DrawableOsuJudgement> judgementLayer;
        private readonly FollowPointRenderer followPoints;
        private readonly OrderedHitPolicy hitPolicy;

        public static readonly Vector2 BASE_SIZE = new Vector2(512, 384);

        protected override GameplayCursorContainer CreateCursor() => new OsuCursorContainer();

        private readonly IDictionary<HitResult, DrawablePool<DrawableOsuJudgement>> poolDictionary = new Dictionary<HitResult, DrawablePool<DrawableOsuJudgement>>();

        private readonly Container judgementAboveHitObjectLayer;

        public OsuPlayfield()
        {
            InternalChildren = new Drawable[]
            {
                playfieldBorder = new PlayfieldBorder { RelativeSizeAxes = Axes.Both },
                spinnerProxies = new ProxyContainer { RelativeSizeAxes = Axes.Both },
                followPoints = new FollowPointRenderer { RelativeSizeAxes = Axes.Both },
                judgementLayer = new JudgementContainer<DrawableOsuJudgement> { RelativeSizeAxes = Axes.Both },
                HitObjectContainer,
                judgementAboveHitObjectLayer = new Container { RelativeSizeAxes = Axes.Both },
                approachCircles = new ProxyContainer { RelativeSizeAxes = Axes.Both },
            };

            hitPolicy = new OrderedHitPolicy(HitObjectContainer);

            var hitWindows = new OsuHitWindows();

            foreach (var result in Enum.GetValues(typeof(HitResult)).OfType<HitResult>().Where(r => r > HitResult.None && hitWindows.IsHitResultAllowed(r)))
                poolDictionary.Add(result, new DrawableJudgementPool(result, onJudgmentLoaded));

            AddRangeInternal(poolDictionary.Values);

            NewResult += onNewResult;
        }

        protected override void OnNewDrawableHitObject(DrawableHitObject drawable)
        {
            ((DrawableOsuHitObject)drawable).CheckHittable = hitPolicy.IsHittable;

            Debug.Assert(!drawable.IsLoaded, $"Already loaded {nameof(DrawableHitObject)} is added to {nameof(OsuPlayfield)}");
            drawable.OnLoadComplete += onDrawableHitObjectLoaded;
        }

        private void onDrawableHitObjectLoaded(Drawable drawable)
        {
            // note: `Slider`'s `ProxiedLayer` is added when its nested `DrawableHitCircle` is loaded.
            switch (drawable)
            {
                case DrawableSpinner _:
                    spinnerProxies.Add(drawable.CreateProxy());
                    break;

                case DrawableHitCircle hitCircle:
                    approachCircles.Add(hitCircle.ProxiedLayer.CreateProxy());
                    break;
            }
        }

        private void onJudgmentLoaded(DrawableOsuJudgement judgement)
        {
            judgementAboveHitObjectLayer.Add(judgement.GetProxyAboveHitObjectsContent());
        }

        [BackgroundDependencyLoader(true)]
        private void load(OsuRulesetConfigManager config)
        {
            config?.BindWith(OsuRulesetSetting.PlayfieldBorderStyle, playfieldBorder.PlayfieldBorderStyle);

            RegisterPool<HitCircle, DrawableHitCircle>(10, 100);

            RegisterPool<Slider, DrawableSlider>(10, 100);
            RegisterPool<SliderHeadCircle, DrawableSliderHead>(10, 100);
            RegisterPool<SliderTailCircle, DrawableSliderTail>(10, 100);
            RegisterPool<SliderTick, DrawableSliderTick>(10, 100);
            RegisterPool<SliderRepeat, DrawableSliderRepeat>(5, 50);

            RegisterPool<Spinner, DrawableSpinner>(2, 20);
            RegisterPool<SpinnerTick, DrawableSpinnerTick>(10, 100);
            RegisterPool<SpinnerBonusTick, DrawableSpinnerBonusTick>(10, 100);
        }

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
            private readonly Action<DrawableOsuJudgement> onLoaded;

            public DrawableJudgementPool(HitResult result, Action<DrawableOsuJudgement> onLoaded)
                : base(10)
            {
                this.result = result;
                this.onLoaded = onLoaded;
            }

            protected override DrawableOsuJudgement CreateNewDrawable()
            {
                var judgement = base.CreateNewDrawable();

                // just a placeholder to initialise the correct drawable hierarchy for this pool.
                judgement.Apply(new JudgementResult(new HitObject(), new Judgement()) { Type = result }, null);

                onLoaded?.Invoke(judgement);

                return judgement;
            }
        }

        private class OsuHitObjectLifetimeEntry : HitObjectLifetimeEntry
        {
            public OsuHitObjectLifetimeEntry(HitObject hitObject)
                : base(hitObject)
            {
                // Prevent past objects in idles states from remaining alive as their end times are skipped in non-frame-stable contexts.
                LifetimeEnd = HitObject.GetEndTime() + HitObject.HitWindows.WindowFor(HitResult.Miss);
            }

            protected override double InitialLifetimeOffset => ((OsuHitObject)HitObject).TimePreempt;
        }
    }
}
