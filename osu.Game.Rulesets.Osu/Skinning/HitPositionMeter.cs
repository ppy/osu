// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Pooling;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Legacy;
using osu.Game.Rulesets.Osu.Judgements;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Screens.Play.HUD.HitErrorMeters;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Skinning
{
    public partial class HitPositionMeter : HitErrorMeter
    {
        private Container averagePositionContainer = null!;
        private Vector2 averagePosition = Vector2.Zero;

        private readonly DrawablePool<HitPosition> hitPositionPool = new DrawablePool<HitPosition>(20);
        private Container hitPositionsContainer = null!;

        private const float arrow_width = 3f;

        private float objectRadius;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        public HitPositionMeter()
        {
            AutoSizeAxes = Axes.Both;
            AlwaysPresent = true;
        }

        [BackgroundDependencyLoader]
        private void load(IBindable<WorkingBeatmap> beatmap)
        {
            InternalChild = new Container
            {
                Height = 100,
                Width = 100,
                Children = new Drawable[]
                {
                    hitPositionPool,
                    new CircularContainer
                    {
                        BorderColour = Colour4.White,
                        Masking = true,
                        BorderThickness = 2,
                        RelativeSizeAxes = Axes.Both,
                        Child = new Box
                        {
                            Colour = Colour4.Gray,
                            Alpha = 0.3f,
                            RelativeSizeAxes = Axes.Both
                        },
                    },
                    hitPositionsContainer = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre
                    },
                    averagePositionContainer = new Container
                    {
                        RelativePositionAxes = Axes.Both,
                        AutoSizeAxes = Axes.Both,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Children = new Drawable[]
                        {
                            new Circle
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Width = arrow_width,
                                Height = arrow_width * 4,
                            },
                            new Circle
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Width = arrow_width,
                                Height = arrow_width * 4,
                                Rotation = 90
                            }
                        }
                    }
                }
            };

            objectRadius = OsuHitObject.OBJECT_RADIUS * LegacyRulesetExtensions.CalculateScaleFromCircleSize(beatmap.Value.Beatmap.Difficulty.CircleSize, true);
        }

        protected override void OnNewJudgement(JudgementResult judgement)
        {
            if (judgement is not OsuHitCircleJudgementResult circleJudgement) return;

            if (circleJudgement.CursorPositionAtHit == null) return;

            var relativeHitPosition = (circleJudgement.CursorPositionAtHit.Value - ((OsuHitObject)circleJudgement.HitObject).StackedPosition) / objectRadius / 2;

            hitPositionPool.Get(drawableHit =>
            {
                drawableHit.X = relativeHitPosition.X;
                drawableHit.Y = relativeHitPosition.Y;
                drawableHit.Colour = getColourForPosition(relativeHitPosition);

                hitPositionsContainer.Add(drawableHit);
            });

            averagePositionContainer.MoveTo(averagePosition = (relativeHitPosition + averagePosition) / 2, 800, Easing.OutQuint);
        }

        private Color4 getColourForPosition(Vector2 position)
        {
            switch (Vector2.Distance(position, Vector2.Zero))
            {
                case >= 0.5f:
                    return colours.Red;

                case >= 0.35f:
                    return colours.Yellow;

                case >= 0.2f:
                    return colours.Green;

                default:
                    return colours.Blue;
            }
        }

        public override void Clear()
        {
            averagePositionContainer.MoveTo(averagePosition = Vector2.Zero, 800, Easing.OutQuint);
            hitPositionsContainer.Clear();
        }

        private partial class HitPosition : PoolableDrawable
        {
            private const float small_arrow_width = 1.5f;

            public HitPosition()
            {
                AutoSizeAxes = Axes.Both;
                RelativePositionAxes = Axes.Both;

                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;

                InternalChildren = new Drawable[]
                {
                    new Circle
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Width = small_arrow_width,
                        Height = small_arrow_width * 4,
                    },
                    new Circle
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Width = small_arrow_width,
                        Height = small_arrow_width * 4,
                        Rotation = 90
                    }
                };
            }

            protected override void PrepareForUse()
            {
                base.PrepareForUse();

                const int judgement_fade_in_duration = 100;
                const int judgement_fade_out_duration = 5000;

                Alpha = 0;

                this
                    .FadeTo(1f, judgement_fade_in_duration, Easing.OutQuint)
                    .Then()
                    .FadeOut(judgement_fade_out_duration)
                    .Expire();
            }
        }
    }
}
