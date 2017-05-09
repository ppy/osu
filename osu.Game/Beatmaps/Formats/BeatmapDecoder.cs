// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.IO;
using osu.Game.Rulesets.Objects;
using osu.Game.Database;

namespace osu.Game.Beatmaps.Formats
{
    public abstract class BeatmapDecoder
    {
        private static readonly Dictionary<string, Type> decoders = new Dictionary<string, Type>();

        public static BeatmapDecoder GetDecoder(StreamReader stream)
        {
            string line = stream.ReadLine()?.Trim();

            if (line == null || !decoders.ContainsKey(line))
                throw new IOException(@"Unknown file format");
            return (BeatmapDecoder)Activator.CreateInstance(decoders[line], line);
        }

        protected static void AddDecoder<T>(string magic) where T : BeatmapDecoder
        {
            decoders[magic] = typeof(T);
        }

        public virtual Beatmap Decode(StreamReader stream)
        {
            return ParseFile(stream);
        }

        public virtual void Decode(StreamReader stream, Beatmap beatmap)
        {
            ParseFile(stream, beatmap);
        }

        protected virtual Beatmap ParseFile(StreamReader stream)
        {
            var beatmap = new Beatmap
            {
                HitObjects = new List<HitObject>(),
                BeatmapInfo = new BeatmapInfo
                {
                    Metadata = new BeatmapMetadata(),
                    Difficulty = new BeatmapDifficulty(),
                },
            };

            ParseFile(stream, beatmap);
            return beatmap;
        }

        protected abstract void ParseFile(StreamReader stream, Beatmap beatmap);
    }
}
