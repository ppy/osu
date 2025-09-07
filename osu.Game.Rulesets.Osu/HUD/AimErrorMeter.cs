// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;
using System.Linq;
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
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Legacy;
using osu.Game.Rulesets.Osu.Judgements;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Statistics;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Play.HUD.HitErrorMeters;
using osuTK;
using osuTK.Graphics;
using Container = osu.Framework.Graphics.Containers.Container;

namespace osu.Game.Rulesets.Osu.HUD
{
    [Cached]
    public partial class AimErrorMeter : HitErrorMeter
    {
        [SettingSource(typeof(AimErrorMeterStrings), nameof(AimErrorMeterStrings.HitMarkerSize), nameof(AimErrorMeterStrings.HitMarkerSizeDescription))]
        public BindableNumber<float> HitMarkerSize { get; } = new BindableNumber<float>(7f)
        {
            MinValue = 0f,
            MaxValue = 12f,
            Precision = 1f
        };

        [SettingSource(typeof(AimErrorMeterStrings), nameof(AimErrorMeterStrings.HitMarkerStyle), nameof(AimErrorMeterStrings.HitMarkerStyleDescription))]
        public Bindable<MarkerStyle> HitMarkerStyle { get; } = new Bindable<MarkerStyle>();

        [SettingSource(typeof(AimErrorMeterStrings), nameof(AimErrorMeterStrings.AverageMarkerSize), nameof(AimErrorMeterStrings.AverageMarkerSizeDescription))]
        public BindableNumber<float> AverageMarkerSize { get; } = new BindableNumber<float>(12f)
        {
            MinValue = 7f,
            MaxValue = 25f,
            Precision = 1f
        };

        [SettingSource(typeof(AimErrorMeterStrings), nameof(AimErrorMeterStrings.AverageMarkerStyle), nameof(AimErrorMeterStrings.AverageMarkerStyleDescription))]
        public Bindable<MarkerStyle> AverageMarkerStyle { get; } = new Bindable<MarkerStyle>(MarkerStyle.Plus);

        [SettingSource(typeof(AimErrorMeterStrings), nameof(AimErrorMeterStrings.PositionDisplayStyle), nameof(AimErrorMeterStrings.PositionDisplayStyleDescription))]
        public Bindable<PositionDisplay> PositionDisplayStyle { get; } = new Bindable<PositionDisplay>();

        // used for calculate relative position.
        private Vector2? lastObjectPosition;

        private Container averagePositionMarker = null!;
        private Container averagePositionMarkerRotationContainer = null!;
        private Vector2? averagePosition;

        private readonly DrawablePool<HitPositionMarker> hitPositionPool = new DrawablePool<HitPositionMarker>(30);
        private Container hitPositionMarkerContainer = null!;

        private Container arrowBackgroundContainer = null!;
        private UprightAspectMaintainingContainer rotateFixedContainer = null!;
        private Container mainContainer = null!;

        private float objectRadius;

        private const int max_concurrent_judgements = 30;

        private const float line_thickness = 2;
        private const float inner_portion = 0.85f;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        public AimErrorMeter()
        {
            AutoSizeAxes = Axes.Both;
            AlwaysPresent = true;
        }

        [BackgroundDependencyLoader]
        private void load(IBindable<WorkingBeatmap> beatmap, ScoreProcessor processor)
        {
            InternalChild = new Container
            {
                Height = 100,
                Width = 100,
                Children = new Drawable[]
                {
                    hitPositionPool,
                    rotateFixedContainer = new UprightAspectMaintainingContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    },
                }
            };

