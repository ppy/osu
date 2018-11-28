// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Users;
using osu.Game.Rulesets.Replays;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Scoring
{
    public class Score : IHasFiles<ScoreFileInfo>, IHasPrimaryKey, ISoftDelete
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

        public RulesetInfo Ruleset { get; set; }

        [NotMapped]
        [JsonIgnore]
        public Mod[] Mods { get; set; } = { };

        public string ModsString
        {
            get => JsonConvert.SerializeObject(Mods);
            set
            {
                var deserialized = JsonConvert.DeserializeObject<SerializableMod[]>(value);
                Mods = Ruleset.CreateInstance().GetAllMods().Where(mod => deserialized.Any(d => d.ShortenedName == mod.ShortenedName)).ToArray();
            }
        }

        [NotMapped]
        [JsonIgnore]
        public User User;

        public string UserString
        {
            get => User?.Username;
            set => User = new User { Username = value };
        }

        [JsonIgnore]
        public Replay Replay;

        public BeatmapInfo Beatmap;

        public long OnlineScoreID;

        public DateTimeOffset Date;

        public Dictionary<HitResult, object> Statistics = new Dictionary<HitResult, object>();

        public List<ScoreFileInfo> Files { get; set; }

        public bool DeletePending { get; set; }

        [UsedImplicitly]
        private class SerializableMod : Mod
        {
            public override string Name => ShortenedName;

            public override string ShortenedName { get; } = string.Empty;

            public override double ScoreMultiplier => 0;
        }
    }
}
