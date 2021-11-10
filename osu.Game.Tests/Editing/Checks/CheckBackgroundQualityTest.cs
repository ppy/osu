// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Moq;
using NUnit.Framework;
using osu.Framework.Graphics.Textures;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Checks;
using osu.Game.Rulesets.Objects;
using FileInfo = osu.Game.IO.FileInfo;

namespace osu.Game.Tests.Editing.Checks
{
    [TestFixture]
    public class CheckBackgroundQualityTest
    {
        private CheckBackgroundQuality check;
        private IBeatmap beatmap;

        [SetUp]
        public void Setup()
        {
            check = new CheckBackgroundQuality();
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
                                FileInfo = new FileInfo
                                {
                                    Hash = "abcdef"
                                }
                            }
                        })
                    }
                }
            };
        }

        [Test]
        public void TestMissing()
        {
            // While this is a problem, it is out of scope for this check and is caught by a different one.
            beatmap.Metadata.BackgroundFile = string.Empty;
            var context = getContext(null, new MemoryStream(System.Array.Empty<byte>()));

            Assert.That(check.Run(context), Is.Empty);
        }

        [Test]
        public void TestAcceptable()
        {
            var context = getContext(new Texture(1920, 1080));

            Assert.That(check.Run(context), Is.Empty);
        }

        [Test]
        public void TestTooHighResolution()
        {
            var context = getContext(new Texture(3840, 2160));

            var issues = check.Run(context).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues.Single().Template is CheckBackgroundQuality.IssueTemplateTooHighResolution);
        }

        [Test]
        public void TestLowResolution()
        {
            var context = getContext(new Texture(640, 480));

            var issues = check.Run(context).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues.Single().Template is CheckBackgroundQuality.IssueTemplateLowResolution);
        }

        [Test]
        public void TestTooLowResolution()
        {
            var context = getContext(new Texture(100, 100));

            var issues = check.Run(context).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues.Single().Template is CheckBackgroundQuality.IssueTemplateTooLowResolution);
        }

        [Test]
        public void TestTooUncompressed()
        {
            var context = getContext(new Texture(1920, 1080), new MemoryStream(new byte[1024 * 1024 * 3]));

            var issues = check.Run(context).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues.Single().Template is CheckBackgroundQuality.IssueTemplateTooUncompressed);
        }

        [Test]
        public void TestStreamClosed()
        {
            var background = new Texture(1920, 1080);
            var stream = new Mock<MemoryStream>(new byte[1024 * 1024]);

            var context = getContext(background, stream.Object);

            Assert.That(check.Run(context), Is.Empty);

            stream.Verify(x => x.Close(), Times.Once());
        }

        private BeatmapVerifierContext getContext(Texture background, [CanBeNull] Stream stream = null)
        {
            return new BeatmapVerifierContext(beatmap, getMockWorkingBeatmap(background, stream).Object);
        }

        /// <summary>
        /// Returns the mock of the working beatmap with the given background and its file stream.
        /// </summary>
        /// <param name="background">The texture of the background.</param>
        /// <param name="stream">The stream representing the background file.</param>
        private Mock<IWorkingBeatmap> getMockWorkingBeatmap(Texture background, [CanBeNull] Stream stream = null)
        {
            stream ??= new MemoryStream(new byte[1024 * 1024]);

            var mock = new Mock<IWorkingBeatmap>();
            mock.SetupGet(w => w.Beatmap).Returns(beatmap);
            mock.SetupGet(w => w.Background).Returns(background);
            mock.Setup(w => w.GetStream(It.IsAny<string>())).Returns(stream);

            return mock;
        }
    }
}
