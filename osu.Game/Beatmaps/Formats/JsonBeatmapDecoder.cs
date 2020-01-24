// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.IO;
using osu.Game.IO.Serialization;

namespace osu.Game.Beatmaps.Formats
{
    public class JsonBeatmapDecoder : Decoder<Beatmap>
    {
        public static void Register()
        {
            AddDecoder<Beatmap>("{", m => new JsonBeatmapDecoder());
        }

        protected override void ParseStreamInto(LineBufferedReader stream, Beatmap output)
        {
            stream.ReadToEnd().DeserializeInto(output);

            foreach (var hitObject in output.HitObjects)
                hitObject.ApplyDefaults(output.ControlPointInfo, output.BeatmapInfo.BaseDifficulty);
        }
    }
}
