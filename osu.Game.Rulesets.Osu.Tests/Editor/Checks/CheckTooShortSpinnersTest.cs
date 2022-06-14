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

namespace osu.Game.Rulesets.Osu.Tests.Editor.Checks
{
    [TestFixture]
    public class CheckTooShortSpinnersTest
    {
        private CheckTooShortSpinners check;
        private IBeatmapDifficultyInfo difficulty;

        [SetUp]
        public void Setup()
        {
            check = new CheckTooShortSpinners();
            difficulty = new BeatmapDifficulty();
        }

        [Test]
        public void TestLongSpinner()
        {
            Spinner spinner = new Spinner { StartTime = 0, Duration = 4000 };
            spinner.ApplyDefaults(new ControlPointInfo(), difficulty);

            assertOk(new List<HitObject> { spinner }, difficulty);
        }

        [Test]
        public void TestShortSpinner()
        {
            Spinner spinner = new Spinner { StartTime = 0, Duration = 750 };
            spinner.ApplyDefaults(new ControlPointInfo(), difficulty);

            assertOk(new List<HitObject> { spinner }, difficulty);
        }

        [Test]
        public void TestVeryShortSpinner()
        {
            // Spinners at a certain duration only get 1000 points if approached by auto at a certain angle, making it difficult to determine.
            Spinner spinner = new Spinner { StartTime = 0, Duration = 475 };
            spinner.ApplyDefaults(new ControlPointInfo(), difficulty);

            assertVeryShort(new List<HitObject> { spinner }, difficulty);
        }

        [Test]
        public void TestTooShortSpinner()
        {
            Spinner spinner = new Spinner { StartTime = 0, Duration = 400 };
            spinner.ApplyDefaults(new ControlPointInfo(), difficulty);

            assertTooShort(new List<HitObject> { spinner }, difficulty);
        }

        [Test]
        public void TestTooShortSpinnerVaryingOd()
        {
            const double duration = 450;

            var difficultyLowOd = new BeatmapDifficulty { OverallDifficulty = 1 };
            Spinner spinnerLowOd = new Spinner { StartTime = 0, Duration = duration };
            spinnerLowOd.ApplyDefaults(new ControlPointInfo(), difficultyLowOd);

            var difficultyHighOd = new BeatmapDifficulty { OverallDifficulty = 10 };
            Spinner spinnerHighOd = new Spinner { StartTime = 0, Duration = duration };
            spinnerHighOd.ApplyDefaults(new ControlPointInfo(), difficultyHighOd);

            assertOk(new List<HitObject> { spinnerLowOd }, difficultyLowOd);
            assertTooShort(new List<HitObject> { spinnerHighOd }, difficultyHighOd);
        }

        private void assertOk(List<HitObject> hitObjects, IBeatmapDifficultyInfo beatmapDifficulty)
        {
            Assert.That(check.Run(getContext(hitObjects, beatmapDifficulty)), Is.Empty);
        }

        private void assertVeryShort(List<HitObject> hitObjects, IBeatmapDifficultyInfo beatmapDifficulty)
        {
            var issues = check.Run(getContext(hitObjects, beatmapDifficulty)).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues.First().Template is CheckTooShortSpinners.IssueTemplateVeryShort);
        }

        private void assertTooShort(List<HitObject> hitObjects, IBeatmapDifficultyInfo beatmapDifficulty)
        {
            var issues = check.Run(getContext(hitObjects, beatmapDifficulty)).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues.First().Template is CheckTooShortSpinners.IssueTemplateTooShort);
        }

        private BeatmapVerifierContext getContext(List<HitObject> hitObjects, IBeatmapDifficultyInfo beatmapDifficulty)
        {
            var beatmap = new Beatmap<HitObject>
            {
                HitObjects = hitObjects,
                BeatmapInfo = new BeatmapInfo { Difficulty = new BeatmapDifficulty(beatmapDifficulty) }
            };

            return new BeatmapVerifierContext(beatmap, new TestWorkingBeatmap(beatmap));
        }
    }
}
