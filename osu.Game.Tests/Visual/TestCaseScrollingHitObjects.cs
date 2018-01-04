// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Framework.Configuration;
using OpenTK;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Timing;
using osu.Game.Rulesets.UI;
using OpenTK.Graphics;

namespace osu.Game.Tests.Visual
{
    public class TestCaseScrollingHitObjects : OsuTestCase
    {
        public override IReadOnlyList<Type> RequiredTypes => new[] { typeof(Playfield) };

        private readonly List<TestPlayfield> playfields = new List<TestPlayfield>();

        public TestCaseScrollingHitObjects()
        {
            playfields.Add(new TestPlayfield(ScrollingDirection.Down));
            playfields.Add(new TestPlayfield(ScrollingDirection.Right));

            playfields.ForEach(p => p.HitObjects.ControlPoints.Add(new MultiplierControlPoint(double.MinValue)));

            Add(new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Size = new Vector2(0.85f),
                Masking = true,
                BorderColour = Color4.White,
                BorderThickness = 2,
                MaskingSmoothness = 1,
                Children = new Drawable[]
                {
                    new Box
                    {
                        Name = "Background",
                        RelativeSizeAxes = Axes.Both,
                        Alpha = 0.35f,
                    },
                    playfields[0],
                    playfields[1]
                }
            });

            AddSliderStep("Time range", 100, 10000, 5000, v => playfields.ForEach(p => p.TimeRange.Value = v));
            AddStep("Add control point", () => addControlPoint(Time.Current + 5000));
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            for (int i = 0; i <= 5000; i += 1000)
                addHitObject(Time.Current + i);

            Scheduler.AddDelayed(() => addHitObject(Time.Current + 5000), 1000, true);
        }

        private void addHitObject(double time)
        {
            playfields.ForEach(p =>
            {
                p.Add(new TestDrawableHitObject(time)
                {
                    Anchor = p.Direction == ScrollingDirection.Right ? Anchor.CentreRight : Anchor.BottomCentre
                });
            });
        }

        private void addControlPoint(double time)
        {
            playfields.ForEach(p =>
            {
                p.HitObjects.ControlPoints.AddRange(new[]
                {
                    new MultiplierControlPoint(time) { DifficultyPoint = { SpeedMultiplier = 3 } },
                    new MultiplierControlPoint(time + 2000) { DifficultyPoint = { SpeedMultiplier = 2 } },
                    new MultiplierControlPoint(time + 3000) { DifficultyPoint = { SpeedMultiplier = 1 } },
                });

                TestDrawableControlPoint createDrawablePoint(double t) => new TestDrawableControlPoint(t)
                {
                    Anchor = p.Direction == ScrollingDirection.Right ? Anchor.CentreRight : Anchor.BottomCentre
                };

                p.Add(createDrawablePoint(time));
                p.Add(createDrawablePoint(time + 2000));
                p.Add(createDrawablePoint(time + 3000));
            });
        }



        private class TestPlayfield : Playfield
        {
            public readonly BindableDouble TimeRange = new BindableDouble(5000);

            public readonly ScrollingDirection Direction;

            public new ScrollingHitObjectContainer HitObjects => (ScrollingHitObjectContainer)base.HitObjects;

            public TestPlayfield(ScrollingDirection direction)
            {
                Direction = direction;

                base.HitObjects = new ScrollingHitObjectContainer(direction);
                HitObjects.TimeRange.BindTo(TimeRange);
            }
        }

        private class TestDrawableControlPoint : DrawableHitObject<HitObject>
        {
            private readonly Box box;

            public TestDrawableControlPoint(double time)
                : base(new HitObject { StartTime = time })
            {
                Origin = Anchor.Centre;

                Add(box = new Box
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                });
            }

            protected override void Update()
            {
                base.Update();

                RelativeSizeAxes = (Anchor & Anchor.x2) > 0 ? Axes.Y : Axes.X;
                Size = new Vector2(1);

                box.Size = DrawSize;
            }

            protected override void UpdateState(ArmedState state)
            {
            }
        }

        private class TestDrawableHitObject : DrawableHitObject<HitObject>
        {
            public TestDrawableHitObject(double time)
                : base(new HitObject { StartTime = time })
            {
                Origin = Anchor.Centre;
                AutoSizeAxes = Axes.Both;

                Add(new Box { Size = new Vector2(75) });
            }

            protected override void UpdateState(ArmedState state)
            {
            }
        }
    }
}
