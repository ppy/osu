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
        public int ID { get; set; }

        public ScoreRank Rank { get; set; }

        public int TotalScore { get; set; }

        [Column(TypeName="DECIMAL(1,4)")]
        public double Accuracy { get; set; }

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
                if (mods != null)
                    return mods;

                if (modsJson == null)
                    return Array.Empty<Mod>();

                return getModsFromRuleset(JsonConvert.DeserializeObject<DeserializedMod[]>(modsJson));
            }
            set
            {
                modsJson = null;
                mods = value;
            }
        }

        private Mod[] getModsFromRuleset(DeserializedMod[] mods) => Ruleset.CreateInstance().GetAllMods().Where(mod => mods.Any(d => d.Acronym == mod.Acronym)).ToArray();

        private string modsJson;

        [Column("Mods")]
        public string ModsJson
        {
            get
            {
                if (modsJson != null)
                    return modsJson;

                if (mods == null)
                    return null;

                return modsJson = JsonConvert.SerializeObject(mods);
            }
            set
            {
                modsJson = value;

                // we potentially can't update this yet due to Ruleset being late-bound, so instead update on read as necessary.
                mods = null;
            }
        }

        [JsonIgnore]
        public User User;

        [Column("User")]
        public string UserString
        {
            get => User?.Username;
            set => User = new User { Username = value };
        }

        [JsonIgnore]
        public int BeatmapInfoID { get; set; }

        public virtual BeatmapInfo Beatmap { get; set; }

        public long? OnlineScoreID { get; set; }

        public DateTimeOffset Date { get; set; }

        [JsonIgnore]
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

        [JsonIgnore]
        public List<ScoreFileInfo> Files { get; set; }

        public string Hash { get; set; }

        public bool DeletePending { get; set; }

        [Serializable]
        protected class DeserializedMod : IMod
        {
            public string Acronym { get; set; }
        }
    }
}
