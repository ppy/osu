// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.IO.Stores;
using osu.Game.Beatmaps;

namespace osu.Game.Audio
{
    public class PreviewTrackManager : Component
    {
        private readonly BindableDouble muteBindable = new BindableDouble();

        private AudioManager audio;
        private PreviewTrackStore trackStore;

        private TrackManagerPreviewTrack current;

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            // this is a temporary solution to get around muting ourselves.
            // todo: update this once we have a BackgroundTrackManager or similar.
            trackStore = new PreviewTrackStore(new OnlineStore());

            audio.AddItem(trackStore);
            trackStore.AddAdjustment(AdjustableProperty.Volume, audio.VolumeTrack);

            this.audio = audio;
        }

        /// <summary>
        /// Retrieves a <see cref="PreviewTrack"/> for a <see cref="BeatmapSetInfo"/>.
        /// </summary>
        /// <param name="beatmapSetInfo">The <see cref="BeatmapSetInfo"/> to retrieve the preview track for.</param>
        /// <returns>The playable <see cref="PreviewTrack"/>.</returns>
        public PreviewTrack Get(BeatmapSetInfo beatmapSetInfo)
        {
            var track = CreatePreviewTrack(beatmapSetInfo, trackStore);

            track.Started += () =>
            {
                current?.Stop();
                current = track;
                audio.Tracks.AddAdjustment(AdjustableProperty.Volume, muteBindable);
            };

            track.Stopped += () =>
            {
                current = null;
                audio.Tracks.RemoveAdjustment(AdjustableProperty.Volume, muteBindable);
            };

            return track;
        }

        /// <summary>
        /// Stops any currently playing <see cref="PreviewTrack"/>.
        /// </summary>
        /// <remarks>
        /// Only the immediate owner (an object that implements <see cref="IPreviewTrackOwner"/>) of the playing <see cref="PreviewTrack"/>
        /// can globally stop the currently playing <see cref="PreviewTrack"/>. The object holding a reference to the <see cref="PreviewTrack"/>
        /// can always stop the <see cref="PreviewTrack"/> themselves through <see cref="PreviewTrack.Stop()"/>.
        /// </remarks>
        /// <param name="source">The <see cref="IPreviewTrackOwner"/> which may be the owner of the <see cref="PreviewTrack"/>.</param>
        public void StopAnyPlaying(IPreviewTrackOwner source)
        {
            if (current == null || current.Owner != source)
                return;

            current.Stop();
            current = null;
        }

        /// <summary>
        /// Creates the <see cref="TrackManagerPreviewTrack"/>.
        /// </summary>
        protected virtual TrackManagerPreviewTrack CreatePreviewTrack(BeatmapSetInfo beatmapSetInfo, ITrackStore trackStore) => new TrackManagerPreviewTrack(beatmapSetInfo, trackStore);

        protected class TrackManagerPreviewTrack : PreviewTrack
        {
            public IPreviewTrackOwner Owner { get; private set; }

            private readonly BeatmapSetInfo beatmapSetInfo;
            private readonly ITrackStore trackManager;

            public TrackManagerPreviewTrack(BeatmapSetInfo beatmapSetInfo, ITrackStore trackManager)
            {
                this.beatmapSetInfo = beatmapSetInfo;
                this.trackManager = trackManager;
            }

            [BackgroundDependencyLoader]
            private void load(IPreviewTrackOwner owner)
            {
                Owner = owner;
            }

            protected override Track GetTrack() => trackManager.Get($"https://b.ppy.sh/preview/{beatmapSetInfo?.OnlineBeatmapSetID}.mp3");
        }

        private class PreviewTrackStore : AudioCollectionManager<AdjustableAudioComponent>, ITrackStore
        {
            private readonly IResourceStore<byte[]> store;

            internal PreviewTrackStore(IResourceStore<byte[]> store)
            {
                this.store = store;
            }

            public Track GetVirtual(double length = double.PositiveInfinity)
            {
                if (IsDisposed) throw new ObjectDisposedException($"Cannot retrieve items for an already disposed {nameof(PreviewTrackStore)}");

                var track = new TrackVirtual(length);
                AddItem(track);
                return track;
            }

            public Track Get(string name)
            {
                if (IsDisposed) throw new ObjectDisposedException($"Cannot retrieve items for an already disposed {nameof(PreviewTrackStore)}");

                if (string.IsNullOrEmpty(name)) return null;

                var dataStream = store.GetStream(name);

                if (dataStream == null)
                    return null;

                Track track = new TrackBass(dataStream);
                AddItem(track);
                return track;
            }

            public Task<Track> GetAsync(string name) => Task.Run(() => Get(name));

            public Stream GetStream(string name) => store.GetStream(name);

            public IEnumerable<string> GetAvailableResources() => store.GetAvailableResources();
        }
    }
}
