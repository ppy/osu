// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Edit.Checks;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Tests.Beatmaps;
using osuTK;

namespace osu.Game.Rulesets.Osu.Tests.Editor.Checks
{
    [TestFixture]
    public class CheckLowDiffOverlapsTest
    {
        private CheckLowDiffOverlaps check = null!;

        [SetUp]
        public void Setup()
        {
            check = new CheckLowDiffOverlaps();
        }

        [Test]
        public void TestNoOverlapFarApart()
        {
            assertOk(new Beatmap<HitObject>
            {
                HitObjects = new List<HitObject>
                {
                    new HitCircle { StartTime = 0, Position = new Vector2(0) },
                    new HitCircle { StartTime = 500, Position = new Vector2(200, 0) }
                }
            });
        }

        [Test]
        public void TestNoOverlapClose()
        {
            assertShouldProbablyOverlap(new Beatmap<HitObject>
            {
                HitObjects = new List<HitObject>
                {
                    new HitCircle { StartTime = 0, Position = new Vector2(0) },
                    new HitCircle { StartTime = 167, Position = new Vector2(200, 0) }
                }
            });
        }

        [Test]
        public void TestNoOverlapTooClose()
        {
            assertShouldOverlap(new Beatmap<HitObject>
            {
                HitObjects = new List<HitObject>
                {
                    new HitCircle { StartTime = 0, Position = new Vector2(0) },
                    new HitCircle { StartTime = 100, Position = new Vector2(200, 0) }
                }
            });
        }

        [Test]
        public void TestNoOverlapTooCloseExpert()
        {
            assertOk(new Beatmap<HitObject>
            {
                HitObjects = new List<HitObject>
                {
                    new HitCircle { StartTime = 0, Position = new Vector2(0) },
                    new HitCircle { StartTime = 100, Position = new Vector2(200, 0) }
                }
            }, DifficultyRating.Expert);
        }

        [Test]
        public void TestOverlapClose()
        {
            assertOk(new Beatmap<HitObject>
            {
                HitObjects = new List<HitObject>
                {
                    new HitCircle { StartTime = 0, Position = new Vector2(0) },
                    new HitCircle { StartTime = 167, Position = new Vector2(20, 0) }
                }
            });
        }

        [Test]
        public void TestOverlapFarApart()
        {
            assertShouldNotOverlap(new Beatmap<HitObject>
            {
                HitObjects = new List<HitObject>
                {
                    new HitCircle { StartTime = 0, Position = new Vector2(0) },
                    new HitCircle { StartTime = 500, Position = new Vector2(20, 0) }
                }
            });
        }

        [Test]
        public void TestAlmostOverlapFarApart()
        {
            assertOk(new Beatmap<HitObject>
            {
                HitObjects = new List<HitObject>
                {
                    // Default circle diameter is 128 px, but part of that is the fade/border of the circle.
                    // We want this to only be a problem when it actually looks like an overlap.
                    new HitCircle { StartTime = 0, Position = new Vector2(0) },
                    new HitCircle { StartTime = 500, Position = new Vector2(125, 0) }
                }
            });
        }

        [Test]
        public void TestAlmostNotOverlapFarApart()
        {
            assertShouldNotOverlap(new Beatmap<HitObject>
            {
                HitObjects = new List<HitObject>
                {
                    new HitCircle { StartTime = 0, Position = new Vector2(0) },
                    new HitCircle { StartTime = 500, Position = new Vector2(110, 0) }
                }
            });
        }

        [Test]
        public void TestOverlapFarApartExpert()
        {
            assertOk(new Beatmap<HitObject>
            {
                HitObjects = new List<HitObject>
                {
                    new HitCircle { StartTime = 0, Position = new Vector2(0) },
                    new HitCircle { StartTime = 500, Position = new Vector2(20, 0) }
                }
            }, DifficultyRating.Expert);
        }

        [Test]
        public void TestOverlapTooFarApart()
        {
            // Far apart enough to where the objects are not visible at the same time, and so overlapping is fine.
            assertOk(new Beatmap<HitObject>
            {
                HitObjects = new List<HitObject>
                {
                    new HitCircle { StartTime = 0, Position = new Vector2(0) },
                    new HitCircle { StartTime = 2000, Position = new Vector2(20, 0) }
                }
            });
        }

