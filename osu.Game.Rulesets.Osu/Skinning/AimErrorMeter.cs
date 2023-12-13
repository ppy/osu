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
using osu.Game.Localisation.HUD;
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
    public partial class AimErrorMeter : HitErrorMeter
    {
        [SettingSource(typeof(AimErrorMeterStrings), nameof(AimErrorMeterStrings.JudgementSize), nameof(AimErrorMeterStrings.JudgementSizeDescription))]
        public BindableNumber<float> JudgementSize { get; } = new BindableNumber<float>(7f)
        {
            MinValue = 0f,
            MaxValue = 12f,
            Precision = 1f
        };

        [SettingSource(typeof(AimErrorMeterStrings), nameof(AimErrorMeterStrings.JudgementStyle), nameof(AimErrorMeterStrings.JudgementStyleDescription))]
        public Bindable<HitPositionStyle> JudgementStyle { get; } = new Bindable<HitPositionStyle>();

        [SettingSource(typeof(AimErrorMeterStrings), nameof(AimErrorMeterStrings.AverageSize), nameof(AimErrorMeterStrings.AverageSizeDescription))]
        public BindableNumber<float> AverageSize { get; } = new BindableNumber<float>(12f)
        {
            MinValue = 7f,
            MaxValue = 25f,
            Precision = 1f
        };

        [SettingSource(typeof(AimErrorMeterStrings), nameof(AimErrorMeterStrings.AverageStyle), nameof(AimErrorMeterStrings.AverageStyleDescription))]
        public Bindable<HitPositionStyle> AverageStyle { get; } = new Bindable<HitPositionStyle>(HitPositionStyle.Plus);

        private Container averagePositionContainer = null!;
        private Vector2 averagePosition;

        private readonly DrawablePool<HitPosition> hitPositionPool = new DrawablePool<HitPosition>(20);
        private Container hitPositionsContainer = null!;

        private float objectRadius;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        public AimErrorMeter()
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

            foreach (var h in hitPositionsContainer)
            {
                h.ClearTransforms();
                h.Expire();
            }
        }

        private partial class HitPosition : PoolableDrawable
        {
            [Resolved]
            private AimErrorMeter aimErrorMeter { get; set; } = null!;

            public readonly BindableNumber<float> JudgementSize = new BindableFloat();

            public readonly Bindable<HitPositionStyle> JudgementStyle = new Bindable<HitPositionStyle>();

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

                JudgementSize.BindTo(aimErrorMeter.JudgementSize);
                JudgementSize.BindValueChanged(size => Size = new Vector2(size.NewValue));
                JudgementStyle.BindTo(aimErrorMeter.JudgementStyle);
                JudgementStyle.BindValueChanged(style => Rotation = style.NewValue == HitPositionStyle.X ? 0 : 45);
            }

            protected override void PrepareForUse()
            {
                base.PrepareForUse();

                const int judgement_fade_in_duration = 100;
                const int judgement_fade_out_duration = 5000;

                this
                    .ResizeTo(new Vector2(0))
                    .FadeInFromZero(judgement_fade_in_duration, Easing.OutQuint)
                    .ResizeTo(new Vector2(JudgementSize.Value), judgement_fade_in_duration, Easing.OutQuint)
                    .Then()
                    .FadeOut(judgement_fade_out_duration)
                    .Expire();
            }
        }

        public enum HitPositionStyle
        {
            [LocalisableDescription(typeof(AimErrorMeterStrings), nameof(AimErrorMeterStrings.StyleX))]
            X,

            [LocalisableDescription(typeof(AimErrorMeterStrings), nameof(AimErrorMeterStrings.StylePlus))]
            Plus
        }
    }
}
