// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Audio;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;
using osu.Framework.Lists;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Framework.Statistics;
using osu.Framework.Testing;
using osu.Game.Beatmaps.Formats;
using osu.Game.Database;
using osu.Game.IO;
using osu.Game.Skinning;
using osu.Game.Storyboards;

namespace osu.Game.Beatmaps
{
    public class WorkingBeatmapCache : IBeatmapResourceProvider, IWorkingBeatmapCache
    {
        private readonly WeakList<BeatmapManagerWorkingBeatmap> workingCache = new WeakList<BeatmapManagerWorkingBeatmap>();

        /// <summary>
        /// Beatmap files may specify this filename to denote that they don't have an audio track.
        /// </summary>
        private const string virtual_track_filename = @"virtual";

        /// <summary>
        /// A default representation of a WorkingBeatmap to use when no beatmap is available.
        /// </summary>
        public readonly WorkingBeatmap DefaultBeatmap;

        private readonly AudioManager audioManager;
        private readonly IResourceStore<byte[]> resources;
        private readonly LargeTextureStore largeTextureStore;
        private readonly ITrackStore trackStore;
        private readonly IResourceStore<byte[]> files;

        [CanBeNull]
        private readonly GameHost host;

        public WorkingBeatmapCache(ITrackStore trackStore, AudioManager audioManager, IResourceStore<byte[]> resources, IResourceStore<byte[]> files, WorkingBeatmap defaultBeatmap = null,
                                   GameHost host = null)
        {
            DefaultBeatmap = defaultBeatmap;

            this.audioManager = audioManager;
            this.resources = resources;
            this.host = host;
            this.files = files;
            largeTextureStore = new LargeTextureStore(host?.CreateTextureLoaderStore(files));
            this.trackStore = trackStore;
        }

        public void Invalidate(BeatmapSetInfo info)
        {
            foreach (var b in info.Beatmaps)
                Invalidate(b);
        }

        public void Invalidate(BeatmapInfo info)
        {
            lock (workingCache)
            {
                var working = workingCache.FirstOrDefault(w => info.Equals(w.BeatmapInfo));

                if (working != null)
                {
                    Logger.Log($"Invalidating working beatmap cache for {info}");
                    workingCache.Remove(working);
                }
            }
        }

        public virtual WorkingBeatmap GetWorkingBeatmap(BeatmapInfo beatmapInfo)
        {
            if (beatmapInfo?.BeatmapSet == null)
                return DefaultBeatmap;

            lock (workingCache)
            {
                var working = workingCache.FirstOrDefault(w => beatmapInfo.Equals(w.BeatmapInfo));

                if (working != null)
                    return working;

                beatmapInfo = beatmapInfo.Detach();

                workingCache.Add(working = new BeatmapManagerWorkingBeatmap(beatmapInfo, this));

                // best effort; may be higher than expected.
                GlobalStatistics.Get<int>("Beatmaps", $"Cached {nameof(WorkingBeatmap)}s").Value = workingCache.Count();

                return working;
            }
        }

        #region IResourceStorageProvider

        TextureStore IBeatmapResourceProvider.LargeTextureStore => largeTextureStore;
        ITrackStore IBeatmapResourceProvider.Tracks => trackStore;
        AudioManager IStorageResourceProvider.AudioManager => audioManager;
        RealmAccess IStorageResourceProvider.RealmAccess => null;
        IResourceStore<byte[]> IStorageResourceProvider.Files => files;
        IResourceStore<byte[]> IStorageResourceProvider.Resources => resources;
        IResourceStore<TextureUpload> IStorageResourceProvider.CreateTextureLoaderStore(IResourceStore<byte[]> underlyingStore) => host?.CreateTextureLoaderStore(underlyingStore);

        #endregion

        [ExcludeFromDynamicCompile]
        private class BeatmapManagerWorkingBeatmap : WorkingBeatmap
        {
            [NotNull]
            private readonly IBeatmapResourceProvider resources;

            public BeatmapManagerWorkingBeatmap(BeatmapInfo beatmapInfo, [NotNull] IBeatmapResourceProvider resources)
                : base(beatmapInfo, resources.AudioManager)
            {
                this.resources = resources;
            }

            protected override IBeatmap GetBeatmap()
            {
                if (BeatmapInfo.Path == null)
                    return new Beatmap { BeatmapInfo = BeatmapInfo };

                try
                {
                    using (var stream = new LineBufferedReader(GetStream(BeatmapSetInfo.GetPathForFile(BeatmapInfo.Path))))
                        return Decoder.GetDecoder<Beatmap>(stream).Decode(stream);
                }
                catch (Exception e)
                {
                    Logger.Error(e, "Beatmap failed to load");
                    return null;
                }
            }

            protected override Texture GetBackground()
            {
                if (string.IsNullOrEmpty(Metadata?.BackgroundFile))
                    return null;

                try
                {
                    return resources.LargeTextureStore.Get(BeatmapSetInfo.GetPathForFile(Metadata.BackgroundFile));
                }
                catch (Exception e)
                {
                    Logger.Error(e, "Background failed to load");
                    return null;
                }
            }

            protected override Track GetBeatmapTrack()
            {
                if (string.IsNullOrEmpty(Metadata?.AudioFile))
                    return null;

                if (Metadata.AudioFile == virtual_track_filename)
                    return null;

                try
                {
                    return resources.Tracks.Get(BeatmapSetInfo.GetPathForFile(Metadata.AudioFile));
                }
                catch (Exception e)
                {
                    Logger.Error(e, "Track failed to load");
                    return null;
                }
            }

            protected override Waveform GetWaveform()
            {
                if (string.IsNullOrEmpty(Metadata?.AudioFile))
                    return null;

                if (Metadata.AudioFile == virtual_track_filename)
                    return null;

                try
                {
                    var trackData = GetStream(BeatmapSetInfo.GetPathForFile(Metadata.AudioFile));
                    return trackData == null ? null : new Waveform(trackData);
                }
                catch (Exception e)
                {
                    Logger.Error(e, "Waveform failed to load");
                    return null;
                }
            }

            protected override Storyboard GetStoryboard()
            {
                Storyboard storyboard;

                if (BeatmapInfo.Path == null)
                    return new Storyboard();

                try
                {
                    using (var stream = new LineBufferedReader(GetStream(BeatmapSetInfo.GetPathForFile(BeatmapInfo.Path))))
                    {
                        var decoder = Decoder.GetDecoder<Storyboard>(stream);

                        string storyboardFilename = BeatmapSetInfo?.Files.FirstOrDefault(f => f.Filename.EndsWith(".osb", StringComparison.OrdinalIgnoreCase))?.Filename;

                        // todo: support loading from both set-wide storyboard *and* beatmap specific.
                        if (string.IsNullOrEmpty(storyboardFilename))
                            storyboard = decoder.Decode(stream);
                        else
                        {
                            using (var secondaryStream = new LineBufferedReader(GetStream(BeatmapSetInfo.GetPathForFile(storyboardFilename))))
                                storyboard = decoder.Decode(stream, secondaryStream);
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.Error(e, "Storyboard failed to load");
                    storyboard = new Storyboard();
                }

                storyboard.BeatmapInfo = BeatmapInfo;

                return storyboard;
            }

            protected internal override ISkin GetSkin()
            {
                try
                {
                    return new LegacyBeatmapSkin(BeatmapInfo, resources);
                }
                catch (Exception e)
                {
                    Logger.Error(e, "Skin failed to load");
                    return null;
                }
            }

            public override Stream GetStream(string storagePath) => resources.Files.GetStream(storagePath);
        }
    }
}
