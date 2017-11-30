// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.IO;
using osu.Game.Storyboards;

namespace osu.Game.Beatmaps.Formats
{
    public abstract class BeatmapDecoder
    {
        private static readonly Dictionary<string, Type> decoders = new Dictionary<string, Type>();

        static BeatmapDecoder()
        {
            OsuLegacyDecoder.Register();
        }

        public static BeatmapDecoder GetDecoder(StreamReader stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            string line;
            do { line = stream.ReadLine()?.Trim(); }
            while (line != null && line.Length == 0);

            if (line == null || !decoders.ContainsKey(line))
                throw new IOException(@"Unknown file format");
            return (BeatmapDecoder)Activator.CreateInstance(decoders[line], line);
        }

        protected static void AddDecoder<T>(string magic) where T : BeatmapDecoder
        {
            decoders[magic] = typeof(T);
        }

        public virtual Beatmap DecodeBeatmap(StreamReader stream)
        {
            return ParseBeatmap(stream);
        }

        protected virtual Beatmap ParseBeatmap(StreamReader stream)
        {
            var beatmap = new Beatmap
            {
                BeatmapInfo = new BeatmapInfo
                {
                    Metadata = new BeatmapMetadata(),
                    BaseDifficulty = new BeatmapDifficulty(),
                },
            };

            ParseBeatmap(stream, beatmap);
            return beatmap;
        }

        protected abstract void ParseBeatmap(StreamReader stream, Beatmap beatmap);

        public virtual Storyboard DecodeStoryboard(StreamReader stream)
        {
            var storyboard = new Storyboard();
            ParseStoryboard(stream, storyboard);
            return storyboard;
        }

        protected abstract void ParseStoryboard(StreamReader stream, Storyboard storyboard);
    }
}
