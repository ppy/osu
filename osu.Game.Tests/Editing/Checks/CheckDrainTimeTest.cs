// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Checks;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Tests.Beatmaps;

namespace osu.Game.Tests.Editing.Checks
{
    public class CheckDrainTimeTest
    {
        private CheckDrainTime check = null!;

        private IBeatmap beatmap = null!;

        [SetUp]
        public void Setup()
        {
            check = new CheckDrainTime();
        }

        [Test]
        public void TestDrainTimeShort()
        {
            setShortDrainTimeBeatmap();
            var content = new BeatmapVerifierContext(beatmap, new TestWorkingBeatmap(beatmap));

            var issues = check.Run(content).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues.Single().Template is CheckDrainTime.IssueTemplateTooShort);
        }

        private void setShortDrainTimeBeatmap()
        {
            beatmap = new Beatmap<HitObject>
            {
                HitObjects =
                {
                    new HitCircle()
                }
            };
        }
    }
}
