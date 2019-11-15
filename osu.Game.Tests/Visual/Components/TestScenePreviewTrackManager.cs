// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics.Containers;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using static osu.Game.Tests.Visual.Components.TestScenePreviewTrackManager.TestPreviewTrackManager;

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
            AddUntilStep("wait loaded", () => track.IsLoaded);
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

            AddUntilStep("wait loaded", () => track1.IsLoaded && track2.IsLoaded);

            AddStep("start track 1", () => track1.Start());
            AddStep("start track 2", () => track2.Start());
            AddAssert("track 1 stopped", () => !track1.IsRunning);
            AddAssert("track 2 started", () => track2.IsRunning);
            AddStep("start track 1", () => track1.Start());
            AddAssert("track 2 stopped", () => !track2.IsRunning);
            AddAssert("track 1 started", () => track1.IsRunning);
        }

        [Test]
        public void TestCancelFromOwner()
        {
            PreviewTrack track = null;

            AddStep("get track", () => track = getOwnedTrack());
            AddUntilStep("wait loaded", () => track.IsLoaded);
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
            AddUntilStep("wait loaded", () => track.IsLoaded);
            AddStep("start", () => track.Start());
            AddStep("attempt stop", () => trackManager.StopAnyPlaying(this));
            AddAssert("not stopped", () => track.IsRunning);
            AddStep("stop by true owner", () => trackManager.StopAnyPlaying(owner));
            AddAssert("stopped", () => !track.IsRunning);
        }

        [Test]
        public void TestNonPresentTrack()
        {
            TestPreviewTrack track = null;

            AddStep("get non-present track", () =>
            {
                Add(new TestTrackOwner(track = getTrack()));
                track.Alpha = 0;
            });
            AddUntilStep("wait loaded", () => track.IsLoaded);
            AddStep("start", () => track.Start());
            AddStep("seek to end", () => track.Track.Seek(track.Track.Length));
            AddAssert("track stopped", () => !track.IsRunning);
        }

        private TestPreviewTrack getTrack() => (TestPreviewTrack)trackManager.Get(null);

        private TestPreviewTrack getOwnedTrack()
        {
            var track = getTrack();

            LoadComponentAsync(track, Add);

            return track;
        }

        private class TestTrackOwner : CompositeDrawable, IPreviewTrackOwner
        {
            private readonly PreviewTrack track;

            public TestTrackOwner(PreviewTrack track)
            {
                this.track = track;
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                LoadComponentAsync(track, AddInternal);
            }

            protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
            {
                var dependencies = new DependencyContainer(base.CreateChildDependencies(parent));
                dependencies.CacheAs<IPreviewTrackOwner>(this);
                return dependencies;
            }
        }

        public class TestPreviewTrackManager : PreviewTrackManager
        {
            protected override TrackManagerPreviewTrack CreatePreviewTrack(BeatmapSetInfo beatmapSetInfo, ITrackStore trackStore) => new TestPreviewTrack(beatmapSetInfo, trackStore);

            public class TestPreviewTrack : TrackManagerPreviewTrack
            {
                private readonly ITrackStore trackManager;

                public new Track Track => base.Track;

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
