//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.IO;
using osu.Framework.Audio.Track;
using osu.Game.Beatmaps.Formats;
using osu.Game.Beatmaps.IO;
using osu.Game.Database;

namespace osu.Game.Beatmaps
{
    public class WorkingBeatmap : IDisposable
    {
        public readonly BeatmapInfo BeatmapInfo;

        public readonly BeatmapSetInfo BeatmapSetInfo;
        private readonly BeatmapDatabase database;

        private ArchiveReader reader => database.GetReader(BeatmapSetInfo);

        private Beatmap beatmap;
        public Beatmap Beatmap
        {
            get
            {
                if (beatmap != null) return beatmap;

                try
                {
                    using (var stream = new StreamReader(reader.ReadFile(BeatmapInfo.Path)))
                        beatmap = BeatmapDecoder.GetDecoder(stream)?.Decode(stream);
                }
                catch { }

                return beatmap;
            }
            set { beatmap = value; }
        }

        private AudioTrack track;
        public AudioTrack Track
        {
            get
            {
                if (track != null) return track;

                try
                {
                    var trackData = reader.ReadFile(BeatmapInfo.Metadata.AudioFile);
                    if (trackData != null)
                        track = new AudioTrackBass(trackData);
                }
                catch { }

                return track;
            }
            set { track = value; }
        }

        public WorkingBeatmap(Beatmap beatmap)
        {
            this.beatmap = beatmap;
        }

        public WorkingBeatmap(BeatmapInfo beatmapInfo, BeatmapSetInfo beatmapSetInfo, BeatmapDatabase database)
        {
            this.BeatmapInfo = beatmapInfo;
            this.BeatmapSetInfo = beatmapSetInfo;
            this.database = database;
        }

        private bool isDisposed;

        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                track?.Dispose();
                reader?.Dispose();
                isDisposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public void TransferTo(WorkingBeatmap working)
        {
            if (track != null && working.BeatmapInfo.Metadata.AudioFile == BeatmapInfo.Metadata.AudioFile && working.BeatmapInfo.BeatmapSet.Path == BeatmapInfo.BeatmapSet.Path)
                working.track = track;
        }
    }
}
