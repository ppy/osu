// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Taiko.Edit.Checks;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Edit.Checks;
using osu.Game.Rulesets.Edit;
using osu.Game.Tests.Beatmaps;
using System.Linq;

namespace osu.Game.Rulesets.Taiko.Tests.Editor.Checks
{
    [TestFixture]
    public class CheckTaikoAbnormalDifficultySettingsTest
    {
        private CheckTaikoAbnormalDifficultySettings check = null!;

        private readonly IBeatmap beatmap = new Beatmap<HitObject>();

        [SetUp]
        public void Setup()
        {
            check = new CheckTaikoAbnormalDifficultySettings();

            beatmap.BeatmapInfo.Ruleset = new TaikoRuleset().RulesetInfo;
            beatmap.Difficulty = new BeatmapDifficulty
            {
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
        public void TestOverallDifficultyTwoDecimals()
        {
            beatmap.Difficulty.OverallDifficulty = 5.55f;

            var context = getContext();
            var issues = check.Run(context).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues.Single().Template is CheckAbnormalDifficultySettings.IssueTemplateMoreThanOneDecimal);
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
