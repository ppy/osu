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
    public class CheckHitsoundsFormatTest
    {
        private CheckHitsoundsFormat check = null!;

        private IBeatmap beatmap = null!;

        [SetUp]
        public void Setup()
        {
            check = new CheckHitsoundsFormat();
            beatmap = new Beatmap<HitObject>
            {
                BeatmapInfo = new BeatmapInfo
                {
                    BeatmapSet = new BeatmapSetInfo
                    {
                        Files = { CheckTestHelpers.CreateMockFile("wav") }
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
                var issues = check.Run(getContext(resourceStream)).ToList();
                Assert.That(issues, Has.Count.EqualTo(1));
                Assert.That(issues.Single().Template is CheckHitsoundsFormat.IssueTemplateIncorrectFormat);
            }
        }

        [Test]
        public void TestOggAudio()
        {
            using (var resourceStream = TestResources.OpenResource("Samples/test-sample.ogg"))
            {
                var issues = check.Run(getContext(resourceStream)).ToList();
                Assert.That(issues, Has.Count.EqualTo(0));
            }
        }

        [Test]
        public void TestWavAudio()
        {
            using (var resourceStream = TestResources.OpenResource("Samples/hitsound-delay.wav"))
            {
                var issues = check.Run(getContext(resourceStream)).ToList();
                Assert.That(issues, Has.Count.EqualTo(0));
            }
        }

        [Test]
        public void TestWebmAudio()
        {
            using (var resourceStream = TestResources.OpenResource("Samples/test-sample.webm"))
            {
                var issues = check.Run(getContext(resourceStream)).ToList();
                Assert.That(issues, Has.Count.EqualTo(1));
                Assert.That(issues.Single().Template is CheckHitsoundsFormat.IssueTemplateFormatUnsupported);
            }
        }

        [Test]
        public void TestNotAnAudioFile()
        {
            beatmap = new Beatmap<HitObject>
            {
                BeatmapInfo = new BeatmapInfo
                {
                    BeatmapSet = new BeatmapSetInfo
                    {
                        Files = { CheckTestHelpers.CreateMockFile("png") }
                    }
                }
            };

            using (var resourceStream = TestResources.OpenResource("Textures/test-image.png"))
            {
                var issues = check.Run(getContext(resourceStream)).ToList();
                Assert.That(issues, Has.Count.EqualTo(0));
            }
        }

        [Test]
        public void TestCorruptAudio()
        {
            using (var resourceStream = TestResources.OpenResource("Samples/corrupt.wav"))
            {
                var issues = check.Run(getContext(resourceStream)).ToList();
                Assert.That(issues, Has.Count.EqualTo(1));
                Assert.That(issues.Single().Template is CheckHitsoundsFormat.IssueTemplateFormatUnsupported);
            }
        }

        [Test]
        public void TestBeatmapAudioTracksExemptedFromCheck()
        {
            using (var resourceStream = TestResources.OpenResource("Samples/corrupt.wav"))
            {
                var beatmapSet = new BeatmapSetInfo
                {
                    Files =
                    {
                        CheckTestHelpers.CreateMockFile("wav"),
                        CheckTestHelpers.CreateMockFile("mp3")
                    }
                };

                var firstPlayable = new Beatmap<HitObject>
                {
                    BeatmapInfo = new BeatmapInfo
                    {
                        BeatmapSet = beatmapSet,
                        Metadata = new BeatmapMetadata { AudioFile = beatmapSet.Files[0].Filename }
                    }
                };
                var firstWorking = new Mock<TestWorkingBeatmap>(firstPlayable, null, null);
                firstWorking.Setup(w => w.GetStream(It.IsAny<string>())).Returns(resourceStream);

                var secondPlayable = new Beatmap<HitObject>
                {
                    BeatmapInfo = new BeatmapInfo
                    {
                        BeatmapSet = beatmapSet,
                        Metadata = new BeatmapMetadata { AudioFile = beatmapSet.Files[1].Filename }
                    }
                };
                var secondWorking = new Mock<TestWorkingBeatmap>(secondPlayable, null, null);
                secondWorking.Setup(w => w.GetStream(It.IsAny<string>())).Returns(resourceStream);

                var context = new BeatmapVerifierContext(
                    new BeatmapVerifierContext.VerifiedBeatmap(firstWorking.Object, firstPlayable),
                    [new BeatmapVerifierContext.VerifiedBeatmap(secondWorking.Object, secondPlayable)],
                    DifficultyRating.ExpertPlus);

                var issues = check.Run(context).ToList();
                Assert.That(issues, Is.Empty);
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
