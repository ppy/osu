// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Checks.Components;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Edit.Checks;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Tests.Beatmaps;

namespace osu.Game.Rulesets.Osu.Tests.Editor.Checks
{
    [TestFixture]
    public class CheckSpinnerRecoveryTimeTest
    {
        private CheckSpinnerRecoveryTime check = null!;

        [SetUp]
        public void Setup()
        {
            check = new CheckSpinnerRecoveryTime();
        }

        [Test]
        public void TestEasyOk()
        {
            Assert.That(runScenario(DifficultyRating.Easy, 1000, 3000), Is.Empty);
        }

        [Test]
        public void TestEasyFail()
        {
            var issues = runScenario(DifficultyRating.Easy, 1000, 2500).ToList();
            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues.First().Template is CheckSpinnerRecoveryTime.IssueTemplateSpinnerRecoveryTooShort);
        }

        [Test]
        public void TestNormalOk()
        {
            Assert.That(runScenario(DifficultyRating.Normal, 1000, 2000), Is.Empty);
        }

        [Test]
        public void TestNormalFail()
        {
            var issues = runScenario(DifficultyRating.Normal, 1000, 1500).ToList();
            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues.First().Template is CheckSpinnerRecoveryTime.IssueTemplateSpinnerRecoveryTooShort);
        }

        [Test]
        public void TestHardOk()
        {
            Assert.That(runScenario(DifficultyRating.Hard, 1000, 1500), Is.Empty);
        }

        [Test]
        public void TestHardFail()
        {
            var issues = runScenario(DifficultyRating.Hard, 1000, 1250).ToList();
            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues.First().Template is CheckSpinnerRecoveryTime.IssueTemplateSpinnerRecoveryTooShort);
        }

        [Test]
        public void TestEverythingElseOk()
        {
            Assert.That(runScenario(DifficultyRating.Insane, 1000, 1001), Is.Empty);
            Assert.That(runScenario(DifficultyRating.Expert, 1000, 1001), Is.Empty);
            Assert.That(runScenario(DifficultyRating.ExpertPlus, 1000, 1001), Is.Empty);
        }

        private IEnumerable<Issue> runScenario(DifficultyRating rating, double spinnerEnd, double nextObjectStart)
        {
            Spinner spinner = new Spinner { StartTime = 0, EndTime = spinnerEnd };
            HitObject nextObject = new HitObject { StartTime = nextObjectStart };
            var hitObjects = new List<HitObject> { spinner, nextObject };

            TimingControlPoint tp = new TimingControlPoint { BeatLength = 500 };
            ControlPointInfo controlPoints = new ControlPointInfo();
            controlPoints.Add(0, tp);

            var beatmap = new Beatmap { HitObjects = hitObjects, ControlPointInfo = controlPoints };
            var context = new BeatmapVerifierContext(beatmap, new TestWorkingBeatmap(beatmap), rating);

            return check.Run(context);
        }
    }
}
