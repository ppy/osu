// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Rulesets.Taiko.Objects.Drawables;
using osu.Game.Rulesets.Taiko.Judgements;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Taiko.UI
{
    public class TaikoPlayfield : ScrollingPlayfield
    {
        private readonly ControlPointInfo controlPoints;

        /// <summary>
        /// Default height of a <see cref="TaikoPlayfield"/> when inside a <see cref="DrawableTaikoRuleset"/>.
        /// </summary>
        public const float DEFAULT_HEIGHT = 178;

        private Container<HitExplosion> hitExplosionContainer;
        private Container<KiaiHitExplosion> kiaiExplosionContainer;
        private JudgementContainer<DrawableTaikoJudgement> judgementContainer;
        private ScrollingHitObjectContainer drumRollHitContainer;
        internal Drawable HitTarget;
        private SkinnableDrawable mascot;

        private ProxyContainer topLevelHitContainer;
        private ScrollingHitObjectContainer barlineContainer;
        private Container rightArea;
        private Container leftArea;

        private Container hitTargetOffsetContent;

        public TaikoPlayfield(ControlPointInfo controlPoints)
        {
            this.controlPoints = controlPoints;
        }

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
                                barlineContainer = new ScrollingHitObjectContainer(),
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
                        new InputDrum(controlPoints)
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

        public override void Add(DrawableHitObject h)
        {
            switch (h)
            {
                case DrawableBarLine barline:
                    barlineContainer.Add(barline);
                    break;

                case DrawableTaikoHitObject taikoObject:
                    h.OnNewResult += OnNewResult;
                    topLevelHitContainer.Add(taikoObject.CreateProxiedContent());
                    base.Add(h);
                    break;

                default:
                    throw new ArgumentException($"Unsupported {nameof(DrawableHitObject)} type");
            }
        }

        public override bool Remove(DrawableHitObject h)
        {
            switch (h)
            {
                case DrawableBarLine barline:
                    return barlineContainer.Remove(barline);

                case DrawableTaikoHitObject _:
                    h.OnNewResult -= OnNewResult;
                    // todo: consider tidying of proxied content if required.
                    return base.Remove(h);

                default:
                    throw new ArgumentException($"Unsupported {nameof(DrawableHitObject)} type");
            }
        }

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
                        hitExplosionContainer.Children.FirstOrDefault(e => e.JudgedObject == ((DrawableStrongNestedHit)judgedObject).MainObject)?.VisualiseSecondHit();
                    break;

                case TaikoDrumRollTickJudgement _:
                    if (!result.IsHit)
                        break;

                    var drawableTick = (DrawableDrumRollTick)judgedObject;

                    addDrumRollHit(drawableTick);
                    break;

                default:
                    judgementContainer.Add(new DrawableTaikoJudgement(result, judgedObject)
                    {
                        Anchor = result.IsHit ? Anchor.TopLeft : Anchor.CentreLeft,
                        Origin = result.IsHit ? Anchor.BottomCentre : Anchor.Centre,
                        RelativePositionAxes = Axes.X,
                        X = result.IsHit ? judgedObject.Position.X : 0,
                    });

                    var type = (judgedObject.HitObject as Hit)?.Type ?? HitType.Centre;
                    addExplosion(judgedObject, result.Type, type);
                    break;
            }
        }

        private void addDrumRollHit(DrawableDrumRollTick drawableTick) =>
            drumRollHitContainer.Add(new DrawableFlyingHit(drawableTick));

        private void addExplosion(DrawableHitObject drawableObject, HitResult result, HitType type)
        {
            hitExplosionContainer.Add(new HitExplosion(drawableObject, result));
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
