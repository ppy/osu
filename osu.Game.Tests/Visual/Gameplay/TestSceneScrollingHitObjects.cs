// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Timing;
using osu.Game.Rulesets.UI;
using osu.Game.Rulesets.UI.Scrolling;
using osuTK;

namespace osu.Game.Tests.Visual.Gameplay
{
    [TestFixture]
    public class TestSceneScrollingHitObjects : OsuTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[] { typeof(Playfield) };

        [Cached(typeof(IReadOnlyList<Mod>))]
        private IReadOnlyList<Mod> mods { get; set; } = Array.Empty<Mod>();

        private readonly ScrollingTestContainer[] scrollContainers = new ScrollingTestContainer[4];
        private readonly TestPlayfield[] playfields = new TestPlayfield[4];

        public TestSceneScrollingHitObjects()
        {
            Add(new GridContainer
            {
                RelativeSizeAxes = Axes.Both,
                Content = new[]
                {
                    new Drawable[]
                    {
                        scrollContainers[0] = new ScrollingTestContainer(ScrollingDirection.Up)
                        {
                            RelativeSizeAxes = Axes.Both,
                            Child = playfields[0] = new TestPlayfield()
                        },
                        scrollContainers[1] = new ScrollingTestContainer(ScrollingDirection.Up)
                        {
                            RelativeSizeAxes = Axes.Both,
                            Child = playfields[1] = new TestPlayfield()
                        },
                    },
                    new Drawable[]
                    {
                        scrollContainers[2] = new ScrollingTestContainer(ScrollingDirection.Up)
                        {
                            RelativeSizeAxes = Axes.Both,
                            Child = playfields[2] = new TestPlayfield()
                        },
                        scrollContainers[3] = new ScrollingTestContainer(ScrollingDirection.Up)
                        {
                            RelativeSizeAxes = Axes.Both,
                            Child = playfields[3] = new TestPlayfield()
                        }
                    }
                }
            });

            AddStep("Constant scroll", () => setScrollAlgorithm(ScrollVisualisationMethod.Constant));
            AddStep("Overlapping scroll", () => setScrollAlgorithm(ScrollVisualisationMethod.Overlapping));
            AddStep("Sequential scroll", () => setScrollAlgorithm(ScrollVisualisationMethod.Sequential));

            AddSliderStep("Time range", 100, 10000, 5000, v => scrollContainers.ForEach(c => c.TimeRange = v));
            AddStep("Add control point", () => addControlPoint(Time.Current + 5000));
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            scrollContainers.ForEach(c => c.ControlPoints.Add(new MultiplierControlPoint(0)));

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
            scrollContainers.ForEach(c =>
            {
                c.ControlPoints.Add(new MultiplierControlPoint(time) { DifficultyPoint = { SpeedMultiplier = 3 } });
                c.ControlPoints.Add(new MultiplierControlPoint(time + 2000) { DifficultyPoint = { SpeedMultiplier = 2 } });
                c.ControlPoints.Add(new MultiplierControlPoint(time + 3000) { DifficultyPoint = { SpeedMultiplier = 1 } });
            });

            playfields.ForEach(p =>
            {
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

        private void setScrollAlgorithm(ScrollVisualisationMethod algorithm) => scrollContainers.ForEach(c => c.ScrollAlgorithm = algorithm);

        private class TestPlayfield : ScrollingPlayfield
        {
            public new ScrollingDirection Direction => base.Direction.Value;

            public TestPlayfield()
            {
                Padding = new MarginPadding(2);

                InternalChildren = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Alpha = 0.5f,
                    },
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Masking = true,
                        Child = HitObjectContainer
                    }
                };
            }
        }

        private class TestDrawableControlPoint : DrawableHitObject<HitObject>
        {
            public TestDrawableControlPoint(ScrollingDirection direction, double time)
                : base(new HitObject { StartTime = time })
            {
                Origin = Anchor.Centre;

                AddInternal(new Box
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both
                });

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
        }

        private class TestDrawableHitObject : DrawableHitObject<HitObject>
        {
            public TestDrawableHitObject(double time)
                : base(new HitObject { StartTime = time })
            {
                Origin = Anchor.Centre;
                AutoSizeAxes = Axes.Both;

                AddInternal(new Box { Size = new Vector2(75) });
            }
        }
    }
}
