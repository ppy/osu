// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using osu.Game.Database;
using osu.Game.Rulesets.Mods;
using osu.Game.Users;
using osu.Game.Rulesets.Replays;

namespace osu.Game.Rulesets.Scoring
{
    public class Score
    {
        [JsonProperty(@"rank")]
        public ScoreRank Rank { get; set; }

        [JsonProperty(@"score")]
        public double TotalScore { get; set; }

        [JsonProperty(@"accuracy")]
        public double Accuracy { get; set; }

        public double Health { get; set; } = 1;

        [JsonProperty(@"combo")]
        public int MaxCombo { get; set; }

        public int Combo { get; set; }

        [JsonProperty(@"mods")]
        protected string[] ModStrings { get; set; } //todo: parse to Mod objects

        public Mod[] Mods { get; set; }

        [JsonProperty(@"user")]
        public User User;

        [JsonProperty(@"replay_data")]
        public Replay Replay;

        public BeatmapInfo Beatmap;

        [JsonProperty(@"score_id")]
        public long OnlineScoreID;

        [JsonProperty(@"created_at")]
        public DateTime Date;

        public Dictionary<string, dynamic> Statistics = new Dictionary<string, dynamic>();
    }
}
