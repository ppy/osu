// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Audio;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;
using osu.Framework.Logging;
using osu.Framework.Testing;
using osu.Game.Beatmaps.Formats;
using osu.Game.IO;
using osu.Game.Skinning;
using osu.Game.Storyboards;

namespace osu.Game.Beatmaps
{
    public partial class BeatmapManager
    {
        [ExcludeFromDynamicCompile]
        private class BeatmapManagerWorkingBeatmap : WorkingBeatmap
        {
            private readonly IResourceStore<byte[]> store;
            private readonly TextureStore textureStore;
            private readonly ITrackStore trackStore;

            public BeatmapManagerWorkingBeatmap(IResourceStore<byte[]> store, TextureStore textureStore, ITrackStore trackStore, BeatmapInfo beatmapInfo, AudioManager audioManager)
                : base(beatmapInfo, audioManager)
            {
                this.store = store;
                this.textureStore = textureStore;
                this.trackStore = trackStore;
            }

            protected override IBeatmap GetBeatmap()
            {
                if (BeatmapInfo.Path == null)
                    return new Beatmap { BeatmapInfo = BeatmapInfo };

                try
                {
                    using (var stream = new LineBufferedReader(store.GetStream(getPathForFile(BeatmapInfo.Path))))
                        return Decoder.GetDecoder<Beatmap>(stream).Decode(stream);
                }
                catch (Exception e)
                {
                    Logger.Error(e, "Beatmap failed to load");
                    return null;
                }
            }

            private string getPathForFile(string filename) => BeatmapSetInfo.Files.SingleOrDefault(f => string.Equals(f.Filename, filename, StringComparison.OrdinalIgnoreCase))?.FileInfo.StoragePath;

            protected override bool BackgroundStillValid(Texture b) => false; // bypass lazy logic. we want to return a new background each time for refcounting purposes.

            protected override Texture GetBackground()
            {
                if (Metadata?.BackgroundFile == null)
                    return null;

                try
                {
                    return textureStore.Get(getPathForFile(Metadata.BackgroundFile));
                }
                catch (Exception e)
                {
                    Logger.Error(e, "Background failed to load");
                    return null;
                }
            }

            protected override Track GetBeatmapTrack()
            {
                if (Metadata?.AudioFile == null)
                    return null;

                try
                {
                    return trackStore.Get(getPathForFile(Metadata.AudioFile));
                }
                catch (Exception e)
                {
                    Logger.Error(e, "Track failed to load");
                    return null;
                }
            }

            protected override Waveform GetWaveform()
            {
                if (Metadata?.AudioFile == null)
                    return null;

                try
                {
                    var trackData = store.GetStream(getPathForFile(Metadata.AudioFile));
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

                try
                {
                    using (var stream = new LineBufferedReader(store.GetStream(getPathForFile(BeatmapInfo.Path))))
                    {
                        var decoder = Decoder.GetDecoder<Storyboard>(stream);

                        // todo: support loading from both set-wide storyboard *and* beatmap specific.
                        if (BeatmapSetInfo?.StoryboardFile == null)
                            storyboard = decoder.Decode(stream);
                        else
                        {
                            using (var secondaryStream = new LineBufferedReader(store.GetStream(getPathForFile(BeatmapSetInfo.StoryboardFile))))
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

            protected override ISkin GetSkin()
            {
                try
                {
                    return new LegacyBeatmapSkin(BeatmapInfo, store, AudioManager);
                }
                catch (Exception e)
                {
                    Logger.Error(e, "Skin failed to load");
                    return null;
                }
            }
        }
    }
}
