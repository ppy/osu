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
        private static readonly Dictionary<string, Type> beatmapDecoders = new Dictionary<string, Type>();
        private static readonly Dictionary<string, Type> storyboardDecoders = new Dictionary<string, Type>();

        static Decoder()
        {
            LegacyDecoder.Register();
        }

        /// <summary>
        /// Retrieves a <see cref="Decoder"/> to parse <see cref="Beatmap"/>s.
        /// </summary>
        /// <param name="stream">A stream pointing to the <see cref="Beatmap"/> to retrieve the version from.</param>
        public static Decoder GetBeatmapDecoder(StreamReader stream)
        {
            string line = readFirstLine(stream);

            if (line == null || !beatmapDecoders.ContainsKey(line))
                throw new IOException(@"Unknown file format");
            return (Decoder)Activator.CreateInstance(beatmapDecoders[line], line);
        }

        /// <summary>
        /// Retrieves a <see cref="Decoder"/> to parse <see cref="Storyboard"/>s.
        /// </summary>
        /// <param name="stream">A stream pointing to the <see cref="Beatmap"/> to retrieve the version from.</param>
        public static Decoder GetStoryboardDecoder(StreamReader stream)
        {
            string line = readFirstLine(stream);

            if (line == null || !storyboardDecoders.ContainsKey(line))
                throw new IOException(@"Unknown file format");
            return (Decoder)Activator.CreateInstance(storyboardDecoders[line], line);
        }

        private static string readFirstLine(StreamReader stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            string line;
            do
            { line = stream.ReadLine()?.Trim(); }
            while (line != null && line.Length == 0);

            return line;
        }

        /// <summary>
        /// Adds the <see cref="Decoder"/> to the list of <see cref="Beatmap"/> and <see cref="Storyboard"/> decoder.
        /// </summary>
        /// <typeparam name="A">Type to decode a <see cref="Beatmap"/> with.</typeparam>
        /// /// <typeparam name="B">Type to decode a <see cref="Storyboard"/> with.</typeparam>
        /// <param name="version">A string representation of the version.</param>
        protected static void AddDecoder<A, B>(string version) where A : Decoder where B : Decoder
        {
            beatmapDecoders[version] = typeof(A);
            storyboardDecoders[version] = typeof(B);
        }

        /// <summary>
        /// Adds the <see cref="Decoder"/> to the list of <see cref="Beatmap"/> decoder.
        /// </summary>
        /// <typeparam name="T">Type to decode a <see cref="Beatmap"/> with.</typeparam>
        /// <param name="version">A string representation of the version.</param>
        protected static void AddBeatmapDecoder<T>(string version) where T : Decoder
        {
            beatmapDecoders[version] = typeof(T);
        }

        /// <summary>
        /// Adds the <see cref="Decoder"/> to the list of <see cref="Storyboard"/> decoder.
        /// </summary>
        /// <typeparam name="T">Type to decode a <see cref="Storyboard"/> with.</typeparam>
        /// <param name="version">A string representation of the version.</param>
        protected static void AddStoryboardDecoder<T>(string version) where T : Decoder
        {
            storyboardDecoders[version] = typeof(T);
        }

        public virtual Beatmap DecodeBeatmap(StreamReader stream) => ParseBeatmap(stream);

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
