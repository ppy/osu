// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Checks;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Tests.Beatmaps;
using osuTK;

namespace osu.Game.Tests.Editing.Checks
{
    [TestFixture]
    public class CheckZeroLengthObjectsTest
    {
        private CheckZeroLengthObjects check = null!;

        [SetUp]
        public void Setup()
        {
            check = new CheckZeroLengthObjects();
        }

        [Test]
        public void TestCircle()
        {
            assertOk(new List<HitObject>
            {
                new HitCircle { StartTime = 1000, Position = new Vector2(0, 0) }
            });
        }

        [Test]
        public void TestRegularSlider()
        {
            assertOk(new List<HitObject>
            {
                getSliderMock(1000).Object
            });
        }

        [Test]
        public void TestZeroLengthSlider()
        {
            assertZeroLength(new List<HitObject>
            {
                getSliderMock(0).Object
            });
        }

        [Test]
        public void TestNegativeLengthSlider()
        {
            assertZeroLength(new List<HitObject>
            {
                getSliderMock(-1000).Object
            });
        }

        private Mock<Slider> getSliderMock(double duration)
        {
            var mockSlider = new Mock<Slider>();
            mockSlider.As<IHasDuration>().Setup(d => d.Duration).Returns(duration);

            return mockSlider;
        }

        private void assertOk(List<HitObject> hitObjects)
        {
            Assert.That(check.Run(getContext(hitObjects)), Is.Empty);
        }

        private void assertZeroLength(List<HitObject> hitObjects)
        {
            var issues = check.Run(getContext(hitObjects)).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues.First().Template is CheckZeroLengthObjects.IssueTemplateZeroLength);
        }

        private BeatmapVerifierContext getContext(List<HitObject> hitObjects)
        {
            var beatmap = new Beatmap<HitObject> { HitObjects = hitObjects };

            return new BeatmapVerifierContext(beatmap, new TestWorkingBeatmap(beatmap));
        }
    }
}
