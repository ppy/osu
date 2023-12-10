// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Pooling;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Localisation;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Legacy;
using osu.Game.Rulesets.Osu.Judgements;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Screens.Play.HUD.HitErrorMeters;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Skinning
{
    [Cached]
    public partial class HitPositionMeter : HitErrorMeter
    {
        [SettingSource(typeof(PositionMeterStrings), nameof(PositionMeterStrings.JudgmentSize), nameof(PositionMeterStrings.JudgmentSizeDescription))]
        public BindableNumber<float> JudgmentSize { get; } = new BindableNumber<float>(7f)
        {
            MinValue = 0f,
            MaxValue = 12f,
            Precision = 1f
        };

        [SettingSource(typeof(PositionMeterStrings), nameof(PositionMeterStrings.JudgmentStyle), nameof(PositionMeterStrings.JudgmentStyleDescription))]
        public Bindable<HitPositionStyle> JudgmentStyle { get; } = new Bindable<HitPositionStyle>();

        [SettingSource(typeof(PositionMeterStrings), nameof(PositionMeterStrings.AverageSize), nameof(PositionMeterStrings.AverageSizeDescription))]
        public BindableNumber<float> AverageSize { get; } = new BindableNumber<float>(12f)
        {
            MinValue = 7f,
            MaxValue = 25f,
            Precision = 1f
        };

        [SettingSource(typeof(PositionMeterStrings), nameof(PositionMeterStrings.AverageStyle), nameof(PositionMeterStrings.AverageStyleDescription))]
        public Bindable<HitPositionStyle> AverageStyle { get; } = new Bindable<HitPositionStyle>(HitPositionStyle.Plus);

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
                    hitPositionsContainer = new UprightAspectMaintainingContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre
                    },
                    new UprightAspectMaintainingContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Child = averagePositionContainer = new Container
                        {
                            RelativePositionAxes = Axes.Both,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Children = new Drawable[]
                            {
                                new Circle
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Width = 0.25f,
                                },
                                new Circle
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Width = 0.25f,
                                    Rotation = 90
                                }
                            }
                        }
                    }
                }
            };

            objectRadius = OsuHitObject.OBJECT_RADIUS * LegacyRulesetExtensions.CalculateScaleFromCircleSize(beatmap.Value.Beatmap.Difficulty.CircleSize, true);

            AverageSize.BindValueChanged(size => averagePositionContainer.Size = new Vector2(size.NewValue), true);
            AverageStyle.BindValueChanged(style => averagePositionContainer.Rotation = style.NewValue == HitPositionStyle.Plus ? 0 : 45, true);
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
            [Resolved]
            private HitPositionMeter hitPositionMeter { get; set; } = null!;

            public readonly BindableNumber<float> JudgmentSize = new BindableFloat();

            public readonly Bindable<HitPositionStyle> JudgmentStyle = new Bindable<HitPositionStyle>();

            public HitPosition()
            {
                RelativePositionAxes = Axes.Both;

                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;

                InternalChildren = new Drawable[]
                {
                    new Circle
                    {
                        RelativeSizeAxes = Axes.Both,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Width = 0.25f,
                        Rotation = -45
                    },
                    new Circle
                    {
                        RelativeSizeAxes = Axes.Both,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Width = 0.25f,
                        Rotation = 45
                    }
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                JudgmentSize.BindTo(hitPositionMeter.JudgmentSize);
                JudgmentSize.BindValueChanged(size => Size = new Vector2(size.NewValue), true);
                JudgmentStyle.BindTo(hitPositionMeter.JudgmentStyle);
                JudgmentStyle.BindValueChanged(style => Rotation = style.NewValue == HitPositionStyle.X ? 0 : 45);
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

        public enum HitPositionStyle
        {
            [LocalisableDescription(typeof(PositionMeterStrings), nameof(PositionMeterStrings.StyleX))]
            X,

            [LocalisableDescription(typeof(PositionMeterStrings), nameof(PositionMeterStrings.StylePlus))]
            Plus
        }
    }
}
