// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Edit.Checks;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Edit;
using osu.Game.Tests.Beatmaps;
using System.Linq;

namespace osu.Game.Tests.Editing.Checks
{
    [TestFixture]
    public class CheckAbnormalDifficultySettingsTest
    {
        private CheckAbnormalDifficultySettings check = null!;

        private IBeatmap beatmap = new Beatmap<HitObject>();

        [SetUp]
        public void Setup()
        {
            check = new CheckAbnormalDifficultySettings();
            beatmap.Difficulty = new();
        }

        [Test]
        public void TestSettingsNormal()
        {
            var context = getContext();
            var issues = check.Run(context).ToList();

            Assert.That(issues, Has.Count.EqualTo(0));
        }

        [Test]
        public void TestAllSettingsMoreThanOneDecimal()
        {
            beatmap.Difficulty = new()
            {
                ApproachRate = 5.55f,
                OverallDifficulty = 7.7777f,
                CircleSize = 4.444f,
                DrainRate = 1.1111111111f,
            };

            var context = getContext();
            var issues = check.Run(context).ToList();

            Assert.That(issues, Has.Count.EqualTo(4));
        }

        [Test]
        public void TestAllSettingsLessThanZero()
        {
            beatmap.Difficulty = new()
            {
                ApproachRate = -1,
                OverallDifficulty = -20,
                CircleSize = -11,
                DrainRate = -34,
            };

            var context = getContext();
            var issues = check.Run(context).ToList();

            Assert.That(issues, Has.Count.EqualTo(4));
        }

        [Test]
        public void TestAllSettingsHigherThanTen()
        {
            beatmap.Difficulty = new()
            {
                ApproachRate = 14,
                OverallDifficulty = 24,
                CircleSize = 30,
                DrainRate = 90,
            };

            var context = getContext();
            var issues = check.Run(context).ToList();

            Assert.That(issues, Has.Count.EqualTo(4));
        }

        private BeatmapVerifierContext getContext()
        {
            return new BeatmapVerifierContext(beatmap, new TestWorkingBeatmap(beatmap));
        }
    }
}
