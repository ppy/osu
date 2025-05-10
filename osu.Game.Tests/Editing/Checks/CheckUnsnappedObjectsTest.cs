// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Checks;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Tests.Beatmaps;

namespace osu.Game.Tests.Editing.Checks
{
    [TestFixture]
    public class CheckUnsnappedObjectsTest
    {
        private CheckUnsnappedObjects check = null!;
        private ControlPointInfo cpi = null!;

        [SetUp]
        public void Setup()
        {
            check = new CheckUnsnappedObjects();

            cpi = new ControlPointInfo();
            cpi.Add(100, new TimingControlPoint { BeatLength = 100 });
        }

        [Test]
        public void TestCircleSnapped()
        {
            assertOk(new List<HitObject>
            {
                new HitCircle { StartTime = 100 }
            });
        }

        [Test]
        public void TestCircleUnsnapped1Ms()
        {
            assert1Ms(new List<HitObject>
            {
                new HitCircle { StartTime = 101 }
            });

            assert1Ms(new List<HitObject>
            {
                new HitCircle { StartTime = 99 }
            });
        }

        [Test]
        public void TestCircleUnsnapped2Ms()
        {
            assert2Ms(new List<HitObject>
            {
                new HitCircle { StartTime = 102 }
            });

            assert2Ms(new List<HitObject>
            {
                new HitCircle { StartTime = 98 }
            });
        }

        [Test]
        public void TestSliderSnapped()
        {
            // Slider ends are naturally < 1 ms unsnapped because of how SV works.
            assertOk(new List<HitObject>
            {
                getSliderMock(startTime: 100, endTime: 400.75d).Object
            });
        }

        [Test]
        public void TestSliderUnsnapped1Ms()
        {
            assert1Ms(new List<HitObject>
            {
                getSliderMock(startTime: 101, endTime: 401.75d).Object
            }, count: 2);

            // End is only off by 0.25 ms, hence count 1.
            assert1Ms(new List<HitObject>
            {
                getSliderMock(startTime: 99, endTime: 399.75d).Object
            }, count: 1);
        }

        [Test]
        public void TestSliderUnsnapped2Ms()
        {
            assert2Ms(new List<HitObject>
            {
                getSliderMock(startTime: 102, endTime: 402.75d).Object
            }, count: 2);

            // Start and end are 2 ms and 1.25 ms off respectively, hence two different issues in one object.
            var hitObjects = new List<HitObject>
            {
                getSliderMock(startTime: 98, endTime: 398.75d).Object
            };

            var issues = check.Run(getContext(hitObjects)).ToList();

            Assert.That(issues, Has.Count.EqualTo(2));
            Assert.That(issues.Any(issue => issue.Template is CheckUnsnappedObjects.IssueTemplateSmallUnsnap));
            Assert.That(issues.Any(issue => issue.Template is CheckUnsnappedObjects.IssueTemplateLargeUnsnap));
        }

        private Mock<Slider> getSliderMock(double startTime, double endTime, int repeats = 0)
        {
            var mockSlider = new Mock<Slider>();
            mockSlider.SetupGet(s => s.StartTime).Returns(startTime);
            mockSlider.As<IHasRepeats>().Setup(r => r.RepeatCount).Returns(repeats);
            mockSlider.As<IHasDuration>().Setup(d => d.EndTime).Returns(endTime);

            return mockSlider;
        }

        private void assertOk(List<HitObject> hitObjects)
        {
            Assert.That(check.Run(getContext(hitObjects)), Is.Empty);
        }

        private void assert1Ms(List<HitObject> hitObjects, int count = 1)
        {
            var issues = check.Run(getContext(hitObjects)).ToList();

            Assert.That(issues, Has.Count.EqualTo(count));
            Assert.That(issues.All(issue => issue.Template is CheckUnsnappedObjects.IssueTemplateSmallUnsnap));
        }

        private void assert2Ms(List<HitObject> hitObjects, int count = 1)
        {
            var issues = check.Run(getContext(hitObjects)).ToList();

            Assert.That(issues, Has.Count.EqualTo(count));
            Assert.That(issues.All(issue => issue.Template is CheckUnsnappedObjects.IssueTemplateLargeUnsnap));
        }

        private BeatmapVerifierContext getContext(List<HitObject> hitObjects)
        {
            var beatmap = new Beatmap<HitObject>
            {
                ControlPointInfo = cpi,
                HitObjects = hitObjects
            };

            return new BeatmapVerifierContext(beatmap, new TestWorkingBeatmap(beatmap));
        }
    }
}
