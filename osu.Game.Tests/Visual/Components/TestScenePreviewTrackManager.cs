// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics.Containers;
using osu.Game.Audio;
using osu.Game.Beatmaps;

namespace osu.Game.Tests.Visual.Components
{
    public class TestScenePreviewTrackManager : OsuTestScene, IPreviewTrackOwner
    {
        private readonly PreviewTrackManager trackManager = new TestPreviewTrackManager();

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            var dependencies = new DependencyContainer(base.CreateChildDependencies(parent));
            dependencies.CacheAs(trackManager);
            dependencies.CacheAs<IPreviewTrackOwner>(this);
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

        [Test]
        public void TestCancelFromOwner()
        {
            PreviewTrack track = null;

            AddStep("get track", () => track = getOwnedTrack());
            AddStep("start", () => track.Start());
            AddStep("stop by owner", () => trackManager.StopAnyPlaying(this));
            AddAssert("stopped", () => !track.IsRunning);
        }

        [Test]
        public void TestCancelFromNonOwner()
        {
            TestTrackOwner owner = null;
            PreviewTrack track = null;

            AddStep("get track", () => Add(owner = new TestTrackOwner(track = getTrack())));
            AddStep("start", () => track.Start());
            AddStep("attempt stop", () => trackManager.StopAnyPlaying(this));
            AddAssert("not stopped", () => track.IsRunning);
            AddStep("stop by true owner", () => trackManager.StopAnyPlaying(owner));
            AddAssert("stopped", () => !track.IsRunning);
        }

        private PreviewTrack getTrack() => trackManager.Get(null);

        private PreviewTrack getOwnedTrack()
        {
            var track = getTrack();

            Add(track);

            return track;
        }

        private class TestTrackOwner : CompositeDrawable, IPreviewTrackOwner
        {
            public TestTrackOwner(PreviewTrack track)
            {
                AddInternal(track);
            }

            protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
            {
                var dependencies = new DependencyContainer(base.CreateChildDependencies(parent));
                dependencies.CacheAs<IPreviewTrackOwner>(this);
                return dependencies;
            }
        }

        private class TestPreviewTrackManager : PreviewTrackManager
        {
            protected override TrackManagerPreviewTrack CreatePreviewTrack(BeatmapSetInfo beatmapSetInfo, ITrackStore trackStore) => new TestPreviewTrack(beatmapSetInfo, trackStore);

            protected class TestPreviewTrack : TrackManagerPreviewTrack
            {
                private readonly ITrackStore trackManager;

                public TestPreviewTrack(BeatmapSetInfo beatmapSetInfo, ITrackStore trackManager)
                    : base(beatmapSetInfo, trackManager)
                {
                    this.trackManager = trackManager;
                }

                protected override Track GetTrack() => trackManager.GetVirtual(100000);
            }
        }
    }
}
