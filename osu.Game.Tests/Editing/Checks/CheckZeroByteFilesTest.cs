// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Moq;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Checks;
using osu.Game.Rulesets.Objects;
using FileInfo = osu.Game.IO.FileInfo;

namespace osu.Game.Tests.Editing.Checks
{
    [TestFixture]
    public class CheckZeroByteFilesTest
    {
        private CheckZeroByteFiles check;
        private IBeatmap beatmap;

        [SetUp]
        public void Setup()
        {
            check = new CheckZeroByteFiles();
            beatmap = new Beatmap<HitObject>
            {
                BeatmapInfo = new BeatmapInfo
                {
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
        public void TestNonZeroBytes()
        {
            Assert.IsEmpty(check.Run(getContext(byteLength: 44)));
        }

        [Test]
        public void TestZeroBytes()
        {
            var issues = check.Run(getContext(byteLength: 0)).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues.Single().Template is CheckZeroByteFiles.IssueTemplateZeroBytes);
        }

        [Test]
        public void TestMissing()
        {
            Assert.IsEmpty(check.Run(getContextMissing()));
        }

        private BeatmapVerifierContext getContext(long byteLength)
        {
            var mockStream = new Mock<Stream>();
            mockStream.Setup(s => s.Length).Returns(byteLength);

            var mockWorkingBeatmap = new Mock<IWorkingBeatmap>();
            mockWorkingBeatmap.Setup(w => w.GetStream(It.IsAny<string>())).Returns(mockStream.Object);

            return new BeatmapVerifierContext(beatmap, mockWorkingBeatmap.Object);
        }

        private BeatmapVerifierContext getContextMissing()
        {
            var mockWorkingBeatmap = new Mock<IWorkingBeatmap>();
            mockWorkingBeatmap.Setup(w => w.GetStream(It.IsAny<string>())).Returns((Stream)null);

            return new BeatmapVerifierContext(beatmap, mockWorkingBeatmap.Object);
        }
    }
}
