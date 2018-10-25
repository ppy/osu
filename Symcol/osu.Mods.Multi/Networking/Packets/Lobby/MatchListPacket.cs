using System;
using System.Collections.Generic;
using Symcol.Networking.Packets;

namespace osu.Mods.Multi.Networking.Packets.Lobby
{
    [Serializable]
    public class MatchListPacket : Packet
    {
        public override uint PacketSize => Convert.ToUInt32(MatchInfoList.Count > 0 ? MatchInfoList.Count * 1024 + 1024 : 2048);

        public List<MatchInfo> MatchInfoList = new List<MatchInfo>();

        [Serializable]
        public class MatchInfo
        {
            public string Name = @"Welcome to Symcol!";

            public List<OsuUserInfo> Users = new List<OsuUserInfo>();

            #region User

            public string Username = @"Shawdooow";

            public int UserID = 7726082;

            public string UserCountry = "US";

            #endregion

            #region Beatmap

            public string BeatmapTitle = "Lost Emotion";

            public string BeatmapArtist = "Masayoshi Minoshima feat.nomico";

            public string BeatmapMapper = "Shawdooow";

            public string BeatmapDifficulty = "Last Dance Heaven";

            public int OnlineBeatmapSetID = 734008;

            public int OnlineBeatmapID = 1548917;

            public double BeatmapStars = 4.85d;

            #endregion

            public int RulesetID = 0;
        }
    }
}
