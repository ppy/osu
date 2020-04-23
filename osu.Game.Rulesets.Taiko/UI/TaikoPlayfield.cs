// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.UI;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Rulesets.Taiko.Objects.Drawables;
using osu.Game.Rulesets.Taiko.Judgements;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Taiko.UI
{
    public class TaikoPlayfield : ScrollingPlayfield
    {
        private readonly ControlPointInfo controlPoints;

        /// <summary>
        /// Default height of a <see cref="TaikoPlayfield"/> when inside a <see cref="DrawableTaikoRuleset"/>.
        /// </summary>
        public const float DEFAULT_HEIGHT = 178;

        /// <summary>
        /// The size of the left area of the playfield. This area contains the input drum.
        /// </summary>
        private const float left_area_size = 180;

        private Container<HitExplosion> hitExplosionContainer;
        private Container<KiaiHitExplosion> kiaiExplosionContainer;
        private JudgementContainer<DrawableTaikoJudgement> judgementContainer;
        internal Drawable HitTarget;

        private ProxyContainer topLevelHitContainer;
        private ProxyContainer barlineContainer;
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
            InternalChildren = new Drawable[]
            {
                new SkinnableDrawable(new TaikoSkinComponent(TaikoSkinComponents.PlayfieldBackgroundRight), _ => new PlayfieldBackground()),
                rightArea = new Container
                {
                    Name = "Right area",
                    RelativeSizeAxes = Axes.Both,
                    RelativePositionAxes = Axes.Both,
                    Masking = true,
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
                                    Blending = BlendingParameters.Additive,
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
                                barlineContainer = new ProxyContainer
                                {
                                    RelativeSizeAxes = Axes.Both,
                                },
                                new Container
                                {
                                    Name = "Hit objects",
                                    RelativeSizeAxes = Axes.Both,
                                    Child = HitObjectContainer
                                },
                                kiaiExplosionContainer = new Container<KiaiHitExplosion>
                                {
                                    Name = "Kiai hit explosions",
                                    RelativeSizeAxes = Axes.Both,
                                    FillMode = FillMode.Fit,
                                    Blending = BlendingParameters.Additive
                                },
                                judgementContainer = new JudgementContainer<DrawableTaikoJudgement>
                                {
                                    Name = "Judgements",
                                    RelativeSizeAxes = Axes.Y,
                                    Blending = BlendingParameters.Additive
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
                    Size = new Vector2(left_area_size, 1),
                    Children = new Drawable[]
                    {
                        new SkinnableDrawable(new TaikoSkinComponent(TaikoSkinComponents.PlayfieldBackgroundLeft), _ => new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    Colour = colours.Gray1,
                                    RelativeSizeAxes = Axes.Both,
                                },
                                new Box
                                {
                                    Anchor = Anchor.TopRight,
                                    RelativeSizeAxes = Axes.Y,
                                    Width = 10,
                                    Colour = Framework.Graphics.Colour.ColourInfo.GradientHorizontal(Color4.Black.Opacity(0.6f), Color4.Black.Opacity(0)),
                                },
                            }
                        }),
                        new InputDrum(controlPoints)
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                        },
                    }
                },
                topLevelHitContainer = new ProxyContainer
                {
                    Name = "Top level hit objects",
                    RelativeSizeAxes = Axes.Both,
                }
            };
        }

        protected override void Update()
        {
            base.Update();

            // Padding is required to be updated for elements which are based on "absolute" X sized elements.
            // This is basically allowing for correct alignment as relative pieces move around them.
            rightArea.Padding = new MarginPadding { Left = leftArea.DrawWidth };
            hitTargetOffsetContent.Padding = new MarginPadding { Left = HitTarget.DrawWidth / 2 };
        }

        public override void Add(DrawableHitObject h)
        {
            h.OnNewResult += OnNewResult;

            base.Add(h);

            switch (h)
            {
                case DrawableBarLine barline:
                    barlineContainer.Add(barline.CreateProxy());
                    break;

                case DrawableTaikoHitObject taikoObject:
                    topLevelHitContainer.Add(taikoObject.CreateProxiedContent());
                    break;
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

                default:
                    judgementContainer.Add(new DrawableTaikoJudgement(result, judgedObject)
                    {
                        Anchor = result.IsHit ? Anchor.TopLeft : Anchor.CentreLeft,
                        Origin = result.IsHit ? Anchor.BottomCentre : Anchor.Centre,
                        RelativePositionAxes = Axes.X,
                        X = result.IsHit ? judgedObject.Position.X : 0,
                    });

                    if (!result.IsHit)
                        break;

                    bool isRim = (judgedObject.HitObject as Hit)?.Type == HitType.Rim;

                    hitExplosionContainer.Add(new HitExplosion(judgedObject, isRim));

                    if (judgedObject.HitObject.Kiai)
                        kiaiExplosionContainer.Add(new KiaiHitExplosion(judgedObject, isRim));

                    break;
            }
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
