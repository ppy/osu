// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Edit.Checks;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Tests.Beatmaps;
using osuTK;

namespace osu.Game.Rulesets.Osu.Tests.Editor.Checks
{
    [TestFixture]
    public class CheckOffscreenObjectsTest
    {
        private static readonly Vector2 playfield_centre = OsuPlayfield.BASE_SIZE * 0.5f;

        private CheckOffscreenObjects check = null!;

        [SetUp]
        public void Setup()
        {
            check = new CheckOffscreenObjects();
        }

        [Test]
        public void TestCircleInCenter()
        {
            assertOk(new Beatmap<HitObject>
            {
                HitObjects = new List<HitObject>
                {
                    new HitCircle
                    {
                        StartTime = 3000,
                        Position = playfield_centre
                    }
                }
            });
        }

        [Test]
        public void TestCircleNearEdge()
        {
            assertOk(new Beatmap<HitObject>
            {
                HitObjects = new List<HitObject>
                {
                    new HitCircle
                    {
                        StartTime = 3000,
                        Position = new Vector2(5, 5)
                    }
                }
            });
        }

        [Test]
        public void TestCircleNearEdgeStackedOffscreen()
        {
            assertOffscreenCircle(new Beatmap<HitObject>
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
            });
        }

        [Test]
        public void TestCircleOffscreen()
        {
            assertOffscreenCircle(new Beatmap<HitObject>
            {
                HitObjects = new List<HitObject>
                {
                    new HitCircle
                    {
                        StartTime = 3000,
                        Position = new Vector2(0, 0)
                    }
                }
            });
        }

        [Test]
        public void TestSliderInCenter()
        {
            assertOk(new Beatmap<HitObject>
            {
                HitObjects = new List<HitObject>
                {
                    new Slider
                    {
                        StartTime = 3000,
                        Position = new Vector2(420, 240),
                        Path = new SliderPath(new[]
                        {
                            new PathControlPoint(new Vector2(0, 0), PathType.LINEAR),
                            new PathControlPoint(new Vector2(-100, 0))
                        }),
                    }
                }
            });
        }

        [Test]
        public void TestSliderNearEdge()
        {
            assertOk(new Beatmap<HitObject>
            {
                HitObjects = new List<HitObject>
                {
                    new Slider
                    {
                        StartTime = 3000,
                        Position = playfield_centre,
                        Path = new SliderPath(new[]
                        {
                            new PathControlPoint(new Vector2(0, 0), PathType.LINEAR),
                            new PathControlPoint(new Vector2(0, -playfield_centre.Y + 5))
                        }),
                    }
                }
            });
        }

        [Test]
        public void TestSliderNearEdgeStackedOffscreen()
        {
            assertOffscreenSlider(new Beatmap<HitObject>
            {
                HitObjects = new List<HitObject>
                {
                    new Slider
                    {
                        StartTime = 3000,
                        Position = playfield_centre,
                        Path = new SliderPath(new[]
                        {
                            new PathControlPoint(new Vector2(0, 0), PathType.LINEAR),
                            new PathControlPoint(new Vector2(0, -playfield_centre.Y + 5))
                        }),
                        StackHeight = 5
                    }
                }
            });
        }

        [Test]
        public void TestSliderOffscreenStart()
        {
            assertOffscreenSlider(new Beatmap<HitObject>
            {
                HitObjects = new List<HitObject>
                {
                    new Slider
                    {
                        StartTime = 3000,
                        Position = new Vector2(0, 0),
                        Path = new SliderPath(new[]
                        {
                            new PathControlPoint(new Vector2(0, 0), PathType.LINEAR),
                            new PathControlPoint(playfield_centre)
                        }),
                    }
                }
            });
        }

        [Test]
        public void TestSliderOffscreenEnd()
        {
            assertOffscreenSlider(new Beatmap<HitObject>
            {
                HitObjects = new List<HitObject>
                {
                    new Slider
                    {
                        StartTime = 3000,
                        Position = playfield_centre,
                        Path = new SliderPath(new[]
                        {
                            new PathControlPoint(new Vector2(0, 0), PathType.LINEAR),
                            new PathControlPoint(-playfield_centre)
                        }),
                    }
                }
            });
        }

        [Test]
        public void TestSliderOffscreenPath()
        {
            assertOffscreenSlider(new Beatmap<HitObject>
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
                            new PathControlPoint(new Vector2(0, 0), PathType.PERFECT_CURVE),
                            new PathControlPoint(new Vector2(-100, -200)),
                            new PathControlPoint(new Vector2(100, -200))
                        }),
                    }
                }
            });
        }

        private void assertOk(IBeatmap beatmap)
        {
            var context = new BeatmapVerifierContext(beatmap, new TestWorkingBeatmap(beatmap));
            Assert.That(check.Run(context), Is.Empty);
        }

        private void assertOffscreenCircle(IBeatmap beatmap)
        {
            var context = new BeatmapVerifierContext(beatmap, new TestWorkingBeatmap(beatmap));
            var issues = check.Run(context).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues.Single().Template is CheckOffscreenObjects.IssueTemplateOffscreenCircle);
        }

        private void assertOffscreenSlider(IBeatmap beatmap)
        {
            var context = new BeatmapVerifierContext(beatmap, new TestWorkingBeatmap(beatmap));
            var issues = check.Run(context).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues.Single().Template is CheckOffscreenObjects.IssueTemplateOffscreenSlider);
        }
    }
}
