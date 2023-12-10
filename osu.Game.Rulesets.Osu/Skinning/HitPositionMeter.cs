// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
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
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Play;
using osu.Game.Screens.Play.HUD.HitErrorMeters;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Skinning
{
    public partial class HitPositionMeter : HitErrorMeter
    {
        [Resolved]
        private ScoreProcessor processor { get; set; } = null!;

        private Container averagePositionContainer = null!;
        private Vector2 averagePosition = Vector2.Zero;

        private readonly DrawablePool<HitPosition> hitPosisionPool = new DrawablePool<HitPosition>(20);
        private Container hitPosisionsContainer = null!;

        private const float arrow_width = 3f;

        private float objectRadis;

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
                    hitPosisionPool,
                    new Circle
                    {
                        RelativeSizeAxes = Axes.Both,
                        Alpha = 0.3f,
                        Colour = Colour4.Gray
                    },
                    hitPosisionsContainer = new Container
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

            objectRadis = OsuHitObject.OBJECT_RADIUS * LegacyRulesetExtensions.CalculateScaleFromCircleSize(beatmap.Value.Beatmap.Difficulty.CircleSize, true);
        }

        protected override void OnNewJudgement(JudgementResult _)
        {
            var lastHit = processor.HitEvents.Last();
            if (lastHit.Position == null) return;

            var relativeHitPosition = (lastHit.Position.Value - ((OsuHitObject)lastHit.HitObject).StackedPosition) / objectRadis / 2;

            hitPosisionPool.Get(drawableHit =>
            {
                drawableHit.X = relativeHitPosition.X;
                drawableHit.Y = relativeHitPosition.Y;
                drawableHit.Colour = getColourForPosition(relativeHitPosition);

                hitPosisionsContainer.Add(drawableHit);
            });

            averagePositionContainer.MoveTo(averagePosition = (relativeHitPosition + averagePosition) / 2, 800, Easing.OutQuint);
        }

        private Color4 getColourForPosition(Vector2 position)
        {
            switch (Vector2.Distance(position, Vector2.Zero))
            {
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
            hitPosisionsContainer.Clear();
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
