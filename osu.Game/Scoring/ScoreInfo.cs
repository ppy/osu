// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Newtonsoft.Json;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Users;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Scoring
{
    public class ScoreInfo : IHasFiles<ScoreFileInfo>, IHasPrimaryKey, ISoftDelete
    {
        [JsonIgnore]
        public int ID { get; set; }

        public ScoreRank Rank { get; set; }

        public double TotalScore { get; set; }

        public double Accuracy { get; set; }

        public double Health { get; set; } = 1;

        public double? PP { get; set; }

        public int MaxCombo { get; set; }

        public int Combo { get; set; }

        public int RulesetID { get; set; }

        public RulesetInfo Ruleset { get; set; }

        [NotMapped, JsonIgnore]
        public Mod[] Mods
        {
            get
            {
                var deserialized = JsonConvert.DeserializeObject<string[]>(modsString);
                return Ruleset.CreateInstance().GetAllMods().Where(mod => deserialized.Any(d => d == mod.ShortenedName)).ToArray();
            }
            set => modsString = JsonConvert.SerializeObject(value.Select(m => m.ShortenedName).ToArray());
        }

        [NotMapped, JsonIgnore]
        private string modsString;

        public string ModsString
        {
            get => modsString;
            set => modsString = value;
        }

        [NotMapped, JsonIgnore]
        public User User;

        public string UserString
        {
            get => User?.Username;
            set => User = new User { Username = value };
        }

        public int BeatmapInfoID { get; set; }

        public BeatmapInfo BeatmapInfo;

        public long? OnlineScoreID { get; set; }

        public DateTimeOffset Date;

        public Dictionary<HitResult, object> Statistics = new Dictionary<HitResult, object>();

        /// <summary>
        /// MD5 is kept for legacy support.
        /// </summary>
        [JsonProperty("file_md5")]
        public string MD5Hash { get; set; }

        public List<ScoreFileInfo> Files { get; set; }

        public bool DeletePending { get; set; }
    }
}
