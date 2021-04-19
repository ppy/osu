// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using Moq;
using NUnit.Framework;
using osu.Framework.Audio.Track;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Edit.Checks;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Tests.Editing.Checks
{
    [TestFixture]
    public class CheckAudioQualityTest
    {
        private CheckAudioQuality check;
        private IBeatmap beatmap;

        [SetUp]
        public void Setup()
        {
            check = new CheckAudioQuality();
            beatmap = new Beatmap<HitObject>
            {
                BeatmapInfo = new BeatmapInfo
                {
                    Metadata = new BeatmapMetadata { AudioFile = "abc123.jpg" }
                }
            };
        }

        [Test]
        public void TestMissing()
        {
            // While this is a problem, it is out of scope for this check and is caught by a different one.
            beatmap.Metadata.AudioFile = null;

            var mock = new Mock<IWorkingBeatmap>();
            mock.SetupGet(_ => _.Beatmap).Returns(beatmap);
            mock.SetupGet(_ => _.Track).Returns((Track)null);

            Assert.That(check.Run(beatmap, mock.Object), Is.Empty);
        }

        [Test]
        public void TestAcceptable()
        {
            var mock = getMockWorkingBeatmap(192);

            Assert.That(check.Run(beatmap, mock.Object), Is.Empty);
        }

        [Test]
        public void TestNullBitrate()
        {
            var mock = getMockWorkingBeatmap(null);

            var issues = check.Run(beatmap, mock.Object).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues.Single().Template is CheckAudioQuality.IssueTemplateNoBitrate);
        }

        [Test]
        public void TestZeroBitrate()
        {
            var mock = getMockWorkingBeatmap(0);

            var issues = check.Run(beatmap, mock.Object).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues.Single().Template is CheckAudioQuality.IssueTemplateNoBitrate);
        }

        [Test]
        public void TestTooHighBitrate()
        {
            var mock = getMockWorkingBeatmap(320);

            var issues = check.Run(beatmap, mock.Object).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues.Single().Template is CheckAudioQuality.IssueTemplateTooHighBitrate);
        }

        [Test]
        public void TestTooLowBitrate()
        {
            var mock = getMockWorkingBeatmap(64);

            var issues = check.Run(beatmap, mock.Object).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues.Single().Template is CheckAudioQuality.IssueTemplateTooLowBitrate);
        }

        /// <summary>
        /// Returns the mock of the working beatmap with the given audio properties.
        /// </summary>
        /// <param name="audioBitrate">The bitrate of the audio file the beatmap uses.</param>
        private Mock<IWorkingBeatmap> getMockWorkingBeatmap(int? audioBitrate)
        {
            var mockTrack = new Mock<Track>();
            mockTrack.SetupGet(_ => _.Bitrate).Returns(audioBitrate);

            var mockWorkingBeatmap = new Mock<IWorkingBeatmap>();
            mockWorkingBeatmap.SetupGet(_ => _.Beatmap).Returns(beatmap);
            mockWorkingBeatmap.SetupGet(_ => _.Track).Returns(mockTrack.Object);

            return mockWorkingBeatmap;
        }
    }
}
