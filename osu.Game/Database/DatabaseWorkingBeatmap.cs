// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.IO;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics.Textures;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Formats;
using osu.Game.Beatmaps.IO;

namespace osu.Game.Database
{
    internal class DatabaseWorkingBeatmap : WorkingBeatmap
    {
        private readonly BeatmapDatabase database;

        public DatabaseWorkingBeatmap(BeatmapDatabase database, BeatmapInfo beatmapInfo, BeatmapSetInfo beatmapSetInfo, bool withStoryboard = false)
            : base(beatmapInfo, beatmapSetInfo, withStoryboard)
        {
            this.database = database;
        }

        private ArchiveReader getReader() => database?.GetReader(BeatmapSetInfo);

        protected override Beatmap GetBeatmap()
        {
            try
            {
                Beatmap beatmap;

                using (var reader = getReader())
                {
                    BeatmapDecoder decoder;
                    using (var stream = new StreamReader(reader.GetStream(BeatmapInfo.Path)))
                    {
                        decoder = BeatmapDecoder.GetDecoder(stream);
                        beatmap = decoder.Decode(stream);
                    }

                    if (beatmap == null || !WithStoryboard || BeatmapSetInfo.StoryboardFile == null)
                        return beatmap;

                    using (var stream = new StreamReader(reader.GetStream(BeatmapSetInfo.StoryboardFile)))
                        decoder.Decode(stream, beatmap);
                }

                return beatmap;
            }
            catch { return null; }
        }

        protected override Texture GetBackground()
        {
            if (BeatmapInfo?.Metadata?.BackgroundFile == null)
                return null;

            try
            {
                using (var reader = getReader())
                    return new TextureStore(new RawTextureLoaderStore(reader), false).Get(BeatmapInfo.Metadata.BackgroundFile);
            }
            catch { return null; }
        }

        protected override Track GetTrack()
        {
            try
            {
                var trackData = getReader()?.GetStream(BeatmapInfo.Metadata.AudioFile);
                return trackData == null ? null : new TrackBass(trackData);
            }
            catch { return null; }
        }
    }
}