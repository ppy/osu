// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using System.Linq;
using Moq;
using NUnit.Framework;
using osu.Framework.Graphics.Rendering.Dummy;
using osu.Framework.Graphics.Textures;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Checks;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Tests.Editing.Checks
{
    [TestFixture]
    public class CheckBackgroundQualityTest
    {
        private CheckBackgroundQuality check = null!;
        private IBeatmap beatmap = null!;

        [SetUp]
        public void Setup()
        {
            var file = CheckTestHelpers.CreateMockFile("jpg");

            check = new CheckBackgroundQuality();
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
        public void TestMissing()
        {
            // While this is a problem, it is out of scope for this check and is caught by a different one.
            beatmap.Metadata.BackgroundFile = string.Empty;
            var context = getContext(null!, new MemoryStream(Array.Empty<byte>()));

            Assert.That(check.Run(context), Is.Empty);
        }

        [Test]
        public void TestAcceptable()
        {
            var context = getContext(new DummyRenderer().CreateTexture(1920, 1080));

            Assert.That(check.Run(context), Is.Empty);
        }

        [Test]
        public void TestTooHighResolution()
        {
            var context = getContext(new DummyRenderer().CreateTexture(3840, 2160));

            var issues = check.Run(context).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues.Single().Template is CheckBackgroundQuality.IssueTemplateTooHighResolution);
        }

        [Test]
        public void TestLowResolution()
        {
            var context = getContext(new DummyRenderer().CreateTexture(640, 480));

            var issues = check.Run(context).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues.Single().Template is CheckBackgroundQuality.IssueTemplateLowResolution);
        }

        [Test]
        public void TestTooLowResolution()
        {
            var context = getContext(new DummyRenderer().CreateTexture(100, 100));

            var issues = check.Run(context).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues.Single().Template is CheckBackgroundQuality.IssueTemplateTooLowResolution);
        }

        [Test]
        public void TestTooUncompressed()
        {
            var context = getContext(new DummyRenderer().CreateTexture(1920, 1080), new MemoryStream(new byte[1024 * 1024 * 3]));

            var issues = check.Run(context).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues.Single().Template is CheckBackgroundQuality.IssueTemplateTooUncompressed);
        }

        [Test]
        public void TestStreamClosed()
        {
            var background = new DummyRenderer().CreateTexture(1920, 1080);
            var stream = new Mock<MemoryStream>(new byte[1024 * 1024]);

            var context = getContext(background, stream.Object);

            Assert.That(check.Run(context), Is.Empty);

            stream.Verify(x => x.Close(), Times.Once());
        }

        private BeatmapVerifierContext getContext(Texture background, Stream? stream = null)
        {
            return new BeatmapVerifierContext(beatmap, getMockWorkingBeatmap(background, stream).Object);
        }

        /// <summary>
        /// Returns the mock of the working beatmap with the given background and its file stream.
        /// </summary>
        /// <param name="background">The texture of the background.</param>
        /// <param name="stream">The stream representing the background file.</param>
        private Mock<IWorkingBeatmap> getMockWorkingBeatmap(Texture background, Stream? stream = null)
        {
            stream ??= new MemoryStream(new byte[1024 * 1024]);

            var mock = new Mock<IWorkingBeatmap>();
            mock.SetupGet(w => w.Beatmap).Returns(beatmap);
            mock.Setup(w => w.GetBackground()).Returns(background);
            mock.Setup(w => w.GetStream(It.IsAny<string>())).Returns(stream);

            return mock;
        }
    }
}
