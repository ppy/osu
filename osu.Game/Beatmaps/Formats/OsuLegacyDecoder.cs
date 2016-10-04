using System;
using System.IO;

namespace osu.Game.Beatmaps.Formats
{
    public class OsuLegacyDecoder : BeatmapDecoder
    {
        static OsuLegacyDecoder()
        {
            AddDecoder<OsuLegacyDecoder>("osu file format v14");
            AddDecoder<OsuLegacyDecoder>("osu file format v13");
            AddDecoder<OsuLegacyDecoder>("osu file format v12");
            AddDecoder<OsuLegacyDecoder>("osu file format v11");
            AddDecoder<OsuLegacyDecoder>("osu file format v10");
            // TODO: Not sure how far back to go, or differences between versions
        }
        private enum Section
        {
            General,
            Editor,
            Metadata,
            Difficulty,
            Events,
            TimingPoints,
            Colours,
            HitObjects,
        }

        public override Beatmap Decode(TextReader stream)
        {
            throw new NotImplementedException();
        }
    }
}