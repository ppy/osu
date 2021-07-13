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
            Assert.IsEmpty(check.Run(getContext("Samples/test-sample.mp3")));
        }

        [Test]
        public void TestBlankAudioFile()
        {
            // This is a 0 ms duration audio file, commonly used to silence sliderslides/ticks, and so should be fine.
            Assert.IsEmpty(check.Run(getContext("Samples/blank.wav")));
        }

        [Test]
        public void TestTooShortAudioFile()
        {
            var issues = check.Run(getContext("Samples/test-sample-cut.mp3")).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues.Single().Template is CheckTooShortAudioFiles.IssueTemplateTooShort);
        }

        [Test]
        public void TestMissingAudioFile()
        {
            Assert.IsEmpty(check.Run(getContext("Samples/missing.mp3", allowMissing: true)));
        }

        [Test]
        public void TestCorruptAudioFile()
        {
            var issues = check.Run(getContext("Samples/corrupt.wav")).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues.Single().Template is CheckTooShortAudioFiles.IssueTemplateBadFormat);
        }

        private BeatmapVerifierContext getContext(string resourceName, bool allowMissing = false)
        {
            Stream resourceStream = string.IsNullOrEmpty(resourceName) ? null : TestResources.OpenResource(resourceName);
            if (!allowMissing && resourceStream == null)
                throw new FileNotFoundException($"The requested test resource \"{resourceName}\" does not exist.");

            var mockWorkingBeatmap = new Mock<TestWorkingBeatmap>(beatmap, null, null);
            mockWorkingBeatmap.Setup(w => w.GetStream(It.IsAny<string>())).Returns(resourceStream);

            return new BeatmapVerifierContext(beatmap, mockWorkingBeatmap.Object);
        }
    }
}
