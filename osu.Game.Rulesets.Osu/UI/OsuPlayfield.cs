// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Beatmaps;
using osu.Game.Rulesets.Osu.Configuration;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects.Drawables.Connections;
using osu.Game.Rulesets.Osu.UI.Cursor;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osuTK;

namespace osu.Game.Rulesets.Osu.UI
{
    [Cached]
    public partial class OsuPlayfield : Playfield
    {
        private readonly PlayfieldBorder playfieldBorder;
        private readonly ProxyContainer approachCircles;
        private readonly ProxyContainer spinnerProxies;
        private readonly JudgementContainer<DrawableOsuJudgement> judgementLayer;

        private readonly JudgementPooler<DrawableOsuJudgement> judgementPooler;

        public SmokeContainer Smoke { get; }
        public FollowPointRenderer FollowPoints { get; }

        public static readonly Vector2 BASE_SIZE = new Vector2(512, 384);

        protected override GameplayCursorContainer CreateCursor() => new OsuCursorContainer();

        private readonly Container judgementAboveHitObjectLayer;

        public OsuPlayfield()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            InternalChildren = new Drawable[]
            {
                playfieldBorder = new PlayfieldBorder { RelativeSizeAxes = Axes.Both },
                Smoke = new SmokeContainer { RelativeSizeAxes = Axes.Both },
                spinnerProxies = new ProxyContainer { RelativeSizeAxes = Axes.Both },
                FollowPoints = new FollowPointRenderer { RelativeSizeAxes = Axes.Both },
                judgementLayer = new JudgementContainer<DrawableOsuJudgement> { RelativeSizeAxes = Axes.Both },
                HitObjectContainer,
                judgementAboveHitObjectLayer = new Container { RelativeSizeAxes = Axes.Both },
                approachCircles = new ProxyContainer { RelativeSizeAxes = Axes.Both },
            };

            HitPolicy = new StartTimeOrderedHitPolicy();

            AddInternal(judgementPooler = new JudgementPooler<DrawableOsuJudgement>(new[]
            {
                HitResult.Great,
                HitResult.Ok,
                HitResult.Meh,
                HitResult.Miss,
                HitResult.LargeTickMiss,
                HitResult.IgnoreMiss,
            }, onJudgementLoaded));

            NewResult += onNewResult;
        }

        private IHitPolicy hitPolicy;

        public IHitPolicy HitPolicy
        {
            get => hitPolicy;
            set
            {
                hitPolicy = value ?? throw new ArgumentNullException(nameof(value));
                hitPolicy.HitObjectContainer = HitObjectContainer;
            }
        }

        protected override void OnNewDrawableHitObject(DrawableHitObject drawable)
        {
            ((DrawableOsuHitObject)drawable).CheckHittable = hitPolicy.CheckHittable;

            Debug.Assert(!drawable.IsLoaded, $"Already loaded {nameof(DrawableHitObject)} is added to {nameof(OsuPlayfield)}");
            drawable.OnLoadComplete += onDrawableHitObjectLoaded;
        }

        private void onDrawableHitObjectLoaded(Drawable drawable)
        {
            // note: `Slider`'s `ProxiedLayer` is added when its nested `DrawableHitCircle` is loaded.
            switch (drawable)
            {
                case DrawableSpinner:
                    spinnerProxies.Add(drawable.CreateProxy());
                    break;

                case DrawableHitCircle hitCircle:
                    approachCircles.Add(hitCircle.ProxiedLayer.CreateProxy());
                    break;
            }
        }

        private void onJudgementLoaded(DrawableOsuJudgement judgement)
        {
            judgementAboveHitObjectLayer.Add(judgement.ProxiedAboveHitObjectsContent);
        }

        [BackgroundDependencyLoader(true)]
        private void load(OsuRulesetConfigManager config, IBeatmap beatmap)
        {
            config?.BindWith(OsuRulesetSetting.PlayfieldBorderStyle, playfieldBorder.PlayfieldBorderStyle);

            var osuBeatmap = (OsuBeatmap)beatmap;

            RegisterPool<HitCircle, DrawableHitCircle>(20, 100);

            // handle edge cases where a beatmap has a slider with many repeats.
            int maxRepeatsOnOneSlider = 0;
            int maxTicksOnOneSlider = 0;

            if (osuBeatmap != null)
            {
                foreach (var slider in osuBeatmap.HitObjects.OfType<Slider>())
                {
                    maxRepeatsOnOneSlider = Math.Max(maxRepeatsOnOneSlider, slider.RepeatCount);
                    maxTicksOnOneSlider = Math.Max(maxTicksOnOneSlider, slider.NestedHitObjects.OfType<SliderTick>().Count());
                }
            }

            RegisterPool<Slider, DrawableSlider>(20, 100);
            RegisterPool<SliderHeadCircle, DrawableSliderHead>(20, 100);
            RegisterPool<SliderTailCircle, DrawableSliderTail>(20, 100);
            RegisterPool<SliderTick, DrawableSliderTick>(Math.Max(maxTicksOnOneSlider, 20), Math.Max(maxTicksOnOneSlider, 200));
            RegisterPool<SliderRepeat, DrawableSliderRepeat>(Math.Max(maxRepeatsOnOneSlider, 20), Math.Max(maxRepeatsOnOneSlider, 200));

            RegisterPool<Spinner, DrawableSpinner>(2, 20);
            RegisterPool<SpinnerTick, DrawableSpinnerTick>(10, 200);
            RegisterPool<SpinnerBonusTick, DrawableSpinnerBonusTick>(10, 200);
        }

        protected override HitObjectLifetimeEntry CreateLifetimeEntry(HitObject hitObject) => new OsuHitObjectLifetimeEntry(hitObject);

        protected override void OnHitObjectAdded(HitObject hitObject)
        {
            base.OnHitObjectAdded(hitObject);
            FollowPoints.AddFollowPoints((OsuHitObject)hitObject);
        }

        protected override void OnHitObjectRemoved(HitObject hitObject)
        {
            base.OnHitObjectRemoved(hitObject);
            FollowPoints.RemoveFollowPoints((OsuHitObject)hitObject);
        }

        private void onNewResult(DrawableHitObject judgedObject, JudgementResult result)
        {
            // Hitobjects that block future hits should miss previous hitobjects if they're hit out-of-order.
            hitPolicy.HandleHit(judgedObject);

            if (!judgedObject.DisplayResult || !DisplayJudgements.Value)
                return;

            var explosion = judgementPooler.Get(result.Type, doj => doj.Apply(result, judgedObject));

            if (explosion == null)
                return;

            judgementLayer.Add(explosion);

            // the proxied content is added to judgementAboveHitObjectLayer once, on first load, and never removed from it.
            // ensure that ordering is consistent with expectations (latest judgement should be front-most).
            judgementAboveHitObjectLayer.ChangeChildDepth(explosion.ProxiedAboveHitObjectsContent, (float)-result.TimeAbsolute);
        }

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => HitObjectContainer.ReceivePositionalInputAt(screenSpacePos);

        private partial class ProxyContainer : LifetimeManagementContainer
        {
            public void Add(Drawable proxy) => AddInternal(proxy);
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
