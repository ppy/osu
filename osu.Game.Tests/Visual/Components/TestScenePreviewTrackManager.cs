// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics.Containers;
using osu.Game.Audio;
using osu.Game.Beatmaps;

namespace osu.Game.Tests.Visual.Components
{
    public class TestScenePreviewTrackManager : OsuTestScene, IPreviewTrackOwner
    {
        private readonly IAdjustableAudioComponent gameTrackAudio = new AudioAdjustments();

        private readonly TestPreviewTrackManager trackManager;

        private AudioManager audio;

        public TestScenePreviewTrackManager()
        {
            trackManager = new TestPreviewTrackManager(gameTrackAudio);
        }

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            var dependencies = new DependencyContainer(base.CreateChildDependencies(parent));
            dependencies.CacheAs(trackManager);
            dependencies.CacheAs<IPreviewTrackOwner>(this);
            return dependencies;
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            this.audio = audio;

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
            TestPreviewTrackManager.TestPreviewTrack track = null;

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

        /// <summary>
        /// Ensures that <see cref="PreviewTrackManager.CurrentTrack"/> changes correctly.
        /// </summary>
        [Test]
        public void TestCurrentTrackChanges()
        {
            PreviewTrack track = null;
            TestTrackOwner owner = null;

            AddStep("get track", () => Add(owner = new TestTrackOwner(track = getTrack())));
            AddUntilStep("wait loaded", () => track.IsLoaded);
            AddStep("start track", () => track.Start());
            AddAssert("current is track", () => trackManager.CurrentTrack == track);
            AddStep("pause manager updates", () => trackManager.AllowUpdate = false);
            AddStep("stop any playing", () => trackManager.StopAnyPlaying(owner));
            AddAssert("current not changed", () => trackManager.CurrentTrack == track);
            AddStep("resume manager updates", () => trackManager.AllowUpdate = true);
            AddAssert("current is null", () => trackManager.CurrentTrack == null);
        }

        /// <summary>
        /// Ensures that <see cref="PreviewTrackManager"/> mutes game-wide audio tracks correctly.
        /// </summary>
        [TestCase(false)]
        [TestCase(true)]
        public void TestEnsureMutingCorrectly(bool stopAnyPlaying)
        {
            PreviewTrack track = null;
            TestTrackOwner owner = null;

            AddStep("ensure volume not zero", () =>
            {
                if (audio.Volume.Value == 0)
                    audio.Volume.Value = 1;

                if (audio.VolumeTrack.Value == 0)
                    audio.VolumeTrack.Value = 1;
            });

            AddAssert("game not muted", () => gameTrackAudio.AggregateVolume.Value != 0);

            AddStep("get track", () => Add(owner = new TestTrackOwner(track = getTrack())));
            AddUntilStep("wait loaded", () => track.IsLoaded);
            AddStep("start track", () => track.Start());
            AddAssert("game is muted", () => gameTrackAudio.AggregateVolume.Value == 0);

            if (stopAnyPlaying)
                AddStep("stop any playing", () => trackManager.StopAnyPlaying(owner));
            else
                AddStep("stop track", () => track.Stop());

            AddAssert("game not muted", () => gameTrackAudio.AggregateVolume.Value != 0);
        }

        [Test]
        public void TestOwnerNotRegistered()
        {
            PreviewTrack track = null;

            AddStep("get track", () => Add(new TestTrackOwner(track = getTrack(), registerAsOwner: false)));
            AddUntilStep("wait for loaded", () => track.IsLoaded);

            AddStep("start track", () => track.Start());
            AddUntilStep("track is running", () => track.IsRunning);

            AddStep("cancel from anyone", () => trackManager.StopAnyPlaying(this));
            AddAssert("track stopped", () => !track.IsRunning);
        }

        private TestPreviewTrackManager.TestPreviewTrack getTrack() => (TestPreviewTrackManager.TestPreviewTrack)trackManager.Get(null);

        private TestPreviewTrackManager.TestPreviewTrack getOwnedTrack()
        {
            var track = getTrack();

            LoadComponentAsync(track, Add);

            return track;
        }

        private class TestTrackOwner : CompositeDrawable, IPreviewTrackOwner
        {
            private readonly PreviewTrack track;
            private readonly bool registerAsOwner;

            public TestTrackOwner(PreviewTrack track, bool registerAsOwner = true)
            {
                this.track = track;
                this.registerAsOwner = registerAsOwner;
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                LoadComponentAsync(track, AddInternal);
            }

            protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
            {
                var dependencies = new DependencyContainer(base.CreateChildDependencies(parent));
                if (registerAsOwner)
                    dependencies.CacheAs<IPreviewTrackOwner>(this);
                return dependencies;
            }
        }

        public class TestPreviewTrackManager : PreviewTrackManager
        {
            public bool AllowUpdate = true;

            public new PreviewTrack CurrentTrack => base.CurrentTrack;

            public TestPreviewTrackManager(IAdjustableAudioComponent mainTrackAdjustments)
                : base(mainTrackAdjustments)
            {
            }

            protected override TrackManagerPreviewTrack CreatePreviewTrack(IBeatmapSetInfo beatmapSetInfo, ITrackStore trackStore) => new TestPreviewTrack(beatmapSetInfo, trackStore);

            public override bool UpdateSubTree()
            {
                if (!AllowUpdate)
                    return true;

                return base.UpdateSubTree();
            }

            public class TestPreviewTrack : TrackManagerPreviewTrack
            {
                private readonly ITrackStore trackManager;

                public new Track Track => base.Track;

                public TestPreviewTrack(IBeatmapSetInfo beatmapSetInfo, ITrackStore trackManager)
                    : base(beatmapSetInfo, trackManager)
                {
                    this.trackManager = trackManager;
                }

                protected override Track GetTrack() => trackManager.GetVirtual(100000);
            }
        }
    }
}
