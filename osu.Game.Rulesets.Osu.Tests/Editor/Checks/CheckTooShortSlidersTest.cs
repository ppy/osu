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

namespace osu.Game.Rulesets.Osu.Tests.Editor.Checks
{
    [TestFixture]
    public class CheckTooShortSlidersTest
    {
        private CheckTooShortSliders check;

        [SetUp]
        public void Setup()
        {
            check = new CheckTooShortSliders();
        }

        [Test]
        public void TestRegularSlider()
        {
            assertOk(new Beatmap<HitObject>
            {
                HitObjects = new List<HitObject> { getSliderMock(duration: 210).Object }
            }, DifficultyRating.Easy);
        }

        [Test]
        public void TestVeryShortSlider()
        {
            assertVeryShort(new Beatmap<HitObject>
            {
                HitObjects = new List<HitObject> { getSliderMock(duration: 140).Object }
            }, DifficultyRating.Easy);
        }

        [Test]
        public void TestVeryShortSliderWithRepeats()
        {
            // Ensure we're looking at the span duration and not the duration as a whole.
            // We have 4 spans; 560 / 4 = 140, so should be equivalent to the other test.
            assertVeryShort(new Beatmap<HitObject>
            {
                HitObjects = new List<HitObject> { getSliderMock(duration: 560, repeats: 3).Object }
            }, DifficultyRating.Easy);
        }

        [Test]
        public void TestTooShortSlider()
        {
            assertTooShort(new Beatmap<HitObject>
            {
                HitObjects = new List<HitObject> { getSliderMock(duration: 70).Object }
            }, DifficultyRating.Easy);
        }

        [Test]
        public void TestTooShortSliderHardDifficulty()
        {
            assertOk(new Beatmap<HitObject>
            {
                HitObjects = new List<HitObject> { getSliderMock(duration: 70).Object }
            }, DifficultyRating.Hard);
        }

        private Mock<Slider> getSliderMock(double duration, int repeats = 0)
        {
            var mockSlider = new Mock<Slider>();
            mockSlider.SetupGet(p => p.StartTime).Returns(0);
            mockSlider.As<IHasRepeats>().Setup(r => r.RepeatCount).Returns(repeats);
            mockSlider.SetupGet(p => p.EndTime).Returns(duration);

            return mockSlider;
        }

        private void assertOk(IBeatmap beatmap, DifficultyRating difficultyRating)
        {
            Assert.That(check.Run(beatmap, getContext(beatmap, difficultyRating)), Is.Empty);
        }

        private void assertVeryShort(IBeatmap beatmap, DifficultyRating difficultyRating)
        {
            var issues = check.Run(beatmap, getContext(beatmap, difficultyRating)).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues.Single().Template is CheckTooShortSliders.IssueTemplateVeryShort);
        }

        private void assertTooShort(IBeatmap beatmap, DifficultyRating difficultyRating)
        {
            var issues = check.Run(beatmap, getContext(beatmap, difficultyRating)).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues.Single().Template is CheckTooShortSliders.IssueTemplateTooShort);
        }

        private IBeatmapVerifier.Context getContext(IBeatmap beatmap, DifficultyRating difficultyRating)
        {
            return new IBeatmapVerifier.Context(new TestWorkingBeatmap(beatmap), difficultyRating);
        }
    }
}
