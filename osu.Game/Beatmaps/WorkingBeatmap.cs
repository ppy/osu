//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Audio.Track;
using osu.Game.Beatmaps.IO;

namespace osu.Game.Beatmaps
{
    public class WorkingBeatmap : IDisposable
    {
        public readonly ArchiveReader Reader;
        public readonly Beatmap Beatmap;

        private AudioTrack track;
        public AudioTrack Track
        {
            get
            {
                if (track != null) return track;

                try
                {
                    var trackData = Reader.ReadFile(Beatmap.Metadata.AudioFile);
                    if (trackData != null)
                        track = new AudioTrackBass(trackData);
                }
                catch { }

                return track;
            }
            set { track = value; }
        }

        public WorkingBeatmap(Beatmap beatmap, ArchiveReader reader = null)
        {
            Beatmap = beatmap;
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
    }
}
