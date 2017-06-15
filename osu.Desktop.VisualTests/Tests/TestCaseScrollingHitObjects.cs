// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Testing;
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
        private SpriteText timeRangeText;

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

            Add(timeRangeText = new SpriteText
            {
                X = 210,
                TextSize = 16,
            });

            timeRange.Current.BindTo(timeRangeBindable);
            timeRangeBindable.ValueChanged += v => timeRangeText.Text = $"Visible Range: {v:0.#}";
            timeRangeBindable.TriggerChange();

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
                        adjustmentCollection = new SpeedAdjustmentCollection
                        {
                            RelativeSizeAxes = Axes.Both,
                            VisibleTimeRange = timeRangeBindable
                        }
                    }
                }
            });

            adjustmentCollection.Add(new TestSpeedAdjustmentContainer(new MultiplierControlPoint()));

            AddStep("Add hit object", () => adjustmentCollection.Add(new TestDrawableHitObject(new HitObject { StartTime = Time.Current + 2000 })));
        }

        private class TestSpeedAdjustmentContainer : SpeedAdjustmentContainer
        {
            public TestSpeedAdjustmentContainer(MultiplierControlPoint controlPoint)
                : base(controlPoint, Axes.Y)
            {
            }

            protected override DrawableTimingSection CreateTimingSection() => new TestDrawableTimingSection(ControlPoint);

            private class TestDrawableTimingSection : DrawableTimingSection
            {
                private readonly MultiplierControlPoint controlPoint;

                public TestDrawableTimingSection(MultiplierControlPoint controlPoint)
                    : base(Axes.Y)
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

        private class TestDrawableHitObject : DrawableHitObject
        {
            public TestDrawableHitObject(HitObject hitObject)
                : base(hitObject)
            {
                AutoSizeAxes = Axes.Y;
                RelativeSizeAxes = Axes.X;
                RelativePositionAxes = Axes.Y;

                Y = (float)hitObject.StartTime;

                Add(new Box
                {
                    RelativeSizeAxes = Axes.X,
                    Height = 10,
                });
            }
        }
    }
}