        [Test]
        public void TestSliderTailOverlapFarApart()
        {
            assertShouldNotOverlap(new Beatmap<HitObject>
            {
                HitObjects = new List<HitObject>
                {
                    getSliderMock(startTime: 0, endTime: 500, startPosition: new Vector2(0, 0), endPosition: new Vector2(100, 0)).Object,
                    new HitCircle { StartTime = 1000, Position = new Vector2(120, 0) }
                }
            });
        }

        [Test]
        public void TestSliderTailOverlapClose()
        {
            assertOk(new Beatmap<HitObject>
            {
                HitObjects = new List<HitObject>
                {
                    getSliderMock(startTime: 0, endTime: 900, startPosition: new Vector2(0, 0), endPosition: new Vector2(100, 0)).Object,
                    new HitCircle { StartTime = 1000, Position = new Vector2(120, 0) }
                }
            });
        }

        [Test]
        public void TestSliderTailNoOverlapFarApart()
        {
            assertOk(new Beatmap<HitObject>
            {
                HitObjects = new List<HitObject>
                {
                    getSliderMock(startTime: 0, endTime: 500, startPosition: new Vector2(0, 0), endPosition: new Vector2(100, 0)).Object,
                    new HitCircle { StartTime = 1000, Position = new Vector2(300, 0) }
                }
            });
        }

        [Test]
        public void TestSliderTailNoOverlapClose()
        {
            // If these were circles they would need to overlap, but overlapping with slider tails is not required.
            assertOk(new Beatmap<HitObject>
            {
                HitObjects = new List<HitObject>
                {
                    getSliderMock(startTime: 0, endTime: 900, startPosition: new Vector2(0, 0), endPosition: new Vector2(100, 0)).Object,
                    new HitCircle { StartTime = 1000, Position = new Vector2(300, 0) }
                }
            });
        }

        private Mock<Slider> getSliderMock(double startTime, double endTime, Vector2 startPosition, Vector2 endPosition)
        {
            var mockSlider = new Mock<Slider>();
            mockSlider.SetupGet(s => s.StartTime).Returns(startTime);
            mockSlider.SetupGet(s => s.Position).Returns(startPosition);
            mockSlider.SetupGet(s => s.EndPosition).Returns(endPosition);
            mockSlider.As<IHasDuration>().Setup(d => d.EndTime).Returns(endTime);

            return mockSlider;
        }

        private void assertOk(IBeatmap beatmap, DifficultyRating difficultyRating = DifficultyRating.Easy)
        {
            var context = new BeatmapVerifierContext(beatmap, new TestWorkingBeatmap(beatmap), difficultyRating);
            Assert.That(check.Run(context), Is.Empty);
        }

        private void assertShouldProbablyOverlap(IBeatmap beatmap, int count = 1)
        {
            var context = new BeatmapVerifierContext(beatmap, new TestWorkingBeatmap(beatmap), DifficultyRating.Easy);
            var issues = check.Run(context).ToList();

            Assert.That(issues, Has.Count.EqualTo(count));
            Assert.That(issues.All(issue => issue.Template is CheckLowDiffOverlaps.IssueTemplateShouldProbablyOverlap));
        }

        private void assertShouldOverlap(IBeatmap beatmap, int count = 1)
        {
            var context = new BeatmapVerifierContext(beatmap, new TestWorkingBeatmap(beatmap), DifficultyRating.Easy);
            var issues = check.Run(context).ToList();

            Assert.That(issues, Has.Count.EqualTo(count));
            Assert.That(issues.All(issue => issue.Template is CheckLowDiffOverlaps.IssueTemplateShouldOverlap));
        }

        private void assertShouldNotOverlap(IBeatmap beatmap, int count = 1)
        {
            var context = new BeatmapVerifierContext(beatmap, new TestWorkingBeatmap(beatmap), DifficultyRating.Easy);
            var issues = check.Run(context).ToList();

            Assert.That(issues, Has.Count.EqualTo(count));
            Assert.That(issues.All(issue => issue.Template is CheckLowDiffOverlaps.IssueTemplateShouldNotOverlap));
        }
    }
}
