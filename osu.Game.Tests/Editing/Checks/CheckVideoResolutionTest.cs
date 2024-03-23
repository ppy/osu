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
    public class CheckVideoResolutionTest
    {
        private CheckVideoResolution check = null!;

        private IBeatmap beatmap = null!;

        [SetUp]
        public void Setup()
        {
            check = new CheckVideoResolution();
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
        public void TestNoVideo()
        {
            beatmap.BeatmapInfo.BeatmapSet?.Files.Clear();

            var issues = check.Run(getContext(null)).ToList();

            Assert.That(issues, Has.Count.EqualTo(0));
        }

        [Test]
        public void TestVideoAcceptableResolution()
        {
            using (var resourceStream = TestResources.OpenResource("Videos/test-video.mp4"))
            {
                var issues = check.Run(getContext(resourceStream)).ToList();
                Assert.That(issues, Has.Count.EqualTo(0));
            }
        }

        [Test]
        public void TestVideoHighResolution()
        {
            using (var resourceStream = TestResources.OpenResource("Videos/test-video-resolution-high.mp4"))
            {
                var issues = check.Run(getContext(resourceStream)).ToList();

                Assert.That(issues, Has.Count.EqualTo(1));
                Assert.That(issues.Single().Template is CheckVideoResolution.IssueTemplateHighResolution);
            }
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
