// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Game.Graphics;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Rulesets.Taiko.Objects.Drawables;
using osu.Game.Rulesets.Taiko.Judgements;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.Scoring;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Taiko.UI
{
    public partial class TaikoPlayfield : ScrollingPlayfield
    {
        /// <summary>
        /// Base height of a <see cref="TaikoPlayfield"/> when inside a <see cref="DrawableTaikoRuleset"/>.
        /// </summary>
        public const float BASE_HEIGHT = 200;

        public const float INPUT_DRUM_WIDTH = 180f;

        public Container UnderlayElements { get; private set; } = null!;

        private Container<HitExplosion> hitExplosionContainer = null!;
        private Container<KiaiHitExplosion> kiaiExplosionContainer = null!;
        private JudgementContainer<DrawableTaikoJudgement> judgementContainer = null!;
        private ScrollingHitObjectContainer drumRollHitContainer = null!;
        internal Drawable HitTarget = null!;

        private JudgementPooler<DrawableTaikoJudgement> judgementPooler = null!;
        private readonly IDictionary<HitResult, HitExplosionPool> explosionPools = new Dictionary<HitResult, HitExplosionPool>();

        private ProxyContainer topLevelHitContainer = null!;
        private InputDrum inputDrum = null!;

        /// <remarks>
        /// <see cref="Playfield.AddNested"/> is purposefully not called on this to prevent i.e. being able to interact
        /// with bar lines in the editor.
        /// </remarks>
        private BarLinePlayfield barLinePlayfield = null!;

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            const float hit_target_width = BASE_HEIGHT;
            const float hit_target_offset = -24f;

            inputDrum = new InputDrum
            {
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
                RelativeSizeAxes = Axes.Y,
                Width = INPUT_DRUM_WIDTH,
            };

            InternalChildren = new[]
            {
                new SkinnableDrawable(new TaikoSkinComponentLookup(TaikoSkinComponents.PlayfieldBackgroundRight), _ => new PlayfieldBackgroundRight()),
                new Container
                {
                    Name = "Left overlay",
                    RelativeSizeAxes = Axes.Y,
                    Width = INPUT_DRUM_WIDTH,
                    BorderColour = colours.Gray0,
                    Children = new[]
                    {
                        new SkinnableDrawable(new TaikoSkinComponentLookup(TaikoSkinComponents.PlayfieldBackgroundLeft), _ => new PlayfieldBackgroundLeft()),
                        inputDrum.CreateProxy(),
                    }
                },
                new SkinnableDrawable(new TaikoSkinComponentLookup(TaikoSkinComponents.Mascot), _ => Empty())
                {
                    Origin = Anchor.BottomLeft,
                    Anchor = Anchor.TopLeft,
                    RelativePositionAxes = Axes.Y,
                    RelativeSizeAxes = Axes.None,
                    Y = 0.2f
                },
                new Container
                {
                    Name = "Right area",
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Left = INPUT_DRUM_WIDTH },
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            Name = "Elements behind hit objects",
                            RelativeSizeAxes = Axes.Y,
                            Width = hit_target_width,
                            X = hit_target_offset,
                            Children = new[]
                            {
                                new SkinnableDrawable(new TaikoSkinComponentLookup(TaikoSkinComponents.KiaiGlow), _ => Empty())
                                {
                                    RelativeSizeAxes = Axes.Both,
                                },
                                hitExplosionContainer = new Container<HitExplosion>
                                {
                                    RelativeSizeAxes = Axes.Both,
                                },
                                HitTarget = new SkinnableDrawable(new TaikoSkinComponentLookup(TaikoSkinComponents.HitTarget), _ => new TaikoHitTarget())
                                {
                                    RelativeSizeAxes = Axes.Both,
                                }
                            }
                        },
                        new Container
                        {
                            Name = "Bar line content",
                            RelativeSizeAxes = Axes.Both,
                            Padding = new MarginPadding { Left = hit_target_width / 2 + hit_target_offset },
                            Children = new Drawable[]
                            {
                                UnderlayElements = new Container
                                {
                                    RelativeSizeAxes = Axes.Both,
                                },
                                barLinePlayfield = new BarLinePlayfield(),
                            }
                        },
                        new Container
                        {
                            Name = "Masked hit objects content",
                            RelativeSizeAxes = Axes.Both,
                            Padding = new MarginPadding { Left = hit_target_width / 2 + hit_target_offset },
                            Masking = true,
                            Child = HitObjectContainer,
                        },
                        new Container
                        {
                            Name = "Overlay content",
                            RelativeSizeAxes = Axes.Both,
                            Padding = new MarginPadding { Left = hit_target_width / 2 + hit_target_offset },
                            Children = new Drawable[]
                            {
                                drumRollHitContainer = new DrumRollHitContainer(),
                                kiaiExplosionContainer = new Container<KiaiHitExplosion>
                                {
                                    Name = "Kiai hit explosions",
                                    Origin = Anchor.TopCentre,
                                    RelativeSizeAxes = Axes.Both,
                                    FillMode = FillMode.Fit,
                                },
                                judgementContainer = new JudgementContainer<DrawableTaikoJudgement>
                                {
                                    Name = "Judgements",
                                    Origin = Anchor.TopCentre,
                                    RelativeSizeAxes = Axes.Both,
                                    FillMode = FillMode.Fit,
                                },
                            }
                        },
                    }
                },
                topLevelHitContainer = new ProxyContainer
                {
                    Name = "Top level hit objects",
                    RelativeSizeAxes = Axes.Both,
                },
                drumRollHitContainer.CreateProxy(),
                new SkinnableDrawable(new TaikoSkinComponentLookup(TaikoSkinComponents.DrumSamplePlayer), _ => new DrumSamplePlayer())
                {
                    RelativeSizeAxes = Axes.Both,
                },
                // this is added at the end of the hierarchy to receive input before taiko objects.
                // but is proxied below everything to not cover visual effects such as hit explosions.
                inputDrum,
            };

            RegisterPool<Hit, DrawableHit>(50);
            RegisterPool<Hit.StrongNestedHit, DrawableHit.StrongNestedHit>(50);

            RegisterPool<DrumRoll, DrawableDrumRoll>(5);
            RegisterPool<DrumRoll.StrongNestedHit, DrawableDrumRoll.StrongNestedHit>(5);

            RegisterPool<DrumRollTick, DrawableDrumRollTick>(100);
            RegisterPool<DrumRollTick.StrongNestedHit, DrawableDrumRollTick.StrongNestedHit>(100);

            RegisterPool<Swell, DrawableSwell>(5);
            RegisterPool<SwellTick, DrawableSwellTick>(100);

            var hitWindows = new TaikoHitWindows();

            HitResult[] usableHitResults = Enum.GetValues<HitResult>().Where(r => hitWindows.IsHitResultAllowed(r)).ToArray();

            AddInternal(judgementPooler = new JudgementPooler<DrawableTaikoJudgement>(usableHitResults));

            foreach (var result in usableHitResults)
                explosionPools.Add(result, new HitExplosionPool(result));
            AddRangeInternal(explosionPools.Values);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            NewResult += OnNewResult;
        }

        protected override void OnNewDrawableHitObject(DrawableHitObject drawableHitObject)
        {
            base.OnNewDrawableHitObject(drawableHitObject);

            var taikoObject = (DrawableTaikoHitObject)drawableHitObject;
            topLevelHitContainer.Add(taikoObject.CreateProxiedContent());
        }

        #region Pooling support

        public override void Add(HitObject h)
        {
            switch (h)
            {
                case BarLine barLine:
                    barLinePlayfield.Add(barLine);
                    break;

                case TaikoHitObject taikoHitObject:
                    base.Add(taikoHitObject);
                    break;

                default:
                    throw new ArgumentException($"Unsupported {nameof(HitObject)} type: {h.GetType()}");
            }
        }

        public override bool Remove(HitObject h)
        {
            switch (h)
            {
                case BarLine barLine:
                    return barLinePlayfield.Remove(barLine);

                case TaikoHitObject taikoHitObject:
                    return base.Remove(taikoHitObject);

                default:
                    throw new ArgumentException($"Unsupported {nameof(HitObject)} type: {h.GetType()}");
            }
        }

        #endregion

        #region Non-pooling support

        public override void Add(DrawableHitObject h)
        {
            switch (h)
            {
                case DrawableBarLine barLine:
                    barLinePlayfield.Add(barLine);
                    break;

                case DrawableTaikoHitObject:
                    base.Add(h);
                    break;

                default:
                    throw new ArgumentException($"Unsupported {nameof(DrawableHitObject)} type: {h.GetType()}");
            }
        }

        public override bool Remove(DrawableHitObject h)
        {
            switch (h)
            {
                case DrawableBarLine barLine:
                    return barLinePlayfield.Remove(barLine);

                case DrawableTaikoHitObject:
                    return base.Remove(h);

                default:
                    throw new ArgumentException($"Unsupported {nameof(DrawableHitObject)} type: {h.GetType()}");
            }
        }

        #endregion

        internal void OnNewResult(DrawableHitObject judgedObject, JudgementResult result)
        {
            if (!DisplayJudgements.Value)
                return;
            if (!judgedObject.DisplayResult)
                return;

            switch (result.Judgement)
            {
                case TaikoStrongJudgement:
                    if (result.IsHit)
                        hitExplosionContainer.Children.FirstOrDefault(e => e.JudgedObject == ((DrawableStrongNestedHit)judgedObject).ParentHitObject)?.VisualiseSecondHit(result);
                    break;

                case TaikoDrumRollTickJudgement:
                    if (!result.IsHit)
                        break;

                    var drawableTick = (DrawableDrumRollTick)judgedObject;

                    addDrumRollHit(drawableTick);
                    break;

                default:
                    if (!result.Type.IsScorable())
                        break;

                    var judgement = judgementPooler.Get(result.Type, j => j.Apply(result, judgedObject));

                    if (judgement == null)
                        return;

                    judgementContainer.Add(judgement);

                    var type = (judgedObject.HitObject as Hit)?.Type ?? HitType.Centre;
                    addExplosion(judgedObject, result.Type, type);
                    break;
            }
        }

        private void addDrumRollHit(DrawableDrumRollTick drawableTick) =>
            drumRollHitContainer.Add(new DrawableFlyingHit(drawableTick));

        private void addExplosion(DrawableHitObject drawableObject, HitResult result, HitType type)
        {
            hitExplosionContainer.Add(explosionPools[result]
                .Get(explosion => explosion.Apply(drawableObject)));
            if (drawableObject.HitObject.Kiai)
                kiaiExplosionContainer.Add(new KiaiHitExplosion(drawableObject, type));
        }

        private partial class ProxyContainer : LifetimeManagementContainer
        {
            public void Add(Drawable proxy) => AddInternal(proxy);

            // DrawableHitObject disables masking.
            // Hitobject content is proxied and unproxied based on hit status and the IsMaskedAway value could get stuck because of this.
            protected override bool UpdateChildrenMasking => false;

            protected override bool ComputeIsMaskedAway(RectangleF maskingBounds) => false;
        }
    }
}
