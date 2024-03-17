// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using Moq;
using NUnit.Framework;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Checks;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Objects;
using static osu.Game.Tests.Visual.OsuTestScene.ClockBackedTestWorkingBeatmap;

namespace osu.Game.Tests.Editing.Checks
{
    public class CheckUnusedAudioTest
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
                    Metadata = new BeatmapMetadata { AudioFile = "abc123.jpg" }
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
        public void TestAudioFullyUsed()
        {
            var context = getContext(beatmapFullyMapped);
            var issues = check.Run(context).ToList();

            Assert.That(issues, Has.Count.EqualTo(0));
        }

        private BeatmapVerifierContext getContext(IBeatmap beatmap)
        {
            return new BeatmapVerifierContext(beatmap, getMockWorkingBeatmap(beatmap).Object);
        }

        private Mock<IWorkingBeatmap> getMockWorkingBeatmap(IBeatmap beatmap)
        {
            var mockTrack = new TrackVirtualStore(new FramedClock()).GetVirtual(10000, "virtual");

            var mockWorkingBeatmap = new Mock<IWorkingBeatmap>();
            mockWorkingBeatmap.SetupGet(w => w.Beatmap).Returns(beatmap);
            mockWorkingBeatmap.SetupGet(w => w.Track).Returns(mockTrack);

            return mockWorkingBeatmap;
        }
    }
}
