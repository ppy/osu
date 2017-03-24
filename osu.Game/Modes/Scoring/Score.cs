// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using Newtonsoft.Json;
using osu.Game.Database;
using osu.Game.Modes.Mods;
using osu.Game.Users;

namespace osu.Game.Modes.Scoring
{
    public class Score
    {
        [JsonProperty(@"rank")]
        public ScoreRank Rank { get; set; }

        [JsonProperty(@"score")]
        public double TotalScore { get; set; }
        public double Accuracy { get; set; }
        public double Health { get; set; }

        [JsonProperty(@"maxcombo")]
        public int MaxCombo { get; set; }
        public int Combo { get; set; }
        public Mod[] Mods { get; set; }

        public User User { get; set; }

        [JsonProperty(@"replay_data")]
        public Replay Replay;

        public BeatmapInfo Beatmap;

        [JsonProperty(@"score_id")]
        public long OnlineScoreID;

        [JsonProperty(@"username")]
        public string Username;

        [JsonProperty(@"user_id")]
        public long UserID;

        [JsonProperty(@"date")]
        public DateTime Date;

        //  [JsonProperty(@"count50")] 0,
        //[JsonProperty(@"count100")] 0,
        //[JsonProperty(@"count300")] 100,
        //[JsonProperty(@"countmiss")] 0,
        //[JsonProperty(@"countkatu")] 0,
        //[JsonProperty(@"countgeki")] 31,
        //[JsonProperty(@"perfect")] true,
        //[JsonProperty(@"enabled_mods")] [
        //  "DT",
        //  "FL",
        //  "HD",
        //  "HR"
        //],
        //[JsonProperty(@"rank")] "XH",
        //[JsonProperty(@"pp")] 26.1816,
        //[JsonProperty(@"replay")] true
    }
}
