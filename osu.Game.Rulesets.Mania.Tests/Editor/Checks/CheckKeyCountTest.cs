// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Tests.Beatmaps;
using osu.Game.Rulesets.Mania.Edit.Checks;
using System.Linq;

namespace osu.Game.Rulesets.Mania.Tests.Editor.Checks
{
    [TestFixture]
    public class CheckKeyCountTest
    {
        private CheckKeyCount check = null!;

        private IBeatmap beatmap = null!;

        [SetUp]
        public void Setup()
        {
            check = new CheckKeyCount();

            beatmap = new Beatmap<HitObject>
            {
                BeatmapInfo = new BeatmapInfo
                {
                    Ruleset = new ManiaRuleset().RulesetInfo
                }
            };
        }

        [Test]
        public void TestKeycountFour()
        {
            beatmap.Difficulty.CircleSize = 4;

            var context = getContext();
            var issues = check.Run(context).ToList();

            Assert.That(issues, Has.Count.EqualTo(0));
        }

        [Test]
        public void TestKeycountSmallerThanFour()
        {
            beatmap.Difficulty.CircleSize = 1;

            var context = getContext();
            var issues = check.Run(context).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues.Single().Template is CheckKeyCount.IssueTemplateKeycountTooLow);
        }

        private BeatmapVerifierContext getContext()
        {
            return new BeatmapVerifierContext(beatmap, new TestWorkingBeatmap(beatmap));
        }
    }
}
