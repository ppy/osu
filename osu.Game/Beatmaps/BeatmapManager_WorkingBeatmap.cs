// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using System.Linq;
using osu.Framework.Audio;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;
using osu.Framework.Logging;
using osu.Game.Beatmaps.Formats;
using osu.Game.Skinning;
using osu.Game.Storyboards;

namespace osu.Game.Beatmaps
{
    public partial class BeatmapManager
    {
        protected class BeatmapManagerWorkingBeatmap : WorkingBeatmap
        {
            private readonly IResourceStore<byte[]> store;
            private readonly AudioManager audioManager;

            public BeatmapManagerWorkingBeatmap(IResourceStore<byte[]> store, TextureStore textureStore, BeatmapInfo beatmapInfo, AudioManager audioManager)
                : base(beatmapInfo)
            {
                this.store = store;
                this.textureStore = textureStore;
                this.audioManager = audioManager;
            }

            protected override IBeatmap GetBeatmap()
            {
                try
                {
                    using (var stream = new StreamReader(store.GetStream(getPathForFile(BeatmapInfo.Path))))
                        return Decoder.GetDecoder<Beatmap>(stream).Decode(stream);
                }
                catch
                {
                    return null;
                }
            }

            private string getPathForFile(string filename) => BeatmapSetInfo.Files.First(f => string.Equals(f.Filename, filename, StringComparison.InvariantCultureIgnoreCase)).FileInfo.StoragePath;

            private TextureStore textureStore;

            protected override bool BackgroundStillValid(Texture b) => false; // bypass lazy logic. we want to return a new background each time for refcounting purposes.

            protected override Texture GetBackground()
            {
                if (Metadata?.BackgroundFile == null)
                    return null;

                try
                {
                    return textureStore.Get(getPathForFile(Metadata.BackgroundFile));
                }
                catch
                {
                    return null;
                }
            }

            protected override Track GetTrack()
            {
                try
                {
                    var trackData = store.GetStream(getPathForFile(Metadata.AudioFile));
                    return trackData == null ? null : new TrackBass(trackData);
                }
                catch
                {
                    return null;
                }
            }

            public override void TransferTo(WorkingBeatmap other)
            {
                base.TransferTo(other);

                if (other is BeatmapManagerWorkingBeatmap owb && textureStore != null && BeatmapInfo.BackgroundEquals(other.BeatmapInfo))
                    owb.textureStore = textureStore;
            }

            protected override Waveform GetWaveform()
            {
                try
                {
                    var trackData = store.GetStream(getPathForFile(Metadata.AudioFile));
                    return trackData == null ? null : new Waveform(trackData);
                }
                catch
                {
                    return null;
                }
            }

            protected override Storyboard GetStoryboard()
            {
                Storyboard storyboard;
                try
                {
                    using (var stream = new StreamReader(store.GetStream(getPathForFile(BeatmapInfo.Path))))
                    {
                        var decoder = Decoder.GetDecoder<Storyboard>(stream);

                        // todo: support loading from both set-wide storyboard *and* beatmap specific.
                        if (BeatmapSetInfo?.StoryboardFile == null)
                            storyboard = decoder.Decode(stream);
                        else
                        {
                            using (var secondaryStream = new StreamReader(store.GetStream(getPathForFile(BeatmapSetInfo.StoryboardFile))))
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

            protected override Skin GetSkin()
            {
                Skin skin;
                try
                {
                    skin = new LegacyBeatmapSkin(BeatmapInfo, store, audioManager);
                }
                catch (Exception e)
                {
                    Logger.Error(e, "Skin failed to load");
                    skin = new DefaultSkin();
                }

                return skin;
            }
        }
    }
}
