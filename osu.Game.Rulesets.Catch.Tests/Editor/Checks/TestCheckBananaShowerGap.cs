// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Edit.Checks;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Tests.Beatmaps;

namespace osu.Game.Rulesets.Catch.Tests.Editor.Checks
{
    [TestFixture]
    public class TestCheckBananaShowerGap
    {
        private CheckBananaShowerGap check = null!;

        [SetUp]
        public void Setup()
        {
            check = new CheckBananaShowerGap();
        }

        [Test]
        public void TestAllowedSpinnerGaps()
        {
            assertOk(mockBeatmap(250, 1000, 1250), DifficultyRating.Easy);
            assertOk(mockBeatmap(250, 1000, 1250), DifficultyRating.Normal);
            assertOk(mockBeatmap(125, 1000, 1250), DifficultyRating.Hard);
            assertOk(mockBeatmap(125, 1000, 1125), DifficultyRating.Insane);
            assertOk(mockBeatmap(62, 1000, 1125), DifficultyRating.Expert);
            assertOk(mockBeatmap(62, 1000, 1125), DifficultyRating.ExpertPlus);
        }

        [Test]
        public void TestDisallowedSpinnerGapStart()
        {
            assertTooShortSpinnerStart(mockBeatmap(249, 1000, 1250), DifficultyRating.Easy);
            assertTooShortSpinnerStart(mockBeatmap(249, 1000, 1250), DifficultyRating.Normal);
            assertTooShortSpinnerStart(mockBeatmap(124, 1000, 1250), DifficultyRating.Hard);
            assertTooShortSpinnerStart(mockBeatmap(124, 1000, 1250), DifficultyRating.Insane);
            assertTooShortSpinnerStart(mockBeatmap(61, 1000, 1250), DifficultyRating.Expert);
            assertTooShortSpinnerStart(mockBeatmap(61, 1000, 1250), DifficultyRating.ExpertPlus);
        }

        [Test]
        public void TestDisallowedSpinnerGapEnd()
        {
            assertTooShortSpinnerEnd(mockBeatmap(250, 1000, 1249), DifficultyRating.Easy);
            assertTooShortSpinnerEnd(mockBeatmap(250, 1000, 1249), DifficultyRating.Normal);
            assertTooShortSpinnerEnd(mockBeatmap(125, 1000, 1249), DifficultyRating.Hard);
            assertTooShortSpinnerEnd(mockBeatmap(125, 1000, 1124), DifficultyRating.Insane);
            assertTooShortSpinnerEnd(mockBeatmap(62, 1000, 1124), DifficultyRating.Expert);
            assertTooShortSpinnerEnd(mockBeatmap(62, 1000, 1124), DifficultyRating.ExpertPlus);
        }

        [Test]
        public void TestConsecutiveSpinners()
        {
            var spinnerConsecutiveBeatmap = new Beatmap<HitObject>
            {
                HitObjects = new List<HitObject>
                {
                    new BananaShower { StartTime = 0, EndTime = 100, X = 0 },
                    new BananaShower { StartTime = 101, EndTime = 200, X = 0 },
                    new BananaShower { StartTime = 201, EndTime = 300, X = 0 }
                }
            };

            assertOk(spinnerConsecutiveBeatmap, DifficultyRating.Easy);
            assertOk(spinnerConsecutiveBeatmap, DifficultyRating.Normal);
            assertOk(spinnerConsecutiveBeatmap, DifficultyRating.Hard);
            assertOk(spinnerConsecutiveBeatmap, DifficultyRating.Insane);
            assertOk(spinnerConsecutiveBeatmap, DifficultyRating.Expert);
            assertOk(spinnerConsecutiveBeatmap, DifficultyRating.ExpertPlus);
        }

        private Beatmap<HitObject> mockBeatmap(double bananaShowerStart, double bananaShowerEnd, double nextFruitStart)
        {
            return new Beatmap<HitObject>
            {
                HitObjects = new List<HitObject>
                {
                    new Fruit { StartTime = 0, X = 0 },
                    new BananaShower { StartTime = bananaShowerStart, EndTime = bananaShowerEnd, X = 0 },
                    new Fruit { StartTime = nextFruitStart, X = 0 }
                }
            };
        }

        private void assertOk(IBeatmap beatmap, DifficultyRating difficultyRating)
        {
            var context = new BeatmapVerifierContext(beatmap, new TestWorkingBeatmap(beatmap), difficultyRating);
            Assert.That(check.Run(context), Is.Empty);
        }

        private void assertTooShortSpinnerStart(IBeatmap beatmap, DifficultyRating difficultyRating)
        {
            var context = new BeatmapVerifierContext(beatmap, new TestWorkingBeatmap(beatmap), difficultyRating);
            var issues = check.Run(context).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues.All(issue => issue.Template is CheckBananaShowerGap.IssueTemplateBananaShowerStartGap));
        }

        private void assertTooShortSpinnerEnd(IBeatmap beatmap, DifficultyRating difficultyRating)
        {
            var context = new BeatmapVerifierContext(beatmap, new TestWorkingBeatmap(beatmap), difficultyRating);
            var issues = check.Run(context).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues.All(issue => issue.Template is CheckBananaShowerGap.IssueTemplateBananaShowerEndGap));
        }
    }
}
