// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using osu.Game.Database;
using osu.Game.Rulesets.Mods;
using osu.Game.Users;
using System.IO;
using osu.Game.Rulesets.Replays;

namespace osu.Game.Rulesets.Scoring
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

        private User user;

        public User User
        {
            get
            {
                return user ?? new User
                {
                    Username = LegacyUsername,
                    Id = LegacyUserID
                };
            }

            set
            {
                user = value;
            }
        }

        [JsonProperty(@"replay_data")]
        public Replay Replay;

        public BeatmapInfo Beatmap;

        [JsonProperty(@"score_id")]
        public long OnlineScoreID;

        [JsonProperty(@"username")]
        public string LegacyUsername;

        [JsonProperty(@"user_id")]
        public long LegacyUserID;

        [JsonProperty(@"date")]
        public DateTime Date;

        /// <summary>
        /// Creates a replay which is read from a stream.
        /// </summary>
        /// <param name="reader">The stream reader.</param>
        /// <returns>The replay.</returns>
        public virtual Replay CreateReplay(StreamReader reader)
        {
            var frames = new List<ReplayFrame>();

            float lastTime = 0;

            foreach (var l in reader.ReadToEnd().Split(','))
            {
                var split = l.Split('|');

                if (split.Length < 4 || float.Parse(split[0]) < 0) continue;

                lastTime += float.Parse(split[0]);

                frames.Add(new ReplayFrame(
                    lastTime,
                    float.Parse(split[1]),
                    384 - float.Parse(split[2]),
                    (ReplayButtonState)int.Parse(split[3])
                    ));
            }

            return new Replay { Frames = frames };
        }

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
