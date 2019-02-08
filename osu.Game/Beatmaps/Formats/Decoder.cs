// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace osu.Game.Beatmaps.Formats
{
    public abstract class Decoder<TOutput> : Decoder
        where TOutput : new()
    {
        protected virtual TOutput CreateTemplateObject() => new TOutput();

        public TOutput Decode(StreamReader primaryStream, params StreamReader[] otherStreams)
        {
            var output = CreateTemplateObject();
            foreach (StreamReader stream in otherStreams.Prepend(primaryStream))
                ParseStreamInto(stream, output);
            return output;
        }

        protected abstract void ParseStreamInto(StreamReader stream, TOutput output);
    }

    public abstract class Decoder
    {
        private static readonly Dictionary<Type, Dictionary<string, Func<string, Decoder>>> decoders = new Dictionary<Type, Dictionary<string, Func<string, Decoder>>>();

        static Decoder()
        {
            LegacyBeatmapDecoder.Register();
            JsonBeatmapDecoder.Register();
            LegacyStoryboardDecoder.Register();
        }

        /// <summary>
        /// Retrieves a <see cref="Decoder"/> to parse a <see cref="Beatmap"/>.
        /// </summary>
        /// <param name="stream">A stream pointing to the <see cref="Beatmap"/>.</param>
        public static Decoder<T> GetDecoder<T>(StreamReader stream)
            where T : new()
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (!decoders.TryGetValue(typeof(T), out var typedDecoders))
                throw new IOException(@"Unknown decoder type");

            string line;
            do
            {
                line = stream.ReadLine()?.Trim();
            } while (line != null && line.Length == 0);

            if (line == null)
                throw new IOException(@"Unknown file format (null)");

            var decoder = typedDecoders.Select(d => line.StartsWith(d.Key, StringComparison.InvariantCulture) ? d.Value : null).FirstOrDefault();
            if (decoder == null)
                throw new IOException($@"Unknown file format ({line})");

            return (Decoder<T>)decoder.Invoke(line);
        }

        /// <summary>
        /// Registers an instantiation function for a <see cref="Decoder"/>.
        /// </summary>
        /// <param name="magic">A string in the file which triggers this decoder to be used.</param>
        /// <param name="constructor">A function which constructs the <see cref="Decoder"/> given <paramref name="magic"/>.</param>
        protected static void AddDecoder<T>(string magic, Func<string, Decoder> constructor)
        {
            if (!decoders.TryGetValue(typeof(T), out var typedDecoders))
                decoders.Add(typeof(T), typedDecoders = new Dictionary<string, Func<string, Decoder>>());

            typedDecoders[magic] = constructor;
        }
    }
}
