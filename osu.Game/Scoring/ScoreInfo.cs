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

        public virtual RulesetInfo Ruleset { get; set; }

        private Mod[] mods;

        [NotMapped]
        public Mod[] Mods
        {
            get
            {
                if (mods != null) return mods;

                if (modsJson == null)
                    return Array.Empty<Mod>();

                return getModsFromRuleset(JsonConvert.DeserializeObject<DeserializedMod[]>(modsJson));
            }
            set
            {
                mods = value;
                ModsJson = null;
            }
        }

        private Mod[] getModsFromRuleset(DeserializedMod[] mods) => Ruleset.CreateInstance().GetAllMods().Where(mod => mods.Any(d => d.ShortenedName == mod.ShortenedName)).ToArray();

        private string modsJson;

        [Column("Mods")]
        public string ModsJson
        {
            get => modsJson ?? JsonConvert.SerializeObject(Mods);
            set
            {
                modsJson = value;

                // we potentially can't update this yet due to Ruleset being late-bound, so instead update on read as necessary.
                mods = null;
            }
        }

        public User User;

        [Column("User")]
        public string UserString
        {
            get => User?.Username;
            set => User = new User { Username = value };
        }

        public int BeatmapInfoID { get; set; }

        public virtual BeatmapInfo Beatmap { get; set; }

        public long? OnlineScoreID { get; set; }

        public DateTimeOffset Date { get; set; }

        public Dictionary<HitResult, object> Statistics = new Dictionary<HitResult, object>();

        [Column("Statistics")]
        public string StatisticsJson
        {
            get => JsonConvert.SerializeObject(Statistics);
            set
            {
                if (value == null)
                {
                    Statistics.Clear();
                    return;
                }

                Statistics = JsonConvert.DeserializeObject<Dictionary<HitResult, object>>(value);
            }
        }

        /// <summary>
        /// MD5 is kept for legacy support.
        /// </summary>
        [JsonProperty("file_md5")]
        public string MD5Hash { get; set; }

        public List<ScoreFileInfo> Files { get; set; }

        public bool DeletePending { get; set; }

        [Serializable]
        protected class DeserializedMod : Mod
        {
            public override string Name { get; } = string.Empty;
            public override string ShortenedName { get; } = string.Empty;
            public override double ScoreMultiplier { get; } = 0;
        }
    }
}
