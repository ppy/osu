// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using osu.Game.IO;
using osu.Game.Rulesets;

namespace osu.Game.Beatmaps.Formats
{
    public abstract class Decoder<TOutput> : Decoder
        where TOutput : new()
    {
        protected virtual TOutput CreateTemplateObject() => new TOutput();

        public TOutput Decode(LineBufferedReader primaryStream, params LineBufferedReader[] otherStreams)
        {
            var output = CreateTemplateObject();
            foreach (LineBufferedReader stream in otherStreams.Prepend(primaryStream))
                ParseStreamInto(stream, output);
            return output;
        }

        protected abstract void ParseStreamInto(LineBufferedReader stream, TOutput output);
    }

    public abstract class Decoder
    {
        private static readonly Dictionary<Type, Dictionary<string, Func<string, Decoder>>> decoders = new Dictionary<Type, Dictionary<string, Func<string, Decoder>>>();
        private static readonly Dictionary<Type, Func<Decoder>> fallback_decoders = new Dictionary<Type, Func<Decoder>>();

        static Decoder()
        {
            LegacyBeatmapDecoder.Register();
            JsonBeatmapDecoder.Register();
            LegacyStoryboardDecoder.Register();
        }

        /// <summary>
        /// Register dependencies for use with static decoder classes.
        /// </summary>
        /// <param name="rulesets">A store containing all available rulesets (used by <see cref="LegacyBeatmapDecoder"/>).</param>
        public static void RegisterDependencies(RulesetStore rulesets)
        {
            LegacyBeatmapDecoder.RulesetStore = rulesets ?? throw new ArgumentNullException(nameof(rulesets));
        }

        /// <summary>
        /// Retrieves a <see cref="Decoder"/> to parse a <see cref="Beatmap"/>.
        /// </summary>
        /// <param name="stream">A stream pointing to the <see cref="Beatmap"/>.</param>
        public static Decoder<T> GetDecoder<T>(LineBufferedReader stream)
            where T : new()
        {
            ArgumentNullException.ThrowIfNull(stream);

            if (!decoders.TryGetValue(typeof(T), out var typedDecoders))
                throw new IOException(@"Unknown decoder type");

            // start off with the first line of the file
            string? line = stream.PeekLine()?.Trim();

            while (line != null && line.Length == 0)
            {
                // consume the previously peeked empty line and advance to the next one
                stream.ReadLine();
                line = stream.PeekLine()?.Trim();
            }

            if (line == null)
                throw new IOException("Unknown file format (no content)");

            var decoder = typedDecoders.Where(d => line.StartsWith(d.Key, StringComparison.InvariantCulture)).Select(d => d.Value).FirstOrDefault();

            // it's important the magic does NOT get consumed here, since sometimes it's part of the structure
            // (see JsonBeatmapDecoder - the magic string is the opening brace)
            // decoder implementations should therefore not die on receiving their own magic
            if (decoder != null)
                return (Decoder<T>)decoder.Invoke(line);

            if (!fallback_decoders.TryGetValue(typeof(T), out var fallbackDecoder))
                throw new IOException($"Unknown file format ({line})");

            return (Decoder<T>)fallbackDecoder.Invoke();
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

        /// <summary>
        /// Registers a fallback decoder instantiation function.
        /// The fallback will be returned if the first non-empty line of the decoded stream does not match any known magic.
        /// Calling this method will overwrite any existing global fallback registration for type <typeparamref name="T"/> - use with caution.
        /// </summary>
        /// <typeparam name="T">Type of object being decoded.</typeparam>
        /// <param name="constructor">A function that constructs the fallback<see cref="Decoder"/>.</param>
        protected static void SetFallbackDecoder<T>(Func<Decoder> constructor)
        {
            fallback_decoders[typeof(T)] = constructor;
        }
    }
}
