// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.IO;
using osu.Game.Storyboards;

namespace osu.Game.Beatmaps.Formats
{
    public abstract class Decoder
    {
        private static readonly Dictionary<string, Type> decoders = new Dictionary<string, Type>();

        static Decoder()
        {
            LegacyDecoder.Register();
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
            return (Decoder)Activator.CreateInstance(decoders[line], line);
        }

        /// <summary>
        /// Adds the <see cref="Decoder"/> to the list of <see cref="Beatmap"/> and <see cref="Storyboard"/> decoder.
        /// </summary>
        /// <typeparam name="T">Type to decode a <see cref="Beatmap"/> with.</typeparam>
        /// <param name="version">A string representation of the version.</param>
        protected static void AddDecoder<T>(string version) where T : Decoder
        {
            decoders[version] = typeof(T);
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
