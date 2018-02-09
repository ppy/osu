// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.IO;
using osu.Game.IO.Serialization;
using osu.Game.Storyboards;

namespace osu.Game.Beatmaps.Formats
{
    public class JsonBeatmapDecoder : Decoder
    {
        public static void Register()
        {
            AddDecoder("{", m => new JsonBeatmapDecoder());
        }

        public override Decoder GetStoryboardDecoder() => this;

        protected override void ParseBeatmap(StreamReader stream, Beatmap beatmap)
        {
            stream.BaseStream.Position = 0;
            stream.DiscardBufferedData();

            stream.ReadToEnd().DeserializeInto(beatmap);

            foreach (var hitObject in beatmap.HitObjects)
                hitObject.ApplyDefaults(beatmap.ControlPointInfo, beatmap.BeatmapInfo.BaseDifficulty);
        }

        protected override void ParseStoryboard(StreamReader stream, Storyboard storyboard)
        {
            // throw new System.NotImplementedException();
        }
    }
}
