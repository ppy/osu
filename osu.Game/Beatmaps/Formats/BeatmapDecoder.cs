using System;
using System.Collections.Generic;
using System.IO;

namespace osu.Game.Beatmaps.Formats
{
    public abstract class BeatmapDecoder
    {
        private static Dictionary<string, Type> Decoders { get; set; } = new Dictionary<string, Type>();
    
        public static BeatmapDecoder GetDecoder(TextReader stream)
        {
            var line = stream.ReadLine().Trim();
            if (!Decoders.ContainsKey(line))
                throw new IOException("Unknown file format");
            return (BeatmapDecoder)Activator.CreateInstance(Decoders[line]);
        }
        protected static void AddDecoder<T>(string magic) where T : BeatmapDecoder
        {
            Decoders[magic] = typeof(T);
        }
    
        public abstract Beatmap Decode(TextReader stream);
    }
}