// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Game.Audio;
using osu.Game.Beatmaps;

namespace osu.Game.Tests.Visual
{
    public class TestCasePreviewTrackManager : OsuTestCase
    {
        private readonly PreviewTrackManager trackManager = new TestPreviewTrackManager();

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            var dependencies = new DependencyContainer(base.CreateChildDependencies(parent));
            dependencies.CacheAs(trackManager);
            return dependencies;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Add(trackManager);
        }

        [Test]
        public void TestStartStop()
        {
            PreviewTrack track = null;

            AddStep("get track", () => track = getOwnedTrack());
            AddStep("start", () => track.Start());
            AddAssert("started", () => track.IsRunning);
            AddStep("stop", () => track.Stop());
            AddAssert("stopped", () => !track.IsRunning);
        }

        [Test]
        public void TestStartMultipleTracks()
        {
            PreviewTrack track1 = null;
            PreviewTrack track2 = null;

            AddStep("get tracks", () =>
            {
                track1 = getOwnedTrack();
                track2 = getOwnedTrack();
            });

            AddStep("start track 1", () => track1.Start());
            AddStep("start track 2", () => track2.Start());
            AddAssert("track 1 stopped", () => !track1.IsRunning);
            AddAssert("track 2 started", () => track2.IsRunning);
        }

        private PreviewTrack getTrack() => trackManager.Get(null);

        private PreviewTrack getOwnedTrack()
        {
            var track = getTrack();

            Add(track);

            return track;
        }

        private class TestPreviewTrackManager : PreviewTrackManager
        {
            protected override TrackManagerPreviewTrack CreatePreviewTrack(BeatmapSetInfo beatmapSetInfo, TrackManager trackManager) => new TestPreviewTrack(beatmapSetInfo, trackManager);

            protected class TestPreviewTrack : TrackManagerPreviewTrack
            {
                public TestPreviewTrack(BeatmapSetInfo beatmapSetInfo, TrackManager trackManager)
                    : base(beatmapSetInfo, trackManager)
                {
                }

                protected override Track GetTrack() => new TrackVirtual { Length = 100000 };
            }
        }
    }
}
