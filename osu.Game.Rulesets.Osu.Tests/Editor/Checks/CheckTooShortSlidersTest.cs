// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Edit.Checks;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Tests.Beatmaps;
using osuTK;

namespace osu.Game.Rulesets.Osu.Tests.Editor.Checks
{
    [TestFixture]
    public class CheckTooShortSlidersTest
    {
        private CheckTooShortSliders check = null!;

        [SetUp]
        public void Setup()
        {
            check = new CheckTooShortSliders();
        }

        [Test]
        public void TestLongSlider()
        {
            Slider slider = new Slider
            {
                StartTime = 0,
                RepeatCount = 0,
                Path = new SliderPath(new[]
                {
                    new PathControlPoint(new Vector2(0, 0)),
                    new PathControlPoint(new Vector2(100, 0))
                })
            };

            slider.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());

            assertOk(new List<HitObject> { slider });
        }

        [Test]
        public void TestShortSlider()
        {
            Slider slider = new Slider
            {
                StartTime = 0,
                RepeatCount = 0,
                Path = new SliderPath(new[]
                {
                    new PathControlPoint(new Vector2(0, 0)),
                    new PathControlPoint(new Vector2(25, 0))
                })
            };

            slider.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());

            assertOk(new List<HitObject> { slider });
        }

        [Test]
        public void TestTooShortSliderExpert()
        {
            Slider slider = new Slider
            {
                StartTime = 0,
                RepeatCount = 0,
                Path = new SliderPath(new[]
                {
                    new PathControlPoint(new Vector2(0, 0)),
                    new PathControlPoint(new Vector2(10, 0))
                })
            };

            slider.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());

            assertOk(new List<HitObject> { slider }, DifficultyRating.Expert);
        }

        [Test]
        public void TestTooShortSlider()
        {
            Slider slider = new Slider
            {
                StartTime = 0,
                RepeatCount = 0,
                Path = new SliderPath(new[]
                {
                    new PathControlPoint(new Vector2(0, 0)),
                    new PathControlPoint(new Vector2(10, 0))
                })
            };

            slider.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());

            assertTooShort(new List<HitObject> { slider });
        }

        [Test]
        public void TestTooShortSliderWithRepeats()
        {
            // Would be ok if we looked at the duration, but not if we look at the span duration.
            Slider slider = new Slider
            {
                StartTime = 0,
                RepeatCount = 2,
                Path = new SliderPath(new[]
                {
                    new PathControlPoint(new Vector2(0, 0)),
                    new PathControlPoint(new Vector2(10, 0))
                })
            };

            slider.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());

            assertTooShort(new List<HitObject> { slider });
        }

        private void assertOk(List<HitObject> hitObjects, DifficultyRating difficultyRating = DifficultyRating.Easy)
        {
            Assert.That(check.Run(getContext(hitObjects, difficultyRating)), Is.Empty);
        }

        private void assertTooShort(List<HitObject> hitObjects, DifficultyRating difficultyRating = DifficultyRating.Easy)
        {
            var issues = check.Run(getContext(hitObjects, difficultyRating)).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues.First().Template is CheckTooShortSliders.IssueTemplateTooShort);
        }

        private BeatmapVerifierContext getContext(List<HitObject> hitObjects, DifficultyRating difficultyRating)
        {
            var beatmap = new Beatmap<HitObject> { HitObjects = hitObjects };

            return new BeatmapVerifierContext(beatmap, new TestWorkingBeatmap(beatmap), difficultyRating);
        }
    }
}
