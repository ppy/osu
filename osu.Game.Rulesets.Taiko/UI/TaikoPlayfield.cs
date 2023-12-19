// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Pooling;
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
using osuTK;

namespace osu.Game.Rulesets.Taiko.UI
{
    public partial class TaikoPlayfield : ScrollingPlayfield
    {
        /// <summary>
        /// Default height of a <see cref="TaikoPlayfield"/> when inside a <see cref="DrawableTaikoRuleset"/>.
        /// </summary>
        public const float DEFAULT_HEIGHT = 200;

        /// <summary>
        /// Whether the hit target should be nudged further towards the left area, matching the stable "classic" position.
        /// </summary>
        public Bindable<bool> ClassicHitTargetPosition = new BindableBool();

        public Container UnderlayElements { get; private set; } = null!;

        private Container<HitExplosion> hitExplosionContainer;
        private Container<KiaiHitExplosion> kiaiExplosionContainer;
        private JudgementContainer<DrawableTaikoJudgement> judgementContainer;
        private ScrollingHitObjectContainer drumRollHitContainer;
        internal Drawable HitTarget;
        private SkinnableDrawable mascot;

        private readonly IDictionary<HitResult, DrawablePool<DrawableTaikoJudgement>> judgementPools = new Dictionary<HitResult, DrawablePool<DrawableTaikoJudgement>>();
        private readonly IDictionary<HitResult, HitExplosionPool> explosionPools = new Dictionary<HitResult, HitExplosionPool>();

        private ProxyContainer topLevelHitContainer;
        private InputDrum inputDrum;
        private Container rightArea;

        /// <remarks>
        /// <see cref="Playfield.AddNested"/> is purposefully not called on this to prevent i.e. being able to interact
        /// with bar lines in the editor.
        /// </remarks>
        private BarLinePlayfield barLinePlayfield;

        private Container barLineContent;
        private Container hitObjectContent;
        private Container overlayContent;

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            inputDrum = new InputDrum
            {
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
                AutoSizeAxes = Axes.X,
                RelativeSizeAxes = Axes.Y,
            };

            InternalChildren = new[]
            {
                new SkinnableDrawable(new TaikoSkinComponentLookup(TaikoSkinComponents.PlayfieldBackgroundRight), _ => new PlayfieldBackgroundRight()),
                new Container
                {
                    Name = "Left overlay",
                    RelativeSizeAxes = Axes.Both,
                    FillMode = FillMode.Fit,
                    BorderColour = colours.Gray0,
                    Children = new[]
                    {
                        new SkinnableDrawable(new TaikoSkinComponentLookup(TaikoSkinComponents.PlayfieldBackgroundLeft), _ => new PlayfieldBackgroundLeft()),
                        inputDrum.CreateProxy(),
                    }
                },
                mascot = new SkinnableDrawable(new TaikoSkinComponentLookup(TaikoSkinComponents.Mascot), _ => Empty())
                {
                    Origin = Anchor.BottomLeft,
                    Anchor = Anchor.TopLeft,
                    RelativePositionAxes = Axes.Y,
                    RelativeSizeAxes = Axes.None,
                    Y = 0.2f
                },
                rightArea = new Container
                {
                    Name = "Right area",
                    RelativeSizeAxes = Axes.Both,
                    RelativePositionAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            Name = "Elements before hit objects",
                            RelativeSizeAxes = Axes.Both,
                            FillMode = FillMode.Fit,
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
                        barLineContent = new Container
                        {
                            Name = "Bar line content",
                            RelativeSizeAxes = Axes.Both,
                            Children = new Drawable[]
                            {
                                UnderlayElements = new Container
                                {
                                    RelativeSizeAxes = Axes.Both,
                                },
                                barLinePlayfield = new BarLinePlayfield(),
                            }
                        },
                        hitObjectContent = new Container
                        {
                            Name = "Masked hit objects content",
                            RelativeSizeAxes = Axes.Both,
                            Masking = true,
                            Child = HitObjectContainer,
                        },
                        overlayContent = new Container
                        {
                            Name = "Elements after hit objects",
                            RelativeSizeAxes = Axes.Both,
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

            foreach (var result in Enum.GetValues<HitResult>().Where(r => hitWindows.IsHitResultAllowed(r)))
            {
                judgementPools.Add(result, new DrawablePool<DrawableTaikoJudgement>(15));
                explosionPools.Add(result, new HitExplosionPool(result));
            }

            AddRangeInternal(judgementPools.Values);
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

        protected override void Update()
        {
            base.Update();

            // Padding is required to be updated for elements which are based on "absolute" X sized elements.
            // This is basically allowing for correct alignment as relative pieces move around them.
            rightArea.Padding = new MarginPadding { Left = inputDrum.Width };
            barLineContent.Padding = new MarginPadding { Left = HitTarget.DrawWidth / 2 };
            hitObjectContent.Padding = new MarginPadding { Left = HitTarget.DrawWidth / 2 };
            overlayContent.Padding = new MarginPadding { Left = HitTarget.DrawWidth / 2 };

            mascot.Scale = new Vector2(DrawHeight / DEFAULT_HEIGHT);
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
                    {
                        var hitObject = ((DrawableStrongNestedHit)judgedObject).ParentHitObject;

                        hitExplosionContainer.Children.FirstOrDefault(e => e.JudgedObject == hitObject)?.VisualiseSecondHit(result);
                        (judgementContainer.Children.FirstOrDefault(e => e.JudgedObject == hitObject) as IVisualiseSecondHit)?.VisualiseSecondHit(result);
                    }

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

                    judgementContainer.Add(judgementPools[result.Type].Get(j => j.Apply(result, judgedObject)));

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

            public override bool UpdateSubTreeMasking(Drawable source, RectangleF maskingBounds)
            {
                // DrawableHitObject disables masking.
                // Hitobject content is proxied and unproxied based on hit status and the IsMaskedAway value could get stuck because of this.
                return false;
            }
        }
    }
}
