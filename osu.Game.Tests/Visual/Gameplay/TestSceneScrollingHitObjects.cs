// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Threading;
using osu.Framework.Timing;
using osu.Framework.Utils;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Timing;
using osu.Game.Rulesets.UI.Scrolling;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.Gameplay
{
    [TestFixture]
    public partial class TestSceneScrollingHitObjects : OsuTestScene
    {
        [Cached(typeof(IReadOnlyList<Mod>))]
        private IReadOnlyList<Mod> mods { get; set; } = Array.Empty<Mod>();

        private const int time_range = 5000;
        private const int spawn_rate = time_range / 10;

        private readonly ScrollingTestContainer[] scrollContainers = new ScrollingTestContainer[4];
        private readonly TestPlayfield[] playfields = new TestPlayfield[4];
        private ScheduledDelegate hitObjectSpawnDelegate;

        [SetUp]
        public void Setup() => Schedule(() =>
        {
            Child = new GridContainer
            {
                RelativeSizeAxes = Axes.Both,
                Content = new[]
                {
                    new Drawable[]
                    {
                        scrollContainers[0] = new ScrollingTestContainer(ScrollingDirection.Up)
                        {
                            RelativeSizeAxes = Axes.Both,
                            Child = playfields[0] = new TestPlayfield(),
                            TimeRange = time_range
                        },
                        scrollContainers[1] = new ScrollingTestContainer(ScrollingDirection.Down)
                        {
                            RelativeSizeAxes = Axes.Both,
                            Child = playfields[1] = new TestPlayfield(),
                            TimeRange = time_range
                        },
                    },
                    new Drawable[]
                    {
                        scrollContainers[2] = new ScrollingTestContainer(ScrollingDirection.Left)
                        {
                            RelativeSizeAxes = Axes.Both,
                            Child = playfields[2] = new TestPlayfield(),
                            TimeRange = time_range
                        },
                        scrollContainers[3] = new ScrollingTestContainer(ScrollingDirection.Right)
                        {
                            RelativeSizeAxes = Axes.Both,
                            Child = playfields[3] = new TestPlayfield(),
                            TimeRange = time_range
                        }
                    }
                }
            };

            hitObjectSpawnDelegate?.Cancel();
        });

        private void setUpHitObjects() => AddStep("set up hit objects", () =>
        {
            scrollContainers.ForEach(c => c.ControlPoints.Add(new MultiplierControlPoint(0)));

            for (int i = spawn_rate / 2; i <= time_range; i += spawn_rate)
                addHitObject(Time.Current + i);

            hitObjectSpawnDelegate = Scheduler.AddDelayed(() => addHitObject(Time.Current + time_range), spawn_rate, true);
        });

        private IList<MultiplierControlPoint> testControlPoints => new List<MultiplierControlPoint>
        {
            new MultiplierControlPoint(time_range) { EffectPoint = { ScrollSpeed = 1.25 } },
            new MultiplierControlPoint(1.5 * time_range) { EffectPoint = { ScrollSpeed = 1 } },
            new MultiplierControlPoint(2 * time_range) { EffectPoint = { ScrollSpeed = 1.5 } }
        };

        [Test]
        public void TestScrollAlgorithms()
        {
            setUpHitObjects();

            AddStep("constant scroll", () => setScrollAlgorithm(ScrollVisualisationMethod.Constant));
            AddStep("overlapping scroll", () => setScrollAlgorithm(ScrollVisualisationMethod.Overlapping));
            AddStep("sequential scroll", () => setScrollAlgorithm(ScrollVisualisationMethod.Sequential));

            AddSliderStep("time range", 100, 10000, time_range, v => scrollContainers.Where(c => c != null).ForEach(c => c.TimeRange = v));

            AddStep("add control points", () => addControlPoints(testControlPoints, Time.Current));
        }

        [Test]
        public void TestConstantScrollLifetime()
        {
            setUpHitObjects();

            AddStep("set constant scroll", () => setScrollAlgorithm(ScrollVisualisationMethod.Constant));
            // scroll container time range must be less than the rate of spawning hitobjects
            // otherwise the hitobjects will spawn already partly visible on screen and look wrong
            AddStep("set time range", () => scrollContainers.ForEach(c => c.TimeRange = time_range / 2.0));
        }

        [Test]
        public void TestSequentialScrollLifetime()
        {
            setUpHitObjects();

            AddStep("set sequential scroll", () => setScrollAlgorithm(ScrollVisualisationMethod.Sequential));
            AddStep("set time range", () => scrollContainers.ForEach(c => c.TimeRange = time_range / 2.0));
            AddStep("add control points", () => addControlPoints(testControlPoints, Time.Current));
        }

        [Test]
        public void TestSlowSequentialScroll()
        {
            AddStep("set sequential scroll", () => setScrollAlgorithm(ScrollVisualisationMethod.Sequential));
            AddStep("set time range", () => scrollContainers.ForEach(c => c.TimeRange = time_range));
            AddStep("add control points", () => addControlPoints(
                new List<MultiplierControlPoint>
                {
                    new MultiplierControlPoint { Velocity = 0.1 }
                },
                Time.Current + time_range));

            // All of the hit objects added below should be immediately visible on screen
            AddStep("add hit objects", () =>
            {
                for (int i = 0; i < 20; ++i)
                {
                    addHitObject(Time.Current + time_range * (2 + 0.1 * i));
                }
            });
        }

        [Test]
        public void TestOverlappingScrollLifetime()
        {
            setUpHitObjects();

            AddStep("set overlapping scroll", () => setScrollAlgorithm(ScrollVisualisationMethod.Overlapping));
            AddStep("set time range", () => scrollContainers.ForEach(c => c.TimeRange = time_range / 2.0));
            AddStep("add control points", () => addControlPoints(testControlPoints, Time.Current));
        }

        [Test]
        public void TestVeryFlowScroll()
        {
            const double long_time_range = 100000;
            var manualClock = new ManualClock();

            AddStep("set manual clock", () =>
            {
                manualClock.CurrentTime = 0;
                scrollContainers.ForEach(c => c.Clock = new FramedClock(manualClock));

                setScrollAlgorithm(ScrollVisualisationMethod.Constant);
                scrollContainers.ForEach(c => c.TimeRange = long_time_range);
            });

            AddStep("add hit objects", () =>
            {
                addHitObject(long_time_range);
                addHitObject(long_time_range + 100, 250);
            });

            AddAssert("hit objects are alive", () => playfields.All(p => p.HitObjectContainer.AliveObjects.Count() == 2));
        }

        private void addHitObject(double time, float size = 75)
        {
            playfields.ForEach(p =>
            {
                var hitObject = new TestHitObject(size) { StartTime = time };
                var drawable = new TestDrawableHitObject(hitObject);

                setAnchor(drawable, p);
                p.Add(drawable);
            });
        }

        private TestDrawableControlPoint createDrawablePoint(TestPlayfield playfield, double t)
        {
            var obj = new TestDrawableControlPoint(playfield.Direction, t);
            setAnchor(obj, playfield);
            return obj;
        }

        private void addControlPoints(IList<MultiplierControlPoint> controlPoints, double sequenceStartTime)
        {
            controlPoints.ForEach(point => point.Time += sequenceStartTime);

            scrollContainers.ForEach(container =>
            {
                container.ControlPoints.AddRange(controlPoints);
            });

            foreach (var playfield in playfields)
            {
                foreach (var controlPoint in controlPoints)
                    playfield.Add(createDrawablePoint(playfield, controlPoint.Time));
            }
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

        private partial class TestPlayfield : ScrollingPlayfield
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

            protected override ScrollingHitObjectContainer CreateScrollingHitObjectContainer() => new TestScrollingHitObjectContainer();
        }

        private partial class TestDrawableControlPoint : DrawableHitObject<HitObject>
        {
            public TestDrawableControlPoint(ScrollingDirection direction, double time)
                : base(new HitObject { StartTime = time, HitWindows = HitWindows.Empty })
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

        private class TestHitObject : HitObject
        {
            public readonly float Size;

            public TestHitObject(float size)
            {
                Size = size;
            }
        }

        private partial class TestDrawableHitObject : DrawableHitObject<TestHitObject>
        {
            public TestDrawableHitObject(TestHitObject hitObject)
                : base(hitObject)
            {
                Origin = Anchor.Centre;
                Size = new Vector2(hitObject.Size);

                AddInternal(new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = new Color4(RNG.NextSingle(), RNG.NextSingle(), RNG.NextSingle(), 1)
                });
            }
        }

        private partial class TestScrollingHitObjectContainer : ScrollingHitObjectContainer
        {
            protected override RectangleF GetConservativeBoundingBox(HitObjectLifetimeEntry entry)
            {
                if (entry.HitObject is TestHitObject testObject)
                    return new RectangleF().Inflate(testObject.Size / 2);

                return base.GetConservativeBoundingBox(entry);
            }
        }
    }
}
