// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Edit.Checks.Components;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Edit.Checks;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.UI;
using osuTK;

namespace osu.Game.Rulesets.Osu.Tests.Editor.Checks
{
    [TestFixture]
    public class CheckOffscreenObjectsTest
    {
        private static readonly Vector2 playfield_centre = OsuPlayfield.BASE_SIZE * 0.5f;

        private CheckOffscreenObjects check;

        [SetUp]
        public void Setup()
        {
            check = new CheckOffscreenObjects();
        }

        [Test]
        public void TestCircleInCenter()
        {
            var beatmap = new Beatmap<HitObject>
            {
                HitObjects = new List<HitObject>
                {
                    new HitCircle
                    {
                        StartTime = 3000,
                        Position = playfield_centre // Playfield is 640 x 480.
                    }
                }
            };

            Assert.That(check.Run(beatmap), Is.Empty);
        }

        [Test]
        public void TestCircleNearEdge()
        {
            var beatmap = new Beatmap<HitObject>
            {
                HitObjects = new List<HitObject>
                {
                    new HitCircle
                    {
                        StartTime = 3000,
                        Position = new Vector2(5, 5)
                    }
                }
            };

            Assert.That(check.Run(beatmap), Is.Empty);
        }

        [Test]
        public void TestCircleNearEdgeStackedOffscreen()
        {
            var beatmap = new Beatmap<HitObject>
            {
                HitObjects = new List<HitObject>
                {
                    new HitCircle
                    {
                        StartTime = 3000,
                        Position = new Vector2(5, 5),
                        StackHeight = 5
                    }
                }
            };

            assertOffscreenCircle(beatmap);
        }

        [Test]
        public void TestCircleOffscreen()
        {
            var beatmap = new Beatmap<HitObject>
            {
                HitObjects = new List<HitObject>
                {
                    new HitCircle
                    {
                        StartTime = 3000,
                        Position = new Vector2(0, 0)
                    }
                }
            };

            assertOffscreenCircle(beatmap);
        }

        [Test]
        public void TestSliderInCenter()
        {
            var beatmap = new Beatmap<HitObject>
            {
                HitObjects = new List<HitObject>
                {
                    new Slider
                    {
                        StartTime = 3000,
                        Position = new Vector2(420, 240),
                        Path = new SliderPath(new[]
                        {
                            new PathControlPoint(new Vector2(0, 0), PathType.Linear),
                            new PathControlPoint(new Vector2(-100, 0))
                        }),
                    }
                }
            };

            Assert.That(check.Run(beatmap), Is.Empty);
        }

        [Test]
        public void TestSliderNearEdge()
        {
            var beatmap = new Beatmap<HitObject>
            {
                HitObjects = new List<HitObject>
                {
                    new Slider
                    {
                        StartTime = 3000,
                        Position = playfield_centre,
                        Path = new SliderPath(new[]
                        {
                            new PathControlPoint(new Vector2(0, 0), PathType.Linear),
                            new PathControlPoint(new Vector2(0, -playfield_centre.Y + 5))
                        }),
                    }
                }
            };

            Assert.That(check.Run(beatmap), Is.Empty);
        }

        [Test]
        public void TestSliderNearEdgeStackedOffscreen()
        {
            var beatmap = new Beatmap<HitObject>
            {
                HitObjects = new List<HitObject>
                {
                    new Slider
                    {
                        StartTime = 3000,
                        Position = playfield_centre,
                        Path = new SliderPath(new[]
                        {
                            new PathControlPoint(new Vector2(0, 0), PathType.Linear),
                            new PathControlPoint(new Vector2(0, -playfield_centre.Y + 5))
                        }),
                        StackHeight = 5
                    }
                }
            };

            assertOffscreenSlider(beatmap);
        }

        [Test]
        public void TestSliderOffscreenStart()
        {
            var beatmap = new Beatmap<HitObject>
            {
                HitObjects = new List<HitObject>
                {
                    new Slider
                    {
                        StartTime = 3000,
                        Position = new Vector2(0, 0),
                        Path = new SliderPath(new[]
                        {
                            new PathControlPoint(new Vector2(0, 0), PathType.Linear),
                            new PathControlPoint(playfield_centre)
                        }),
                    }
                }
            };

            assertOffscreenSlider(beatmap);
        }

        [Test]
        public void TestSliderOffscreenEnd()
        {
            var beatmap = new Beatmap<HitObject>
            {
                HitObjects = new List<HitObject>
                {
                    new Slider
                    {
                        StartTime = 3000,
                        Position = playfield_centre,
                        Path = new SliderPath(new[]
                        {
                            new PathControlPoint(new Vector2(0, 0), PathType.Linear),
                            new PathControlPoint(-playfield_centre)
                        }),
                    }
                }
            };

            assertOffscreenSlider(beatmap);
        }

        [Test]
        public void TestSliderOffscreenPath()
        {
            var beatmap = new Beatmap<HitObject>
            {
                HitObjects = new List<HitObject>
                {
                    new Slider
                    {
                        StartTime = 3000,
                        Position = playfield_centre,
                        Path = new SliderPath(new[]
                        {
                            // Circular arc shoots over the top of the screen.
                            new PathControlPoint(new Vector2(0, 0), PathType.PerfectCurve),
                            new PathControlPoint(new Vector2(-100, -200)),
                            new PathControlPoint(new Vector2(100, -200))
                        }),
                    }
                }
            };

            assertOffscreenSlider(beatmap);
        }

        private void assertOffscreenCircle(IBeatmap beatmap)
        {
            var issues = check.Run(beatmap).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues.Single().Template is CheckOffscreenObjects.IssueTemplateOffscreenCircle);
        }

        private void assertOffscreenSlider(IBeatmap beatmap)
        {
            var issues = check.Run(beatmap).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues.Single().Template is CheckOffscreenObjects.IssueTemplateOffscreenSlider);
        }
    }
}
