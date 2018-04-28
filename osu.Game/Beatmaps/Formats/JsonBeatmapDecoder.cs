// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.IO;
using osu.Game.IO.Serialization;

namespace osu.Game.Beatmaps.Formats
{
    public class JsonBeatmapDecoder : Decoder<Beatmap>
    {
        public static void Register()
        {
            AddDecoder<Beatmap>("{", m => new JsonBeatmapDecoder());
        }

        protected override void ParseStreamInto(StreamReader stream, Beatmap output)
        {
            stream.BaseStream.Position = 0;
            stream.DiscardBufferedData();

            stream.ReadToEnd().DeserializeInto(output);

            foreach (var hitObject in output.HitObjects)
                hitObject.ApplyDefaults(output.ControlPointInfo, output.BeatmapInfo.BaseDifficulty);
        }
    }
}
