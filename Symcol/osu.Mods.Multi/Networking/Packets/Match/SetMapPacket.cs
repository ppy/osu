using System;

namespace osu.Mods.Multi.Networking.Packets.Match
{
    [Serializable]
    public class SetMapPacket : MatchPacket
    {
        public int OnlineBeatmapSetID = -1;

        public int OnlineBeatmapID = -1;

        public string BeatmapTitle;

        public string BeatmapArtist;

        public string BeatmapMapper;

        public string BeatmapDifficulty;

        public int RulesetID;
    }
}
