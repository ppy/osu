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
        private BeatmapInfo beatmapInfo;

        public readonly ArchiveReader Reader;

        private Beatmap beatmap;
        public Beatmap Beatmap
        {
            get
            {
                if (beatmap != null) return beatmap;

                try
                {
                    using (var stream = new StreamReader(Reader.ReadFile(beatmapInfo.Path)))
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
                    var trackData = Reader.ReadFile(beatmapInfo.Metadata.AudioFile);
                    if (trackData != null)
                        track = new AudioTrackBass(trackData);
                }
                catch { }

                return track;
            }
            set { track = value; }
        }

        public WorkingBeatmap(BeatmapInfo beatmapInfo = null, ArchiveReader reader = null)
        {
            this.beatmapInfo = beatmapInfo;
            Reader = reader;
        }

        private bool isDisposed;

        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                track?.Dispose();
                Reader?.Dispose();
                isDisposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public void TransferTo(WorkingBeatmap working)
        {
            if (track != null && working.beatmapInfo.Metadata.AudioFile == beatmapInfo.Metadata.AudioFile && working.beatmapInfo.BeatmapSet.Path == beatmapInfo.BeatmapSet.Path)
                working.track = track;
        }
    }
}
