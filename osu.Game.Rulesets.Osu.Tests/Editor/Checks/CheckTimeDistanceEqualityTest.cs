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
    public class CheckTimeDistanceEqualityTest
    {
        private CheckTimeDistanceEquality check = null!;

        [SetUp]
        public void Setup()
        {
            check = new CheckTimeDistanceEquality();
        }

        [Test]
        public void TestCirclesEquidistant()
        {
            assertOk(new Beatmap<HitObject>
            {
                HitObjects = new List<HitObject>
                {
                    new HitCircle { StartTime = 0, Position = new Vector2(0) },
                    new HitCircle { StartTime = 500, Position = new Vector2(50, 0) },
                    new HitCircle { StartTime = 1000, Position = new Vector2(100, 0) },
                    new HitCircle { StartTime = 1500, Position = new Vector2(150, 0) }
                }
            });
        }

        [Test]
        public void TestCirclesOneSlightlyOff()
        {
            assertWarning(new Beatmap<HitObject>
            {
                HitObjects = new List<HitObject>
                {
                    new HitCircle { StartTime = 0, Position = new Vector2(0) },
                    new HitCircle { StartTime = 500, Position = new Vector2(50, 0) },
                    new HitCircle { StartTime = 1000, Position = new Vector2(80, 0) }, // Distance a quite low compared to previous.
                    new HitCircle { StartTime = 1500, Position = new Vector2(130, 0) }
                }
            });
        }

        [Test]
        public void TestCirclesOneOff()
        {
            assertProblem(new Beatmap<HitObject>
            {
                HitObjects = new List<HitObject>
                {
                    new HitCircle { StartTime = 0, Position = new Vector2(0) },
                    new HitCircle { StartTime = 500, Position = new Vector2(50, 0) },
                    new HitCircle { StartTime = 1000, Position = new Vector2(150, 0) }, // Twice the regular spacing.
                    new HitCircle { StartTime = 1500, Position = new Vector2(100, 0) }
                }
            });
        }

        [Test]
        public void TestCirclesTwoOff()
        {
            assertProblem(new Beatmap<HitObject>
            {
                HitObjects = new List<HitObject>
                {
                    new HitCircle { StartTime = 0, Position = new Vector2(0) },
                    new HitCircle { StartTime = 500, Position = new Vector2(50, 0) },
                    new HitCircle { StartTime = 1000, Position = new Vector2(150, 0) }, // Twice the regular spacing.
                    new HitCircle { StartTime = 1500, Position = new Vector2(250, 0) } // Also twice the regular spacing.
                }
            }, count: 2);
        }

        [Test]
        public void TestCirclesStacked()
        {
            assertOk(new Beatmap<HitObject>
            {
                HitObjects = new List<HitObject>
                {
                    new HitCircle { StartTime = 0, Position = new Vector2(0) },
                    new HitCircle { StartTime = 500, Position = new Vector2(50, 0) },
                    new HitCircle { StartTime = 1000, Position = new Vector2(50, 0) }, // Stacked, is fine.
                    new HitCircle { StartTime = 1500, Position = new Vector2(100, 0) }
                }
            });
        }

        [Test]
        public void TestCirclesStacking()
        {
            assertWarning(new Beatmap<HitObject>
            {
                HitObjects = new List<HitObject>
                {
                    new HitCircle { StartTime = 0, Position = new Vector2(0) },
                    new HitCircle { StartTime = 500, Position = new Vector2(50, 0) },
                    new HitCircle { StartTime = 1000, Position = new Vector2(50, 0), StackHeight = 1 },
                    new HitCircle { StartTime = 1500, Position = new Vector2(50, 0), StackHeight = 2 },
                    new HitCircle { StartTime = 2000, Position = new Vector2(50, 0), StackHeight = 3 },
                    new HitCircle { StartTime = 2500, Position = new Vector2(50, 0), StackHeight = 4 }, // Ends up far from (50; 0), causing irregular spacing.
                    new HitCircle { StartTime = 3000, Position = new Vector2(100, 0) }
                }
            });
        }

        [Test]
        public void TestCirclesHalfStack()
        {
            assertOk(new Beatmap<HitObject>
            {
                HitObjects = new List<HitObject>
                {
                    new HitCircle { StartTime = 0, Position = new Vector2(0) },
                    new HitCircle { StartTime = 500, Position = new Vector2(50, 0) },
                    new HitCircle { StartTime = 1000, Position = new Vector2(55, 0) }, // Basically stacked, so is fine.
                    new HitCircle { StartTime = 1500, Position = new Vector2(105, 0) }
                }
            });
        }

        [Test]
        public void TestCirclesPartialOverlap()
        {
            assertProblem(new Beatmap<HitObject>
            {
                HitObjects = new List<HitObject>
                {
                    new HitCircle { StartTime = 0, Position = new Vector2(0) },
                    new HitCircle { StartTime = 500, Position = new Vector2(50, 0) },
                    new HitCircle { StartTime = 1000, Position = new Vector2(65, 0) }, // Really low distance compared to previous.
                    new HitCircle { StartTime = 1500, Position = new Vector2(115, 0) }
                }
            });
        }

        [Test]
        public void TestCirclesSlightlyDifferent()
        {
            assertOk(new Beatmap<HitObject>
            {
                HitObjects = new List<HitObject>
                {
                    // Does not need to be perfect, as long as the distance is approximately correct it's sight-readable.
                    new HitCircle { StartTime = 0, Position = new Vector2(0) },
                    new HitCircle { StartTime = 500, Position = new Vector2(52, 0) },
                    new HitCircle { StartTime = 1000, Position = new Vector2(97, 0) },
                    new HitCircle { StartTime = 1500, Position = new Vector2(165, 0) }
                }
            });
        }

        [Test]
        public void TestCirclesSlowlyChanging()
        {
            const float multiplier = 1.2f;

            assertOk(new Beatmap<HitObject>
            {
                HitObjects = new List<HitObject>
                {
                    new HitCircle { StartTime = 0, Position = new Vector2(0) },
                    new HitCircle { StartTime = 500, Position = new Vector2(50, 0) },
                    new HitCircle { StartTime = 1000, Position = new Vector2(50 + 50 * multiplier, 0) },
                    // This gap would be a warning if it weren't for the previous pushing the average spacing up.
                    new HitCircle { StartTime = 1500, Position = new Vector2(50 + 50 * multiplier + 50 * multiplier * multiplier, 0) }
                }
            });
        }

        [Test]
        public void TestCirclesQuicklyChanging()
        {
            const float multiplier = 1.6f;

            var beatmap = new Beatmap<HitObject>
            {
                HitObjects = new List<HitObject>
                {
                    new HitCircle { StartTime = 0, Position = new Vector2(0) },
                    new HitCircle { StartTime = 500, Position = new Vector2(50, 0) },
                    new HitCircle { StartTime = 1000, Position = new Vector2(50 + 50 * multiplier, 0) }, // Warning
                    new HitCircle { StartTime = 1500, Position = new Vector2(50 + 50 * multiplier + 50 * multiplier * multiplier, 0) } // Problem
                }
            };

            var context = new BeatmapVerifierContext(beatmap, new TestWorkingBeatmap(beatmap), DifficultyRating.Easy);
            var issues = check.Run(context).ToList();

            Assert.That(issues, Has.Count.EqualTo(2));
            Assert.That(issues.First().Template is CheckTimeDistanceEquality.IssueTemplateIrregularSpacingWarning);
            Assert.That(issues.Last().Template is CheckTimeDistanceEquality.IssueTemplateIrregularSpacingProblem);
        }

        [Test]
        public void TestCirclesTooFarApart()
        {
            assertOk(new Beatmap<HitObject>
            {
                HitObjects = new List<HitObject>
                {
                    new HitCircle { StartTime = 0, Position = new Vector2(0) },
                    new HitCircle { StartTime = 500, Position = new Vector2(50, 0) },
                    new HitCircle { StartTime = 4000, Position = new Vector2(200, 0) }, // 2 seconds apart from previous, so can start from wherever.
                    new HitCircle { StartTime = 4500, Position = new Vector2(250, 0) }
                }
            });
        }

        [Test]
        public void TestCirclesOneOffExpert()
        {
            assertOk(new Beatmap<HitObject>
            {
                HitObjects = new List<HitObject>
                {
                    new HitCircle { StartTime = 0, Position = new Vector2(0) },
                    new HitCircle { StartTime = 500, Position = new Vector2(50, 0) },
                    new HitCircle { StartTime = 1000, Position = new Vector2(150, 0) }, // Jumps are allowed in higher difficulties.
                    new HitCircle { StartTime = 1500, Position = new Vector2(100, 0) }
                }
            }, DifficultyRating.Expert);
        }

        [Test]
        public void TestSpinner()
        {
            assertOk(new Beatmap<HitObject>
            {
                HitObjects = new List<HitObject>
                {
                    new HitCircle { StartTime = 0, Position = new Vector2(0) },
                    new HitCircle { StartTime = 500, Position = new Vector2(50, 0) },
                    new Spinner { StartTime = 500, EndTime = 1000 }, // Distance to and from the spinner should be ignored. If it isn't this should give a problem.
                    new HitCircle { StartTime = 1500, Position = new Vector2(100, 0) },
                    new HitCircle { StartTime = 2000, Position = new Vector2(150, 0) }
                }
            });
        }

        [Test]
        public void TestSliders()
        {
            assertOk(new Beatmap<HitObject>
            {
                HitObjects = new List<HitObject>
                {
                    new HitCircle { StartTime = 0, Position = new Vector2(0) },
                    new HitCircle { StartTime = 500, Position = new Vector2(50, 0) },
                    getSliderMock(startTime: 1000, endTime: 1500, startPosition: new Vector2(100, 0), endPosition: new Vector2(150, 0)).Object,
                    getSliderMock(startTime: 2000, endTime: 2500, startPosition: new Vector2(200, 0), endPosition: new Vector2(250, 0)).Object,
                    new HitCircle { StartTime = 2500, Position = new Vector2(300, 0) }
                }
            });
        }

        [Test]
        public void TestSlidersOneOff()
        {
            assertProblem(new Beatmap<HitObject>
            {
                HitObjects = new List<HitObject>
                {
                    new HitCircle { StartTime = 0, Position = new Vector2(0) },
                    new HitCircle { StartTime = 500, Position = new Vector2(50, 0) },
                    getSliderMock(startTime: 1000, endTime: 1500, startPosition: new Vector2(100, 0), endPosition: new Vector2(150, 0)).Object,
                    getSliderMock(startTime: 2000, endTime: 2500, startPosition: new Vector2(250, 0), endPosition: new Vector2(300, 0)).Object, // Twice the spacing.
                    new HitCircle { StartTime = 2500, Position = new Vector2(300, 0) }
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

        private void assertWarning(IBeatmap beatmap, int count = 1)
        {
            var context = new BeatmapVerifierContext(beatmap, new TestWorkingBeatmap(beatmap), DifficultyRating.Easy);
            var issues = check.Run(context).ToList();

            Assert.That(issues, Has.Count.EqualTo(count));
            Assert.That(issues.All(issue => issue.Template is CheckTimeDistanceEquality.IssueTemplateIrregularSpacingWarning));
        }

        private void assertProblem(IBeatmap beatmap, int count = 1)
        {
            var context = new BeatmapVerifierContext(beatmap, new TestWorkingBeatmap(beatmap), DifficultyRating.Easy);
            var issues = check.Run(context).ToList();

            Assert.That(issues, Has.Count.EqualTo(count));
            Assert.That(issues.All(issue => issue.Template is CheckTimeDistanceEquality.IssueTemplateIrregularSpacingProblem));
        }
    }
}
