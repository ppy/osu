// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.IO;
using System.Linq;
using ManagedBass;
using Moq;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Checks;
using osu.Game.Rulesets.Objects;
using osu.Game.Tests.Beatmaps;
using osu.Game.Tests.Resources;
using osuTK.Audio;

namespace osu.Game.Tests.Editing.Checks
{
    [TestFixture]
    public partial class CheckSongFormatTest
    {
        private CheckSongFormat check = null!;

        private IBeatmap beatmap = null!;

        [SetUp]
        public void Setup()
        {
            check = new CheckSongFormat();
            beatmap = new Beatmap<HitObject>
            {
                BeatmapInfo = new BeatmapInfo
                {
                    BeatmapSet = new BeatmapSetInfo
                    {
                        Files = { CheckTestHelpers.CreateMockFile("mp3") }
                    }
                }
            };

            // 0 = No output device. This still allows decoding.
            if (!Bass.Init(0) && Bass.LastError != Errors.Already)
                throw new AudioException("Could not initialize Bass.");
        }

        [Test]
        public void TestMp3Audio()
        {
            using (var resourceStream = TestResources.OpenResource("Samples/test-sample-cut.mp3"))
            {
                beatmap.Metadata.AudioFile = "abc123.mp3";
                var issues = check.Run(getContext(resourceStream)).ToList();
                Assert.That(issues, Has.Count.EqualTo(0));
            }
        }

        [Test]
        public void TestOggAudio()
        {
            using (var resourceStream = TestResources.OpenResource("Samples/test-sample.ogg"))
            {
                beatmap.Metadata.AudioFile = "abc123.mp3";
                var issues = check.Run(getContext(resourceStream)).ToList();
                Assert.That(issues, Has.Count.EqualTo(0));
            }
        }

        [Test]
        public void TestWavAudio()
        {
            using (var resourceStream = TestResources.OpenResource("Samples/hitsound-delay.wav"))
            {
                beatmap.Metadata.AudioFile = "abc123.mp3";
                var issues = check.Run(getContext(resourceStream)).ToList();
                Assert.That(issues, Has.Count.EqualTo(1));
                Assert.That(issues.Single().Template is CheckSongFormat.IssueTemplateIncorrectFormat);
            }
        }

        [Test]
        public void TestWebmAudio()
        {
            using (var resourceStream = TestResources.OpenResource("Samples/test-sample.webm"))
            {
                beatmap.Metadata.AudioFile = "abc123.mp3";
                var issues = check.Run(getContext(resourceStream)).ToList();
                Assert.That(issues, Has.Count.EqualTo(1));
                Assert.That(issues.Single().Template is CheckSongFormat.IssueTemplateFormatUnsupported);
            }
        }

        [Test]
        public void TestCorruptAudio()
        {
            using (var resourceStream = TestResources.OpenResource("Samples/corrupt.wav"))
            {
                beatmap.Metadata.AudioFile = "abc123.mp3";
                var issues = check.Run(getContext(resourceStream)).ToList();
                Assert.That(issues, Has.Count.EqualTo(1));
                Assert.That(issues.Single().Template is CheckSongFormat.IssueTemplateFormatUnsupported);
            }
        }

        private BeatmapVerifierContext getContext(Stream? resourceStream)
        {
            var mockWorkingBeatmap = new Mock<TestWorkingBeatmap>(beatmap, null, null);
            mockWorkingBeatmap.Setup(w => w.GetStream(It.IsAny<string>())).Returns(resourceStream);

            return new BeatmapVerifierContext(beatmap, mockWorkingBeatmap.Object);
        }
    }
}
