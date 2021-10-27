// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
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
using FileInfo = osu.Game.IO.FileInfo;

namespace osu.Game.Tests.Editing.Checks
{
    [TestFixture]
    public class CheckTooShortAudioFilesTest
    {
        private CheckTooShortAudioFiles check;
        private IBeatmap beatmap;

        [SetUp]
        public void Setup()
        {
            check = new CheckTooShortAudioFiles();
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
                                Filename = "abc123.wav",
                                FileInfo = new FileInfo { Hash = "abcdef" }
                            }
                        })
                    }
                }
            };

            // 0 = No output device. This still allows decoding.
            if (!Bass.Init(0) && Bass.LastError != Errors.Already)
                throw new AudioException("Could not initialize Bass.");
        }

        [Test]
        public void TestDifferentExtension()
        {
            beatmap.BeatmapInfo.BeatmapSet.Files.Clear();
            beatmap.BeatmapInfo.BeatmapSet.Files.Add(new BeatmapSetFileInfo
            {
                Filename = "abc123.jpg",
                FileInfo = new FileInfo { Hash = "abcdef" }
            });

            // Should fail to load, but not produce an error due to the extension not being expected to load.
            Assert.IsEmpty(check.Run(getContext(null, allowMissing: true)));
        }

        [Test]
        public void TestRegularAudioFile()
        {
            using (var resourceStream = TestResources.OpenResource("Samples/test-sample.mp3"))
            {
                Assert.IsEmpty(check.Run(getContext(resourceStream)));
            }
        }

        [Test]
        public void TestBlankAudioFile()
        {
            using (var resourceStream = TestResources.OpenResource("Samples/blank.wav"))
            {
                // This is a 0 ms duration audio file, commonly used to silence sliderslides/ticks, and so should be fine.
                Assert.IsEmpty(check.Run(getContext(resourceStream)));
            }
        }

        [Test]
        public void TestTooShortAudioFile()
        {
            using (var resourceStream = TestResources.OpenResource("Samples/test-sample-cut.mp3"))
            {
                var issues = check.Run(getContext(resourceStream)).ToList();

                Assert.That(issues, Has.Count.EqualTo(1));
                Assert.That(issues.Single().Template is CheckTooShortAudioFiles.IssueTemplateTooShort);
            }
        }

        [Test]
        public void TestMissingAudioFile()
        {
            using (var resourceStream = TestResources.OpenResource("Samples/missing.mp3"))
            {
                Assert.IsEmpty(check.Run(getContext(resourceStream, allowMissing: true)));
            }
        }

        [Test]
        public void TestCorruptAudioFile()
        {
            using (var resourceStream = TestResources.OpenResource("Samples/corrupt.wav"))
            {
                var issues = check.Run(getContext(resourceStream)).ToList();

                Assert.That(issues, Has.Count.EqualTo(1));
                Assert.That(issues.Single().Template is CheckTooShortAudioFiles.IssueTemplateBadFormat);
            }
        }

        private BeatmapVerifierContext getContext(Stream resourceStream, bool allowMissing = false)
        {
            var mockWorkingBeatmap = new Mock<TestWorkingBeatmap>(beatmap, null, null);
            mockWorkingBeatmap.Setup(w => w.GetStream(It.IsAny<string>())).Returns(resourceStream);

            return new BeatmapVerifierContext(beatmap, mockWorkingBeatmap.Object);
        }
    }
}
