// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using osu.Framework.Extensions;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Users;
using osu.Game.Utils;

namespace osu.Game.Scoring
{
    public class ScoreInfo : IHasFiles<ScoreFileInfo>, IHasPrimaryKey, ISoftDelete, IEquatable<ScoreInfo>
    {
        public int ID { get; set; }

        [JsonProperty("rank")]
        [JsonConverter(typeof(StringEnumConverter))]
        public ScoreRank Rank { get; set; }

        [JsonProperty("total_score")]
        public long TotalScore { get; set; }

        [JsonProperty("accuracy")]
        [Column(TypeName = "DECIMAL(1,4)")]
        public double Accuracy { get; set; }

        [JsonIgnore]
        public string DisplayAccuracy => Accuracy.FormatAccuracy();

        [JsonProperty(@"pp")]
        public double? PP { get; set; }

        [JsonProperty("max_combo")]
        public int MaxCombo { get; set; }

        [JsonIgnore]
        public int Combo { get; set; } // Todo: Shouldn't exist in here

        [JsonIgnore]
        public int RulesetID { get; set; }

        [JsonProperty("passed")]
        [NotMapped]
        public bool Passed { get; set; } = true;

        [JsonIgnore]
        public virtual RulesetInfo Ruleset { get; set; }

        private Mod[] mods;

        [JsonProperty("mods")]
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

        [JsonIgnore]
        [Column("Mods")]
        public string ModsJson
        {
            get
            {
                if (modsJson != null)
                    return modsJson;

                if (mods == null)
                    return null;

                return modsJson = JsonConvert.SerializeObject(mods.Select(m => new DeserializedMod { Acronym = m.Acronym }));
            }
            set
            {
                modsJson = value;

                // we potentially can't update this yet due to Ruleset being late-bound, so instead update on read as necessary.
                mods = null;
            }
        }

        [NotMapped]
        [JsonProperty("user")]
        public User User { get; set; }

        [JsonIgnore]
        [Column("User")]
        public string UserString
        {
            get => User?.Username;
            set
            {
                User ??= new User();
                User.Username = value;
            }
        }

        [JsonIgnore]
        [Column("UserID")]
        public int? UserID
        {
            get => User?.Id ?? 1;
            set
            {
                User ??= new User();
                User.Id = value ?? 1;
            }
        }

        [JsonIgnore]
        public int BeatmapInfoID { get; set; }

        [JsonIgnore]
        public virtual BeatmapInfo Beatmap { get; set; }

        [JsonIgnore]
        public long? OnlineScoreID { get; set; }

        [JsonIgnore]
        public DateTimeOffset Date { get; set; }

        [JsonProperty("statistics")]
        public Dictionary<HitResult, int> Statistics = new Dictionary<HitResult, int>();

        [JsonIgnore]
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

                Statistics = JsonConvert.DeserializeObject<Dictionary<HitResult, int>>(value);
            }
        }

        [NotMapped]
        [JsonIgnore]
        public List<HitEvent> HitEvents { get; set; }

        [JsonIgnore]
        public List<ScoreFileInfo> Files { get; set; }

        [JsonIgnore]
        public string Hash { get; set; }

        [JsonIgnore]
        public bool DeletePending { get; set; }

        /// <summary>
        /// The position of this score, starting at 1.
        /// </summary>
        [NotMapped]
        [JsonProperty("position")]
        public int? Position { get; set; }

        private bool isLegacyScore;

        /// <summary>
        /// Whether this <see cref="ScoreInfo"/> represents a legacy (osu!stable) score.
        /// </summary>
        [JsonIgnore]
        [NotMapped]
        public bool IsLegacyScore
        {
            get
            {
                if (isLegacyScore)
                    return true;

                // The above check will catch legacy online scores that have an appropriate UserString + UserId.
                // For non-online scores such as those imported in, a heuristic is used based on the following table:
                //
                //       Mode      | UserString | UserId
                // --------------- | ---------- | ---------
                // stable          | <username> | 1
                // lazer           | <username> | <userid>
                // lazer (offline) | Guest      | 1

                return ID > 0 && UserID == 1 && UserString != "Guest";
            }
            set => isLegacyScore = value;
        }

        public IEnumerable<HitResultDisplayStatistic> GetStatisticsForDisplay()
        {
            foreach (var r in Ruleset.CreateInstance().GetHitResults())
            {
                int value = Statistics.GetOrDefault(r.result);

                switch (r.result)
                {
                    case HitResult.SmallTickHit:
                    {
                        int total = value + Statistics.GetOrDefault(HitResult.SmallTickMiss);
                        if (total > 0)
                            yield return new HitResultDisplayStatistic(r.result, value, total, r.displayName);

                        break;
                    }

                    case HitResult.LargeTickHit:
                    {
                        int total = value + Statistics.GetOrDefault(HitResult.LargeTickMiss);
                        if (total > 0)
                            yield return new HitResultDisplayStatistic(r.result, value, total, r.displayName);

                        break;
                    }

                    case HitResult.SmallTickMiss:
                    case HitResult.LargeTickMiss:
                        break;

                    default:
                        yield return new HitResultDisplayStatistic(r.result, value, null, r.displayName);

                        break;
                }
            }
        }

        [Serializable]
        protected class DeserializedMod : IMod
        {
            public string Acronym { get; set; }

            public bool Equals(IMod other) => Acronym == other?.Acronym;
        }

        public override string ToString() => $"{User} playing {Beatmap}";

        public bool Equals(ScoreInfo other)
        {
            if (other == null)
                return false;

            if (ID != 0 && other.ID != 0)
                return ID == other.ID;

            if (OnlineScoreID.HasValue && other.OnlineScoreID.HasValue)
                return OnlineScoreID == other.OnlineScoreID;

            if (!string.IsNullOrEmpty(Hash) && !string.IsNullOrEmpty(other.Hash))
                return Hash == other.Hash;

            return ReferenceEquals(this, other);
        }
    }
}
