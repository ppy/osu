// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Checks;
using osu.Game.Rulesets.Objects;
using osu.Game.Tests.Beatmaps;

namespace osu.Game.Tests.Editing.Checks
{
    [TestFixture]
    public class CheckFilePresenceTest
    {
        private CheckBackgroundPresence check;
        private IBeatmap beatmap;

        [SetUp]
        public void Setup()
        {
            var file = CheckTestHelpers.CreateMockFile("jpg");

            check = new CheckBackgroundPresence();
            beatmap = new Beatmap<HitObject>
            {
                BeatmapInfo = new BeatmapInfo
                {
                    Metadata = new BeatmapMetadata { BackgroundFile = file.Filename },
                    BeatmapSet = new BeatmapSetInfo
                    {
                        Files = { file }
                    }
                }
            };
        }

        [Test]
        public void TestBackgroundSetAndInFiles()
        {
            var context = new BeatmapVerifierContext(beatmap, new TestWorkingBeatmap(beatmap));
            Assert.That(check.Run(context), Is.Empty);
        }

        [Test]
        public void TestBackgroundSetAndNotInFiles()
        {
            beatmap.BeatmapInfo.BeatmapSet?.Files.Clear();

            var context = new BeatmapVerifierContext(beatmap, new TestWorkingBeatmap(beatmap));
            var issues = check.Run(context).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues.Single().Template is CheckFilePresence.IssueTemplateDoesNotExist);
        }

        [Test]
        public void TestBackgroundNotSet()
        {
            beatmap.Metadata.BackgroundFile = string.Empty;

            var context = new BeatmapVerifierContext(beatmap, new TestWorkingBeatmap(beatmap));
            var issues = check.Run(context).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues.Single().Template is CheckFilePresence.IssueTemplateNoneSet);
        }
    }
}
