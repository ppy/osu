// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using Moq;
using NUnit.Framework;
using osu.Framework.Audio.Track;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Edit;
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
            beatmap.Metadata.AudioFile = string.Empty;

            var mock = new Mock<IWorkingBeatmap>();
            mock.SetupGet(w => w.Beatmap).Returns(beatmap);
            mock.SetupGet(w => w.Track).Returns((Track)null);

            Assert.That(check.Run(new BeatmapVerifierContext(beatmap, mock.Object)), Is.Empty);
        }

        [Test]
        public void TestAcceptable()
        {
            var context = getContext(192);

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
        public void TestTooLowBitrate()
        {
            var context = getContext(64);

            var issues = check.Run(context).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues.Single().Template is CheckAudioQuality.IssueTemplateTooLowBitrate);
        }

        private BeatmapVerifierContext getContext(int? audioBitrate)
        {
            return new BeatmapVerifierContext(beatmap, getMockWorkingBeatmap(audioBitrate).Object);
        }

        /// <summary>
        /// Returns the mock of the working beatmap with the given audio properties.
        /// </summary>
        /// <param name="audioBitrate">The bitrate of the audio file the beatmap uses.</param>
        private Mock<IWorkingBeatmap> getMockWorkingBeatmap(int? audioBitrate)
        {
            var mockTrack = new Mock<Track>();
            mockTrack.SetupGet(t => t.Bitrate).Returns(audioBitrate);

            var mockWorkingBeatmap = new Mock<IWorkingBeatmap>();
            mockWorkingBeatmap.SetupGet(w => w.Beatmap).Returns(beatmap);
            mockWorkingBeatmap.SetupGet(w => w.Track).Returns(mockTrack.Object);

            return mockWorkingBeatmap;
        }
    }
}
