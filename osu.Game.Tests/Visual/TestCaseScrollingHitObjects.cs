// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Extensions.IEnumerableExtensions;
using OpenTK;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Timing;
using osu.Game.Rulesets.UI;
using osu.Game.Rulesets.UI.Scrolling;

namespace osu.Game.Tests.Visual
{
    [TestFixture]
    public class TestCaseScrollingHitObjects : OsuTestCase
    {
        public override IReadOnlyList<Type> RequiredTypes => new[] { typeof(Playfield) };

        private readonly TestPlayfield[] playfields = new TestPlayfield[4];

        public TestCaseScrollingHitObjects()
        {
            Add(new GridContainer
            {
                RelativeSizeAxes = Axes.Both,
                Content = new[]
                {
                    new Drawable[]
                    {
                        playfields[0] = new TestPlayfield(ScrollingDirection.Up),
                        playfields[1] = new TestPlayfield(ScrollingDirection.Down)
                    },
                    new Drawable[]
                    {
                        playfields[2] = new TestPlayfield(ScrollingDirection.Left),
                        playfields[3] = new TestPlayfield(ScrollingDirection.Right)
                    }
                }
            });

            AddSliderStep("Time range", 100, 10000, 5000, v => playfields.ForEach(p => p.VisibleTimeRange.Value = v));
            AddStep("Add control point", () => addControlPoint(Time.Current + 5000));
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            playfields.ForEach(p => p.HitObjects.AddControlPoint(new MultiplierControlPoint(0)));

            for (int i = 0; i <= 5000; i += 1000)
                addHitObject(Time.Current + i);

            Scheduler.AddDelayed(() => addHitObject(Time.Current + 5000), 1000, true);
        }

        private void addHitObject(double time)
        {
            playfields.ForEach(p =>
            {
                var hitObject = new TestDrawableHitObject(time);
                setAnchor(hitObject, p);

                p.Add(hitObject);
            });
        }

        private void addControlPoint(double time)
        {
            playfields.ForEach(p =>
            {
                p.HitObjects.AddControlPoint(new MultiplierControlPoint(time) { DifficultyPoint = { SpeedMultiplier = 3 } });
                p.HitObjects.AddControlPoint(new MultiplierControlPoint(time + 2000) { DifficultyPoint = { SpeedMultiplier = 2 } });
                p.HitObjects.AddControlPoint(new MultiplierControlPoint(time + 3000) { DifficultyPoint = { SpeedMultiplier = 1 } });

                TestDrawableControlPoint createDrawablePoint(double t)
                {
                    var obj = new TestDrawableControlPoint(p.Direction, t);
                    setAnchor(obj, p);
                    return obj;
                }

                p.Add(createDrawablePoint(time));
                p.Add(createDrawablePoint(time + 2000));
                p.Add(createDrawablePoint(time + 3000));
            });
        }

        private void setAnchor(DrawableHitObject obj, TestPlayfield playfield)
        {
            switch (playfield.Direction)
            {
                case ScrollingDirection.Up:
                    obj.Anchor = Anchor.TopCentre;
                    break;
                case ScrollingDirection.Down:
                    obj.Anchor = Anchor.BottomCentre;
                    break;
                case ScrollingDirection.Left:
                    obj.Anchor = Anchor.CentreLeft;
                    break;
                case ScrollingDirection.Right:
                    obj.Anchor = Anchor.CentreRight;
                    break;
            }
        }


        private class TestPlayfield : ScrollingPlayfield
        {
            public new readonly ScrollingDirection Direction;

            public TestPlayfield(ScrollingDirection direction)
            {
                Direction = direction;

                Padding = new MarginPadding(2);
                Content.Masking = true;

                AddInternal(new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0.5f,
                    Depth = float.MaxValue
                });
            }
        }

        private class TestDrawableControlPoint : DrawableHitObject<HitObject>
        {
            public TestDrawableControlPoint(ScrollingDirection direction, double time)
                : base(new HitObject { StartTime = time })
            {
                Origin = Anchor.Centre;

                InternalChild = new Box
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both
                };

                switch (direction)
                {
                    case ScrollingDirection.Up:
                    case ScrollingDirection.Down:
                        RelativeSizeAxes = Axes.X;
                        Height = 2;
                        break;
                    case ScrollingDirection.Left:
                    case ScrollingDirection.Right:
                        RelativeSizeAxes = Axes.Y;
                        Width = 2;
                        break;
                }
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

                InternalChild = new Box { Size = new Vector2(75) };
            }

            protected override void UpdateState(ArmedState state)
            {
            }
        }
    }
}
