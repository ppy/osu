// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Edit.Checks;
using osu.Game.Rulesets.Osu.Objects;
using osuTK;

namespace osu.Game.Rulesets.Osu.Tests.Editor.Checks
{
    [TestFixture]
    public class CheckOffscreenObjectsTest
    {
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
                        Position = new Vector2(320, 240) // Playfield is 640 x 480.
                    }
                }
            };

            var issues = check.Run(beatmap);

            Assert.That(!issues.Any());
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

            var issues = check.Run(beatmap);

            Assert.That(!issues.Any());
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

            var issues = check.Run(beatmap);

            Assert.That(!issues.Any());
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
                        Position = new Vector2(320, 240),
                        Path = new SliderPath(new[]
                        {
                            new PathControlPoint(new Vector2(0, 0), PathType.Linear),
                            new PathControlPoint(new Vector2(0, -235))
                        }),
                    }
                }
            };

            var issues = check.Run(beatmap);

            Assert.That(!issues.Any());
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
                        Position = new Vector2(320, 240),
                        Path = new SliderPath(new[]
                        {
                            new PathControlPoint(new Vector2(0, 0), PathType.Linear),
                            new PathControlPoint(new Vector2(0, -235))
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
                            new PathControlPoint(new Vector2(320, 240))
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
                        Position = new Vector2(320, 240),
                        Path = new SliderPath(new[]
                        {
                            new PathControlPoint(new Vector2(0, 0), PathType.Linear),
                            new PathControlPoint(new Vector2(-320, -240))
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
                        Position = new Vector2(320, 240),
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
            var issue = issues.FirstOrDefault();

            Assert.That(issues.Count == 1);
            Assert.That(issue != null);
            Assert.That(issue.Template is CheckOffscreenObjects.IssueTemplateOffscreenCircle);
        }

        private void assertOffscreenSlider(IBeatmap beatmap)
        {
            var issues = check.Run(beatmap).ToList();
            var issue = issues.FirstOrDefault();

            Assert.That(issues.Count == 1);
            Assert.That(issue != null);
            Assert.That(issue.Template is CheckOffscreenObjects.IssueTemplateOffscreenSlider);
        }
    }
}
