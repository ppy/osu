// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.UI;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.Objects.Drawables;
using osu.Game.Rulesets.Taiko.Judgements;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Taiko.UI
{
    public class TaikoPlayfield : ScrollingPlayfield
    {
        /// <summary>
        /// Default height of a <see cref="TaikoPlayfield"/> when inside a <see cref="DrawableTaikoRuleset"/>.
        /// </summary>
        public const float DEFAULT_HEIGHT = 178;

        /// <summary>
        /// The offset from <see cref="left_area_size"/> which the center of the hit target lies at.
        /// </summary>
        public const float HIT_TARGET_OFFSET = 100;

        /// <summary>
        /// The size of the left area of the playfield. This area contains the input drum.
        /// </summary>
        private const float left_area_size = 240;

        private readonly Container<HitExplosion> hitExplosionContainer;
        private readonly Container<KiaiHitExplosion> kiaiExplosionContainer;
        private readonly JudgementContainer<DrawableTaikoJudgement> judgementContainer;
        internal readonly HitTarget HitTarget;

        private readonly ProxyContainer topLevelHitContainer;
        private readonly ProxyContainer barlineContainer;

        private readonly Container overlayBackgroundContainer;
        private readonly Container backgroundContainer;

        private readonly Box overlayBackground;
        private readonly Box background;

        public TaikoPlayfield(ControlPointInfo controlPoints)
        {
            InternalChildren = new Drawable[]
            {
                backgroundContainer = new Container
                {
                    Name = "Transparent playfield background",
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    EdgeEffect = new EdgeEffectParameters
                    {
                        Type = EdgeEffectType.Shadow,
                        Colour = Color4.Black.Opacity(0.2f),
                        Radius = 5,
                    },
                    Children = new Drawable[]
                    {
                        background = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Alpha = 0.6f
                        },
                    }
                },
                new Container
                {
                    Name = "Right area",
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Left = left_area_size },
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            Name = "Masked elements before hit objects",
                            RelativeSizeAxes = Axes.Both,
                            Padding = new MarginPadding { Left = HIT_TARGET_OFFSET },
                            Masking = true,
                            Children = new Drawable[]
                            {
                                hitExplosionContainer = new Container<HitExplosion>
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    FillMode = FillMode.Fit,
                                    Blending = BlendingParameters.Additive,
                                },
                                HitTarget = new HitTarget
                                {
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.Centre,
                                    RelativeSizeAxes = Axes.Both,
                                    FillMode = FillMode.Fit
                                }
                            }
                        },
                        barlineContainer = new ProxyContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            Padding = new MarginPadding { Left = HIT_TARGET_OFFSET }
                        },
                        new Container
                        {
                            Name = "Hit objects",
                            RelativeSizeAxes = Axes.Both,
                            Padding = new MarginPadding { Left = HIT_TARGET_OFFSET },
                            Masking = true,
                            Child = HitObjectContainer
                        },
                        kiaiExplosionContainer = new Container<KiaiHitExplosion>
                        {
                            Name = "Kiai hit explosions",
                            RelativeSizeAxes = Axes.Both,
                            FillMode = FillMode.Fit,
                            Margin = new MarginPadding { Left = HIT_TARGET_OFFSET },
                            Blending = BlendingParameters.Additive
                        },
                        judgementContainer = new JudgementContainer<DrawableTaikoJudgement>
                        {
                            Name = "Judgements",
                            RelativeSizeAxes = Axes.Y,
                            Margin = new MarginPadding { Left = HIT_TARGET_OFFSET },
                            Blending = BlendingParameters.Additive
                        },
                    }
                },
                overlayBackgroundContainer = new Container
                {
                    Name = "Left overlay",
                    RelativeSizeAxes = Axes.Y,
                    Size = new Vector2(left_area_size, 1),
                    Children = new Drawable[]
                    {
                        overlayBackground = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                        },
                        new InputDrum(controlPoints)
                        {
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreRight,
                            Scale = new Vector2(0.9f),
                            Margin = new MarginPadding { Right = 20 }
                        },
                        new Box
                        {
                            Anchor = Anchor.TopRight,
                            RelativeSizeAxes = Axes.Y,
                            Width = 10,
                            Colour = Framework.Graphics.Colour.ColourInfo.GradientHorizontal(Color4.Black.Opacity(0.6f), Color4.Black.Opacity(0)),
                        },
                    }
                },
                new Container
                {
                    Name = "Border",
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    MaskingSmoothness = 0,
                    BorderThickness = 2,
                    AlwaysPresent = true,
                    Children = new[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Alpha = 0,
                            AlwaysPresent = true
                        }
                    }
                },
                topLevelHitContainer = new ProxyContainer
                {
                    Name = "Top level hit objects",
                    RelativeSizeAxes = Axes.Both,
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            overlayBackgroundContainer.BorderColour = colours.Gray0;
            overlayBackground.Colour = colours.Gray1;

            backgroundContainer.BorderColour = colours.Gray1;
            background.Colour = colours.Gray0;
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

                    bool isRim = judgedObject.HitObject is RimHit;

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
