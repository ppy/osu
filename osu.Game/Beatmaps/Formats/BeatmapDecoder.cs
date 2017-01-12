//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.IO;
using osu.Game.Modes.Objects;
using OpenTK.Graphics;
using osu.Game.Graphics;

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

        public virtual Beatmap Decode(TextReader stream)
        {
            Beatmap b = ParseFile(stream);
            Process(b);
            return b;
        }

        public virtual Beatmap Process(Beatmap beatmap)
        {
            ApplyColours(beatmap);

            return beatmap;
        }

        protected abstract Beatmap ParseFile(TextReader stream);

        public virtual void ApplyColours(Beatmap b)
        {
            List<Color4> colours = b.ComboColors ?? new List<Color4> {
                new Color4(17, 136, 170, 255),
                new Color4(102, 136, 0, 255),
                new Color4(204, 102, 0, 255),
                new Color4(121, 9, 13, 255),
            };

            if (colours.Count == 0) return;

            int i = -1;

            foreach (HitObject h in b.HitObjects)
            {
                if (h.NewCombo || i == -1) i = (i + 1) % colours.Count;
                h.Colour = colours[i];
            }
        }
    }
}