            mainContainer = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    new CircularContainer
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        BorderColour = Colour4.White,
                        Masking = true,
                        BorderThickness = 2,
                        RelativeSizeAxes = Axes.Both,
                        Size = new Vector2(inner_portion),
                        Child = new Box
                        {
                            Colour = Colour4.Gray,
                            Alpha = 0.3f,
                            RelativeSizeAxes = Axes.Both
                        },
                    },
                    arrowBackgroundContainer = new Container
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Name = "Arrow Background",
                        RelativeSizeAxes = Axes.Both,
                        Rotation = 45,
                        Alpha = 0f,
                        Children = new Drawable[]
                        {
                            new Circle
                            {
                                RelativeSizeAxes = Axes.Y,
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Height = inner_portion + 0.2f,
                                Width = line_thickness / 2,
                            },
                            new Circle
                            {
                                Height = 5f,
                                Width = line_thickness / 2,
                                Anchor = Anchor.Centre,
                                Origin = Anchor.TopCentre,
                                Margin = new MarginPadding(-line_thickness / 4),
                                RelativePositionAxes = Axes.Both,
                                Y = -(inner_portion + 0.2f) / 2,
                                Rotation = -45
                            },
                            new Circle
                            {
                                Height = 5f,
                                Width = line_thickness / 2,
                                Anchor = Anchor.Centre,
                                Origin = Anchor.TopCentre,
                                Margin = new MarginPadding(-line_thickness / 4),
                                RelativePositionAxes = Axes.Both,
                                Y = -(inner_portion + 0.2f) / 2,
                                Rotation = 45
                            }
                        }
                    },
                    new Container
                    {
                        Name = "Cross Background",
                        RelativeSizeAxes = Axes.Both,
                        Children = new Drawable[]
                        {
                            new Circle
                            {
                                RelativeSizeAxes = Axes.Y,
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Alpha = 0.5f,
                                Width = line_thickness,
                                Height = inner_portion * 0.9f
                            },
                            new Circle
                            {
                                RelativeSizeAxes = Axes.Y,
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Alpha = 0.5f,
                                Width = line_thickness,
                                Height = inner_portion * 0.9f,
                                Rotation = 90
                            },
                            new Circle
                            {
                                RelativeSizeAxes = Axes.Y,
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Alpha = 0.2f,
                                Width = line_thickness / 2,
                                Height = inner_portion * 0.9f,
                                Rotation = 45
                            },
                            new Circle
                            {
                                RelativeSizeAxes = Axes.Y,
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Alpha = 0.2f,
                                Width = line_thickness / 2,
                                Height = inner_portion * 0.9f,
                                Rotation = 135
                            },
                        }
                    },
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Children = new Drawable[]
                        {
                            hitPositionMarkerContainer = new Container
                            {
                                RelativeSizeAxes = Axes.Both,
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre
                            },
                            averagePositionMarker = new UprightAspectMaintainingContainer
                            {
                                RelativePositionAxes = Axes.Both,
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Child = averagePositionMarkerRotationContainer = new Container
                                {
                                    RelativeSizeAxes = Axes.Both,
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
                    }
                }
            };

            // handle IApplicableToDifficulty for CS change.
            BeatmapDifficulty newDifficulty = new BeatmapDifficulty();
            beatmap.Value.Beatmap.Difficulty.CopyTo(newDifficulty);

            var mods = processor.Mods.Value;

            foreach (var mod in mods.OfType<IApplicableToDifficulty>())
                mod.ApplyToDifficulty(newDifficulty);

            objectRadius = OsuHitObject.OBJECT_RADIUS * LegacyRulesetExtensions.CalculateScaleFromCircleSize(newDifficulty.CircleSize, true);

            AverageMarkerSize.BindValueChanged(size => averagePositionMarker.Size = new Vector2(size.NewValue), true);
            AverageMarkerStyle.BindValueChanged(style => averagePositionMarkerRotationContainer.Rotation = style.NewValue == MarkerStyle.Plus ? 0 : 45, true);

            PositionDisplayStyle.BindValueChanged(s =>
            {
                Clear();

                if (s.NewValue == PositionDisplay.Normalised)
                {
                    arrowBackgroundContainer.FadeIn(100);
                    rotateFixedContainer.Remove(mainContainer, false);
                    AddInternal(mainContainer);
                }
                else
                {
                    arrowBackgroundContainer.FadeOut(100);
                    // when in absolute mode, rotation of the aim error meter as a whole should not affect how the component is displayed
                    RemoveInternal(mainContainer, false);
                    rotateFixedContainer.Add(mainContainer);
                }
            }, true);
        }

        protected override void OnNewJudgement(JudgementResult judgement)
        {
            if (judgement is not OsuHitCircleJudgementResult circleJudgement) return;

            if (circleJudgement.CursorPositionAtHit == null) return;

            if (hitPositionMarkerContainer.Count > max_concurrent_judgements)
            {
                const double quick_fade_time = 300;

                // check with a bit of lenience to avoid precision error in comparison.
                var old = hitPositionMarkerContainer.FirstOrDefault(j => j.LifetimeEnd > Clock.CurrentTime + quick_fade_time * 1.1);

                if (old != null)
                {
                    old.ClearTransforms();
                    old.FadeOut(quick_fade_time).Expire();
                }
            }

            Vector2 hitPosition;

            if (PositionDisplayStyle.Value == PositionDisplay.Normalised && lastObjectPosition != null)
            {
                hitPosition = AccuracyHeatmap.FindRelativeHitPosition(lastObjectPosition.Value, ((OsuHitObject)circleJudgement.HitObject).StackedEndPosition,
                    circleJudgement.CursorPositionAtHit.Value, objectRadius, 45) * (inner_portion / 2);
            }
            else
            {
                // get relative position between mouse position and current object.
                hitPosition = (circleJudgement.CursorPositionAtHit.Value - ((OsuHitObject)circleJudgement.HitObject).StackedPosition) / objectRadius / 2 * inner_portion;
            }

            hitPosition = Vector2.Clamp(hitPosition, new Vector2(-0.5f), new Vector2(0.5f));

            hitPositionPool.Get(drawableHit =>
            {
                drawableHit.X = hitPosition.X;
                drawableHit.Y = hitPosition.Y;
                drawableHit.Colour = getColourForPosition(hitPosition);

                hitPositionMarkerContainer.Add(drawableHit);
            });

            var newAveragePosition = 0.1f * hitPosition + 0.9f * (averagePosition ?? hitPosition);
            averagePositionMarker.MoveTo(newAveragePosition, 800, Easing.OutQuint);
            averagePosition = newAveragePosition;
            lastObjectPosition = ((OsuHitObject)circleJudgement.HitObject).StackedPosition;
        }

        private Color4 getColourForPosition(Vector2 position)
        {
            float distance = Vector2.Distance(position, Vector2.Zero);

            if (distance >= 0.5f * inner_portion)
                return colours.Red;

            if (distance >= 0.35f * inner_portion)
                return colours.Yellow;

            if (distance >= 0.2f * inner_portion)
                return colours.Green;

            return colours.Blue;
        }

        public override void Clear()
        {
            averagePosition = null;
            averagePositionMarker.MoveTo(Vector2.Zero, 800, Easing.OutQuint);
            lastObjectPosition = null;

            foreach (var h in hitPositionMarkerContainer)
            {
                h.ClearTransforms();
                h.Expire();
            }
        }

        private partial class HitPositionMarker : PoolableDrawable
        {
            [Resolved]
            private AimErrorMeter aimErrorMeter { get; set; } = null!;

            public readonly BindableNumber<float> MarkerSize = new BindableFloat();
            public readonly Bindable<MarkerStyle> Style = new Bindable<MarkerStyle>();

            private readonly Container content;

            public HitPositionMarker()
            {
                RelativePositionAxes = Axes.Both;

                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;

                InternalChild = new UprightAspectMaintainingContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Child = content = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
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
                        }
                    }
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                MarkerSize.BindTo(aimErrorMeter.HitMarkerSize);
                MarkerSize.BindValueChanged(size => Size = new Vector2(size.NewValue), true);
                Style.BindTo(aimErrorMeter.HitMarkerStyle);
                Style.BindValueChanged(style => content.Rotation = style.NewValue == MarkerStyle.X ? 0 : 45, true);
            }

            protected override void PrepareForUse()
            {
                base.PrepareForUse();

                const int judgement_fade_in_duration = 100;
                const int judgement_fade_out_duration = 5000;

                this
                    .ResizeTo(new Vector2(0))
                    .FadeInFromZero(judgement_fade_in_duration, Easing.OutQuint)
                    .ResizeTo(new Vector2(MarkerSize.Value), judgement_fade_in_duration, Easing.OutQuint)
                    .Then()
                    .FadeOut(judgement_fade_out_duration)
                    .Expire();
            }
        }

        public enum MarkerStyle
        {
            [Description("x")]
            X,

            [Description("+")]
            Plus,
        }

        public enum PositionDisplay
        {
            [LocalisableDescription(typeof(AimErrorMeterStrings), nameof(AimErrorMeterStrings.Absolute))]
            Absolute,

            [LocalisableDescription(typeof(AimErrorMeterStrings), nameof(AimErrorMeterStrings.Normalised))]
            Normalised,
        }
    }
}
