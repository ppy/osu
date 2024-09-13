// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Osu.Edit.Checks;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Edit.Checks;
using osu.Game.Rulesets.Edit;
using osu.Game.Tests.Beatmaps;
using System.Linq;

namespace osu.Game.Rulesets.Osu.Tests.Editor.Checks
{
    [TestFixture]
    public class CheckOsuAbnormalDifficultySettingsTest
    {
        private CheckOsuAbnormalDifficultySettings check = null!;

        private readonly IBeatmap beatmap = new Beatmap<HitObject>();

        [SetUp]
        public void Setup()
        {
            check = new CheckOsuAbnormalDifficultySettings();

            beatmap.Difficulty = new BeatmapDifficulty
            {
                ApproachRate = 5,
                CircleSize = 5,
                DrainRate = 5,
                OverallDifficulty = 5,
            };
        }

        [Test]
        public void TestNormalSettings()
        {
            var context = getContext();
            var issues = check.Run(context).ToList();

            Assert.That(issues, Has.Count.EqualTo(0));
        }

        [Test]
        public void TestApproachRateTwoDecimals()
        {
            beatmap.Difficulty.ApproachRate = 5.55f;

            var context = getContext();
            var issues = check.Run(context).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues.Single().Template is CheckAbnormalDifficultySettings.IssueTemplateMoreThanOneDecimal);
        }

        [Test]
        public void TestCircleSizeTwoDecimals()
        {
            beatmap.Difficulty.CircleSize = 5.55f;

            var context = getContext();
            var issues = check.Run(context).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues.Single().Template is CheckAbnormalDifficultySettings.IssueTemplateMoreThanOneDecimal);
        }

        [Test]
        public void TestDrainRateTwoDecimals()
        {
            beatmap.Difficulty.DrainRate = 5.55f;

            var context = getContext();
            var issues = check.Run(context).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues.Single().Template is CheckAbnormalDifficultySettings.IssueTemplateMoreThanOneDecimal);
        }

        [Test]
        public void TestOverallDifficultyTwoDecimals()
        {
            beatmap.Difficulty.OverallDifficulty = 5.55f;

            var context = getContext();
            var issues = check.Run(context).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues.Single().Template is CheckAbnormalDifficultySettings.IssueTemplateMoreThanOneDecimal);
        }

        [Test]
        public void TestApproachRateUnder()
        {
            beatmap.Difficulty.ApproachRate = -10;

            var context = getContext();
            var issues = check.Run(context).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues.Single().Template is CheckAbnormalDifficultySettings.IssueTemplateOutOfRange);
        }

        [Test]
        public void TestCircleSizeUnder()
        {
            beatmap.Difficulty.CircleSize = -10;

            var context = getContext();
            var issues = check.Run(context).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues.Single().Template is CheckAbnormalDifficultySettings.IssueTemplateOutOfRange);
        }

        [Test]
        public void TestDrainRateUnder()
        {
            beatmap.Difficulty.DrainRate = -10;

            var context = getContext();
            var issues = check.Run(context).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues.Single().Template is CheckAbnormalDifficultySettings.IssueTemplateOutOfRange);
        }

        [Test]
        public void TestOverallDifficultyUnder()
        {
            beatmap.Difficulty.OverallDifficulty = -10;

            var context = getContext();
            var issues = check.Run(context).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues.Single().Template is CheckAbnormalDifficultySettings.IssueTemplateOutOfRange);
        }

        [Test]
        public void TestApproachRateOver()
        {
            beatmap.Difficulty.ApproachRate = 20;

            var context = getContext();
            var issues = check.Run(context).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues.Single().Template is CheckAbnormalDifficultySettings.IssueTemplateOutOfRange);
        }

        [Test]
        public void TestCircleSizeOver()
        {
            beatmap.Difficulty.CircleSize = 20;

            var context = getContext();
            var issues = check.Run(context).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues.Single().Template is CheckAbnormalDifficultySettings.IssueTemplateOutOfRange);
        }

        [Test]
        public void TestDrainRateOver()
        {
            beatmap.Difficulty.DrainRate = 20;

            var context = getContext();
            var issues = check.Run(context).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues.Single().Template is CheckAbnormalDifficultySettings.IssueTemplateOutOfRange);
        }

        [Test]
        public void TestOverallDifficultyOver()
        {
            beatmap.Difficulty.OverallDifficulty = 20;

            var context = getContext();
            var issues = check.Run(context).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues.Single().Template is CheckAbnormalDifficultySettings.IssueTemplateOutOfRange);
        }

        private BeatmapVerifierContext getContext()
        {
            return new BeatmapVerifierContext(beatmap, new TestWorkingBeatmap(beatmap));
        }
    }
}
