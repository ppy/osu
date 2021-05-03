// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.IO;
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
            check = new CheckBackgroundPresence();
            beatmap = new Beatmap<HitObject>
            {
                BeatmapInfo = new BeatmapInfo
                {
                    Metadata = new BeatmapMetadata { BackgroundFile = "abc123.jpg" },
                    BeatmapSet = new BeatmapSetInfo
                    {
                        Files = new List<BeatmapSetFileInfo>(new[]
                        {
                            new BeatmapSetFileInfo
                            {
                                Filename = "abc123.jpg",
                                FileInfo = new FileInfo { Hash = "abcdef" }
                            }
                        })
                    }
                }
            };
        }

        [Test]
        public void TestBackgroundSetAndInFiles()
        {
            Assert.That(check.Run(beatmap, new TestWorkingBeatmap(beatmap)), Is.Empty);
        }

        [Test]
        public void TestBackgroundSetAndNotInFiles()
        {
            beatmap.BeatmapInfo.BeatmapSet.Files.Clear();

            var issues = check.Run(beatmap, new TestWorkingBeatmap(beatmap)).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues.Single().Template is CheckFilePresence.IssueTemplateDoesNotExist);
        }

        [Test]
        public void TestBackgroundNotSet()
        {
            beatmap.Metadata.BackgroundFile = null;

            var issues = check.Run(beatmap, new TestWorkingBeatmap(beatmap)).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues.Single().Template is CheckFilePresence.IssueTemplateNoneSet);
        }
    }
}
