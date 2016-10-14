using System;
using System.Collections.Generic;
using System.IO;

namespace osu.Game.Beatmaps.Formats
{
    public abstract class BeatmapDecoder
    {
        private static Dictionary<string, Type> decoders { get; } = new Dictionary<string, Type>();
    
        public static BeatmapDecoder GetDecoder(TextReader stream)
        {
            var line = stream.ReadLine().Trim();
            if (!decoders.ContainsKey(line))
                throw new IOException(@"Unknown file format");
            return (BeatmapDecoder)Activator.CreateInstance(decoders[line]);
        }
        protected static void AddDecoder<T>(string magic) where T : BeatmapDecoder
        {
            decoders[magic] = typeof(T);
        }
    
        public abstract void Decode(TextReader stream, Beatmap beatmap);
    }
}