// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Testing;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Timing;

namespace osu.Desktop.VisualTests.Tests
{
    public class TestCaseScrollingHitObjects : TestCase
    {
        public override string Description => "SpeedAdjustmentContainer/DrawableTimingSection";

        private SpeedAdjustmentCollection adjustmentCollection;

        private BindableDouble timeRangeBindable;
        private OsuSpriteText timeRangeText;
        private OsuSpriteText bottomLabel;
        private SpriteText topTime, bottomTime;

        public override void Reset()
        {
            base.Reset();

            timeRangeBindable = new BindableDouble(2000)
            {
                MinValue = 200,
                MaxValue = 4000,
            };

            SliderBar<double> timeRange;
            Add(timeRange = new BasicSliderBar<double>
            {
                Size = new Vector2(200, 20),
                SelectionColor = Color4.Pink,
                KeyboardStep = 100
            });

            Add(timeRangeText = new OsuSpriteText
            {
                X = 210,
                TextSize = 16,
            });

            timeRange.Current.BindTo(timeRangeBindable);
            timeRangeBindable.ValueChanged += v => timeRangeText.Text = $"Visible Range: {v:#,#.#}";
            timeRangeBindable.ValueChanged += v => bottomLabel.Text = $"t minus {v:#,#}";

            Add(new Drawable[]
            {
                new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(100, 500),
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Alpha = 0.25f
                        },
                        adjustmentCollection = new SpeedAdjustmentCollection(Axes.Y)
                        {
                            RelativeSizeAxes = Axes.Both,
                            VisibleTimeRange = timeRangeBindable,
                            Masking = true,
                        },
                        new OsuSpriteText
                        {
                            Text = "t minus 0",
                            Margin = new MarginPadding(2),
                            TextSize = 14,
                            Anchor = Anchor.TopRight,
                        },
                        bottomLabel = new OsuSpriteText
                        {
                            Text = "t minus x",
                            Margin = new MarginPadding(2),
                            TextSize = 14,
                            Anchor = Anchor.BottomRight,
                            Origin = Anchor.BottomLeft,
                        },
                        topTime = new OsuSpriteText
                        {
                            Margin = new MarginPadding(2),
                            TextSize = 14,
                            Anchor = Anchor.TopLeft,
                            Origin = Anchor.TopRight,
                        },
                        bottomTime = new OsuSpriteText
                        {
                            Margin = new MarginPadding(2),
                            TextSize = 14,
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomRight,
                        },
                    }
                }
            });

            timeRangeBindable.TriggerChange();

            adjustmentCollection.Add(new TestSpeedAdjustmentContainer(new MultiplierControlPoint()));

            AddStep("Add hit object", () => adjustmentCollection.Add(new TestDrawableHitObject(new HitObject { StartTime = Time.Current + 2000 })));
        }

        protected override void Update()
        {
            base.Update();

            topTime.Text = Time.Current.ToString("#,#");
            bottomTime.Text = (Time.Current + timeRangeBindable.Value).ToString("#,#");
        }

        private class TestSpeedAdjustmentContainer : SpeedAdjustmentContainer
        {
            public override bool RemoveWhenNotAlive => false;

            public TestSpeedAdjustmentContainer(MultiplierControlPoint controlPoint)
                : base(controlPoint)
            {
            }

            protected override DrawableTimingSection CreateTimingSection() => new TestDrawableTimingSection(ControlPoint);

            private class TestDrawableTimingSection : DrawableTimingSection
            {
                private readonly MultiplierControlPoint controlPoint;

                public TestDrawableTimingSection(MultiplierControlPoint controlPoint)
                {
                    this.controlPoint = controlPoint;
                }

                protected override void Update()
                {
                    base.Update();

                    Y = (float)(controlPoint.StartTime - Time.Current);
                }
            }
        }

        private class TestDrawableHitObject : DrawableHitObject, IScrollingHitObject
        {
            private readonly Box background;
            private const float height = 14;

            public BindableDouble LifetimeOffset { get; } = new BindableDouble();

            public TestDrawableHitObject(HitObject hitObject)
                : base(hitObject)
            {
                AutoSizeAxes = Axes.Y;
                RelativeSizeAxes = Axes.X;
                RelativePositionAxes = Axes.Y;

                Y = (float)hitObject.StartTime;

                Children = new Drawable[]
                {
                    background = new Box
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = height,
                    },
                    new Box
                    {
                        RelativeSizeAxes = Axes.X,
                        Colour = Color4.Cyan,
                        Height = 1,
                    },
                    new OsuSpriteText
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Colour = Color4.Black,
                        TextSize = height,
                        Font = @"Exo2.0-BoldItalic",
                        Text = $"{hitObject.StartTime:#,#}"
                    }
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                FadeInFromZero(250, EasingTypes.OutQuint);
            }

            protected override void Update()
            {
                base.Update();
                if (Time.Current >= HitObject.StartTime)
                    background.Colour = Color4.Red;
            }
        }
    }
}
