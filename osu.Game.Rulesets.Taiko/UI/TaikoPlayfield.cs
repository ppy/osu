// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Pooling;
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
    public class TaikoPlayfield : ScrollingPlayfield
    {
        /// <summary>
        /// Default height of a <see cref="TaikoPlayfield"/> when inside a <see cref="DrawableTaikoRuleset"/>.
        /// </summary>
        public const float DEFAULT_HEIGHT = 212;

        private Container<HitExplosion> hitExplosionContainer;
        private Container<KiaiHitExplosion> kiaiExplosionContainer;
        private JudgementContainer<DrawableTaikoJudgement> judgementContainer;
        private ScrollingHitObjectContainer drumRollHitContainer;
        internal Drawable HitTarget;
        private SkinnableDrawable mascot;

        private readonly IDictionary<HitResult, DrawablePool<DrawableTaikoJudgement>> judgementPools = new Dictionary<HitResult, DrawablePool<DrawableTaikoJudgement>>();
        private readonly IDictionary<HitResult, HitExplosionPool> explosionPools = new Dictionary<HitResult, HitExplosionPool>();

        private ProxyContainer topLevelHitContainer;
        private Container rightArea;
        private Container leftArea;

        /// <remarks>
        /// <see cref="Playfield.AddNested"/> is purposefully not called on this to prevent i.e. being able to interact
        /// with bar lines in the editor.
        /// </remarks>
        private BarLinePlayfield barLinePlayfield;

        private Container hitTargetOffsetContent;

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            InternalChildren = new[]
            {
                new SkinnableDrawable(new TaikoSkinComponent(TaikoSkinComponents.PlayfieldBackgroundRight), _ => new PlayfieldBackgroundRight()),
                rightArea = new Container
                {
                    Name = "Right area",
                    RelativeSizeAxes = Axes.Both,
                    RelativePositionAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            Name = "Masked elements before hit objects",
                            RelativeSizeAxes = Axes.Both,
                            FillMode = FillMode.Fit,
                            Children = new[]
                            {
                                hitExplosionContainer = new Container<HitExplosion>
                                {
                                    RelativeSizeAxes = Axes.Both,
                                },
                                HitTarget = new SkinnableDrawable(new TaikoSkinComponent(TaikoSkinComponents.HitTarget), _ => new TaikoHitTarget())
                                {
                                    RelativeSizeAxes = Axes.Both,
                                }
                            }
                        },
                        hitTargetOffsetContent = new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Children = new Drawable[]
                            {
                                barLinePlayfield = new BarLinePlayfield(),
                                new Container
                                {
                                    Name = "Hit objects",
                                    RelativeSizeAxes = Axes.Both,
                                    Children = new Drawable[]
                                    {
                                        HitObjectContainer,
                                        drumRollHitContainer = new DrumRollHitContainer()
                                    }
                                },
                                kiaiExplosionContainer = new Container<KiaiHitExplosion>
                                {
                                    Name = "Kiai hit explosions",
                                    RelativeSizeAxes = Axes.Both,
                                    FillMode = FillMode.Fit,
                                },
                                judgementContainer = new JudgementContainer<DrawableTaikoJudgement>
                                {
                                    Name = "Judgements",
                                    RelativeSizeAxes = Axes.Y,
                                },
                            }
                        },
                    }
                },
                leftArea = new Container
                {
                    Name = "Left overlay",
                    RelativeSizeAxes = Axes.Both,
                    FillMode = FillMode.Fit,
                    BorderColour = colours.Gray0,
                    Children = new Drawable[]
                    {
                        new SkinnableDrawable(new TaikoSkinComponent(TaikoSkinComponents.PlayfieldBackgroundLeft), _ => new PlayfieldBackgroundLeft()),
                        new InputDrum(HitObjectContainer)
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                        },
                    }
                },
                mascot = new SkinnableDrawable(new TaikoSkinComponent(TaikoSkinComponents.Mascot), _ => Empty())
                {
                    Origin = Anchor.BottomLeft,
                    Anchor = Anchor.TopLeft,
                    RelativePositionAxes = Axes.Y,
                    RelativeSizeAxes = Axes.None,
                    Y = 0.2f
                },
                topLevelHitContainer = new ProxyContainer
                {
                    Name = "Top level hit objects",
                    RelativeSizeAxes = Axes.Both,
                },
                drumRollHitContainer.CreateProxy(),
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

            foreach (var result in Enum.GetValues(typeof(HitResult)).OfType<HitResult>().Where(r => hitWindows.IsHitResultAllowed(r)))
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
            rightArea.Padding = new MarginPadding { Left = leftArea.DrawWidth };
            hitTargetOffsetContent.Padding = new MarginPadding { Left = HitTarget.DrawWidth / 2 };

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

                case DrawableTaikoHitObject _:
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

                case DrawableTaikoHitObject _:
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
                case TaikoStrongJudgement _:
                    if (result.IsHit)
                        hitExplosionContainer.Children.FirstOrDefault(e => e.JudgedObject == ((DrawableStrongNestedHit)judgedObject).ParentHitObject)?.VisualiseSecondHit(result);
                    break;

                case TaikoDrumRollTickJudgement _:
                    if (!result.IsHit)
                        break;

                    var drawableTick = (DrawableDrumRollTick)judgedObject;

                    addDrumRollHit(drawableTick);
                    break;

                default:
                    judgementContainer.Add(judgementPools[result.Type].Get(j =>
                    {
                        j.Apply(result, judgedObject);

                        j.Anchor = result.IsHit ? Anchor.TopLeft : Anchor.CentreLeft;
                        j.Origin = result.IsHit ? Anchor.BottomCentre : Anchor.Centre;
                        j.RelativePositionAxes = Axes.X;
                        j.X = result.IsHit ? judgedObject.Position.X : 0;
                    }));

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

        private class ProxyContainer : LifetimeManagementContainer
        {
            public new MarginPadding Padding
            {
                set => base.Padding = value;
            }

            public void Add(Drawable proxy) => AddInternal(proxy);
        }
    }
}
