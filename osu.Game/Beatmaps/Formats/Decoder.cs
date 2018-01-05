// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.IO;
using osu.Game.Storyboards;

namespace osu.Game.Beatmaps.Formats
{
    public abstract class Decoder
    {
        private static readonly Dictionary<string, Func<string, Decoder>> decoders = new Dictionary<string, Func<string, Decoder>>();

        static Decoder()
        {
            LegacyDecoder.Register();
            JsonBeatmapDecoder.Register();
        }

        /// <summary>
        /// Retrieves a <see cref="Decoder"/> to parse a <see cref="Beatmap"/>.
        /// </summary>
        /// <param name="stream">A stream pointing to the <see cref="Beatmap"/>.</param>
        public static Decoder GetDecoder(StreamReader stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            string line;
            do
            { line = stream.ReadLine()?.Trim(); }
            while (line != null && line.Length == 0);

            if (line == null || !decoders.ContainsKey(line))
                throw new IOException(@"Unknown file format");

            return decoders[line](line);
        }

        /// <summary>
        /// Registers an instantiation function for a <see cref="Decoder"/>.
        /// </summary>
        /// <param name="magic">A string in the file which triggers this decoder to be used.</param>
        /// <param name="constructor">A function which constructs the <see cref="Decoder"/> given <paramref name="magic"/>.</param>
        protected static void AddDecoder(string magic, Func<string, Decoder> constructor)
        {
            decoders[magic] = constructor;
        }

        /// <summary>
        /// Retrieves a <see cref="Decoder"/> to parse a <see cref="Storyboard"/>
        /// </summary>
        public abstract Decoder GetStoryboardDecoder();

        public virtual Beatmap DecodeBeatmap(StreamReader stream)
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
