// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
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
    public class CheckTooShortAudioFilesTest
    {
        private CheckTooShortAudioFiles check = null!;
        private IBeatmap beatmap = null!;

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
                        Files = { CheckTestHelpers.CreateMockFile("wav") }
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
            Debug.Assert(beatmap.BeatmapInfo.BeatmapSet != null);

            beatmap.BeatmapInfo.BeatmapSet.Files.Clear();
            beatmap.BeatmapInfo.BeatmapSet.Files.Add(CheckTestHelpers.CreateMockFile("jpg"));

            // Should fail to load, but not produce an error due to the extension not being expected to load.
            Assert.IsEmpty(check.Run(getContext(null)));
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
                Assert.IsEmpty(check.Run(getContext(resourceStream)));
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
