// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using Moq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Checks;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Storyboards;
using osuTK;
using static osu.Game.Tests.Visual.OsuTestScene.ClockBackedTestWorkingBeatmap;

namespace osu.Game.Tests.Editing.Checks
{
    public class CheckUnusedAudioAtEndTest
    {
        private CheckUnusedAudioAtEnd check = null!;

        private IBeatmap beatmapNotFullyMapped = null!;

        private IBeatmap beatmapFullyMapped = null!;

        [SetUp]
        public void Setup()
        {
            check = new CheckUnusedAudioAtEnd();
            beatmapNotFullyMapped = new Beatmap<HitObject>
            {
                HitObjects =
                {
                    new HitCircle { StartTime = 0 },
                    new HitCircle { StartTime = 1_298 },
                },
                BeatmapInfo = new BeatmapInfo
                {
                    Metadata = new BeatmapMetadata { AudioFile = "abc123.jpg" }
                }
            };
            beatmapFullyMapped = new Beatmap<HitObject>
            {
                HitObjects =
                {
                    new HitCircle { StartTime = 0 },
                    new HitCircle { StartTime = 9000 },
                },
                BeatmapInfo = new BeatmapInfo
                {
                    Metadata = new BeatmapMetadata { AudioFile = "abc123.jpg" },
                }
            };
        }

        [Test]
        public void TestAudioNotFullyUsed()
        {
            var context = getContext(beatmapNotFullyMapped);
            var issues = check.Run(context).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues.Single().Template is CheckUnusedAudioAtEnd.IssueTemplateUnusedAudioAtEnd);
        }

        [Test]
        public void TestAudioNotFullyUsedWithVideo()
        {
            var storyboard = new Storyboard();

            var video = new StoryboardVideo("abc123.mp4", 0);

            storyboard.GetLayer("Video").Add(video);

            var mockWorkingBeatmap = getMockWorkingBeatmap(beatmapNotFullyMapped, storyboard);

            var context = getContext(beatmapNotFullyMapped, mockWorkingBeatmap);
            var issues = check.Run(context).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues.Single().Template is CheckUnusedAudioAtEnd.IssueTemplateUnusedAudioAtEndStoryboardOrVideo);
        }

        [Test]
        public void TestAudioNotFullyUsedWithStoryboardElement()
        {
            var storyboard = new Storyboard();

            var sprite = new StoryboardSprite("unknown", Anchor.TopLeft, Vector2.Zero);

            storyboard.GetLayer("Background").Add(sprite);

            var mockWorkingBeatmap = getMockWorkingBeatmap(beatmapNotFullyMapped, storyboard);

            var context = getContext(beatmapNotFullyMapped, mockWorkingBeatmap);
            var issues = check.Run(context).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues.Single().Template is CheckUnusedAudioAtEnd.IssueTemplateUnusedAudioAtEndStoryboardOrVideo);
        }

        [Test]
        public void TestAudioFullyUsed()
        {
            var context = getContext(beatmapFullyMapped);
            var issues = check.Run(context).ToList();

            Assert.That(issues, Has.Count.EqualTo(0));
        }

        private BeatmapVerifierContext getContext(IBeatmap beatmap)
        {
            return new BeatmapVerifierContext(beatmap, getMockWorkingBeatmap(beatmap, new Storyboard()).Object);
        }

        private BeatmapVerifierContext getContext(IBeatmap beatmap, Mock<IWorkingBeatmap> workingBeatmap)
        {
            return new BeatmapVerifierContext(beatmap, workingBeatmap.Object);
        }

        private Mock<IWorkingBeatmap> getMockWorkingBeatmap(IBeatmap beatmap, Storyboard storyboard)
        {
            var mockTrack = new TrackVirtualStore(new FramedClock()).GetVirtual(10000, "virtual");

            var mockWorkingBeatmap = new Mock<IWorkingBeatmap>();
            mockWorkingBeatmap.SetupGet(w => w.Beatmap).Returns(beatmap);
            mockWorkingBeatmap.SetupGet(w => w.Track).Returns(mockTrack);
            mockWorkingBeatmap.SetupGet(w => w.Storyboard).Returns(storyboard);

            return mockWorkingBeatmap;
        }
    }
}
