// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using ManagedBass;
using Moq;
using NUnit.Framework;
using osu.Framework.Audio.Track;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Checks;
using osu.Game.Rulesets.Objects;
using osu.Game.Tests.Resources;
using osu.Game.Tests.Visual;
using osuTK.Audio;

namespace osu.Game.Tests.Editing.Checks
{
    [TestFixture]
    public class CheckAudioQualityTest
    {
        private CheckAudioQuality check = null!;
        private IBeatmap beatmap = null!;

        [SetUp]
        public void Setup()
        {
            check = new CheckAudioQuality();
            beatmap = new Beatmap<HitObject>
            {
                BeatmapInfo = new BeatmapInfo
                {
                    Metadata = new BeatmapMetadata()
                }
            };

            // 0 = No output device. This still allows decoding.
            if (!Bass.Init(0) && Bass.LastError != Errors.Already)
                throw new AudioException("Could not initialize Bass.");
        }

        [Test]
        public void TestMissing()
        {
            // While this is a problem, it is out of scope for this check and is caught by a different one.
            beatmap.Metadata.AudioFile = string.Empty;

            var mock = new Mock<IWorkingBeatmap>();
            mock.SetupGet(w => w.Beatmap).Returns(beatmap);
            mock.SetupGet(w => w.Track).Returns((Track)null!);

            Assert.That(check.Run(new BeatmapVerifierContext(beatmap, mock.Object)), Is.Empty);
        }

        [Test]
        public void TestAcceptable()
        {
            var context = getContext(192);

            Assert.That(check.Run(context), Is.Empty);
        }

        [Test]
        public void TestAcceptableOgg()
        {
            var context = getContext(208, useOgg: true);

            Assert.That(check.Run(context), Is.Empty);
        }

        [Test]
        public void TestNullBitrate()
        {
            var context = getContext(null);

            var issues = check.Run(context).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues.Single().Template is CheckAudioQuality.IssueTemplateNoBitrate);
        }

        [Test]
        public void TestZeroBitrate()
        {
            var context = getContext(0);

            var issues = check.Run(context).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues.Single().Template is CheckAudioQuality.IssueTemplateNoBitrate);
        }

        [Test]
        public void TestTooHighBitrate()
        {
            var context = getContext(320);

            var issues = check.Run(context).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues.Single().Template is CheckAudioQuality.IssueTemplateTooHighBitrate);
        }

        [Test]
        public void TestTooHighBitrateOgg()
        {
            var context = getContext(250, useOgg: true);

            var issues = check.Run(context).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues.Single().Template is CheckAudioQuality.IssueTemplateTooHighBitrate);
        }

        [Test]
        public void TestTooLowBitrate()
        {
            var context = getContext(64);

            var issues = check.Run(context).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues.Single().Template is CheckAudioQuality.IssueTemplateTooLowBitrate);
        }

        private BeatmapVerifierContext getContext(int? audioBitrate, bool useOgg = false)
        {
            // Update the audio filename and beatmapset files based on the format being tested
            string audioFileName = useOgg ? "abc123.ogg" : "abc123.mp3";
            string fileExtension = useOgg ? "ogg" : "mp3";

            beatmap.Metadata.AudioFile = audioFileName;
            beatmap.BeatmapInfo.BeatmapSet = new BeatmapSetInfo
            {
                Files = { CheckTestHelpers.CreateMockFile(fileExtension) }
            };

            return new BeatmapVerifierContext(beatmap, getMockWorkingBeatmap(audioBitrate, useOgg).Object);
        }

        /// <summary>
        /// Returns the mock of the working beatmap with the given audio properties.
        /// </summary>
        /// <param name="audioBitrate">The bitrate of the audio file the beatmap uses.</param>
        /// <param name="useOgg">Whether to use an OGG sample instead of MP3.</param>
        private Mock<IWorkingBeatmap> getMockWorkingBeatmap(int? audioBitrate, bool useOgg = false)
        {
            var mockTrack = new Mock<OsuTestScene.ClockBackedTestWorkingBeatmap.TrackVirtualManual>(new FramedClock(), "virtual");
            mockTrack.SetupGet(t => t.Bitrate).Returns(audioBitrate);

            // Use real audio samples for format detection
            string samplePath = useOgg ? "Samples/test-sample.ogg" : "Samples/test-sample-cut.mp3";

            var mockWorkingBeatmap = new Mock<IWorkingBeatmap>();
            mockWorkingBeatmap.SetupGet(w => w.Beatmap).Returns(beatmap);
            mockWorkingBeatmap.SetupGet(w => w.Track).Returns(mockTrack.Object);

            // Return a fresh stream each time GetStream is called to avoid disposed stream issues
            mockWorkingBeatmap.Setup(w => w.GetStream(It.IsAny<string>())).Returns(() => TestResources.OpenResource(samplePath));

            return mockWorkingBeatmap;
        }
    }
}
