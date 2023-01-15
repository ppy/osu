// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.IO;
using System.Linq;
using Moq;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Checks;
using osu.Game.Rulesets.Objects;
using osu.Game.Storyboards;
using osu.Game.Tests.Beatmaps;
using osu.Game.Tests.Resources;

namespace osu.Game.Tests.Editing.Checks
{
    [TestFixture]
    public class CheckAudioInVideoTest
    {
        private CheckAudioInVideo check = null!;
        private IBeatmap beatmap = null!;

        [SetUp]
        public void Setup()
        {
            check = new CheckAudioInVideo();
            beatmap = new Beatmap<HitObject>
            {
                BeatmapInfo = new BeatmapInfo
                {
                    BeatmapSet = new BeatmapSetInfo
                    {
                        Files =
                        {
                            CheckTestHelpers.CreateMockFile("mp4"),
                        }
                    }
                }
            };
        }

        [Test]
        public void TestRegularVideoFile()
        {
            using (var resourceStream = TestResources.OpenResource("Videos/test-video.mp4"))
                Assert.IsEmpty(check.Run(getContext(resourceStream)));
        }

        [Test]
        public void TestVideoFileWithAudio()
        {
            using (var resourceStream = TestResources.OpenResource("Videos/test-video-with-audio.mp4"))
            {
                var issues = check.Run(getContext(resourceStream)).ToList();

                Assert.That(issues, Has.Count.EqualTo(1));
                Assert.That(issues.Single().Template is CheckAudioInVideo.IssueTemplateHasAudioTrack);
            }
        }

        [Test]
        public void TestVideoFileWithTrackButNoAudio()
        {
            using (var resourceStream = TestResources.OpenResource("Videos/test-video-with-track-but-no-audio.mp4"))
            {
                var issues = check.Run(getContext(resourceStream)).ToList();

                Assert.That(issues, Has.Count.EqualTo(1));
                Assert.That(issues.Single().Template is CheckAudioInVideo.IssueTemplateHasAudioTrack);
            }
        }

        [Test]
        public void TestMissingFile()
        {
            beatmap.BeatmapInfo.BeatmapSet?.Files.Clear();

            var issues = check.Run(getContext(null)).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues.Single().Template is CheckAudioInVideo.IssueTemplateMissingFile);
        }

        private BeatmapVerifierContext getContext(Stream? resourceStream)
        {
            var storyboard = new Storyboard();
            var layer = storyboard.GetLayer("Video");
            layer.Add(new StoryboardVideo("abc123.mp4", 0));

            var mockWorkingBeatmap = new Mock<TestWorkingBeatmap>(beatmap, null, null);
            mockWorkingBeatmap.Setup(w => w.GetStream(It.IsAny<string>())).Returns(resourceStream);
            mockWorkingBeatmap.As<IWorkingBeatmap>().SetupGet(w => w.Storyboard).Returns(storyboard);

            return new BeatmapVerifierContext(beatmap, mockWorkingBeatmap.Object);
        }
    }
}
