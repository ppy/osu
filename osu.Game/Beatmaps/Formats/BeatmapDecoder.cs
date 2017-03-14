// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.IO;
using osu.Game.Modes.Objects;
using OpenTK.Graphics;
using osu.Game.Beatmaps.Timing;
using osu.Game.Database;
using osu.Game.Modes.Objects.Types;

namespace osu.Game.Beatmaps.Formats
{
    public abstract class BeatmapDecoder
    {
        private static Dictionary<string, Type> decoders { get; } = new Dictionary<string, Type>();

        public static BeatmapDecoder GetDecoder(TextReader stream)
        {
            var line = stream.ReadLine()?.Trim();

            if (line == null || !decoders.ContainsKey(line))
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

        public virtual void Decode(TextReader stream, Beatmap beatmap)
        {
            ParseFile(stream, beatmap);
        }

        public virtual Beatmap Process(Beatmap beatmap)
        {
            int comboIndex = 0;
            int colourIndex = 0;

            foreach (var obj in beatmap.HitObjects)
            {
                HitObjectWithCombo comboObject = obj as HitObjectWithCombo;

                if (comboObject == null || comboObject.NewCombo)
                {
                    comboIndex = 0;
                    colourIndex = (colourIndex + 1) % beatmap.ComboColors.Count;
                }

                if (comboObject != null)
                {
                    comboObject.ComboIndex = comboIndex++;
                    comboObject.ComboColour = beatmap.ComboColors[colourIndex];
                }
            }

            return beatmap;
        }

        protected virtual Beatmap ParseFile(TextReader stream)
        {
            var beatmap = new Beatmap
            {
                HitObjects = new List<HitObject>(),
                ControlPoints = new List<ControlPoint>(),
                ComboColors = new List<Color4> {
                    new Color4(17, 136, 170, 255),
                    new Color4(102, 136, 0, 255),
                    new Color4(204, 102, 0, 255),
                    new Color4(121, 9, 13, 255),
                },
                BeatmapInfo = new BeatmapInfo
                {
                    Metadata = new BeatmapMetadata(),
                    BaseDifficulty = new BaseDifficulty(),
                },
            };
            ParseFile(stream, beatmap);
            return beatmap;
        }
        protected abstract void ParseFile(TextReader stream, Beatmap beatmap);
    }
}
