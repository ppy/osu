// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Online.API;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Users;
using osu.Game.Utils;

namespace osu.Game.Scoring
{
    public class ScoreInfo : IHasFiles<ScoreFileInfo>, IHasPrimaryKey, ISoftDelete, IEquatable<ScoreInfo>, IDeepCloneable<ScoreInfo>
    {
        public int ID { get; set; }

        [JsonProperty("rank")]
        [JsonConverter(typeof(StringEnumConverter))]
        public ScoreRank Rank { get; set; }

        [JsonProperty("total_score")]
        public long TotalScore { get; set; }

        [JsonProperty("accuracy")]
        [Column(TypeName = "DECIMAL(1,4)")] // TODO: This data type is wrong (should contain more precision). But at the same time, we probably don't need to be storing this in the database.
        public double Accuracy { get; set; }

        [JsonIgnore]
        public LocalisableString DisplayAccuracy => Accuracy.FormatAccuracy();

        [JsonProperty(@"pp")]
        public double? PP { get; set; }

        [JsonProperty("max_combo")]
        public int MaxCombo { get; set; }

        [JsonIgnore]
        public int Combo { get; set; } // Todo: Shouldn't exist in here

        [JsonProperty("ruleset_id")]
        public int RulesetID { get; set; }

        [JsonProperty("passed")]
        [NotMapped]
        public bool Passed { get; set; } = true;

        [JsonIgnore]
        public virtual RulesetInfo Ruleset { get; set; }

        private APIMod[] localAPIMods;
        private Mod[] mods;

        [JsonIgnore]
        [NotMapped]
        public Mod[] Mods
        {
            get
            {
                var rulesetInstance = Ruleset?.CreateInstance();
                if (rulesetInstance == null)
                    return mods ?? Array.Empty<Mod>();

                Mod[] scoreMods = Array.Empty<Mod>();

                if (mods != null)
                    scoreMods = mods;
                else if (localAPIMods != null)
                    scoreMods = apiMods.Select(m => m.ToMod(rulesetInstance)).ToArray();

                return scoreMods;
            }
            set
            {
                localAPIMods = null;
                mods = value;
            }
        }

        // Used for API serialisation/deserialisation.
        [JsonProperty("mods")]
        [NotMapped]
        private APIMod[] apiMods
        {
            get
            {
                if (localAPIMods != null)
                    return localAPIMods;

                if (mods == null)
                    return Array.Empty<APIMod>();

                return localAPIMods = mods.Select(m => new APIMod(m)).ToArray();
            }
            set
            {
                localAPIMods = value;

                // We potentially can't update this yet due to Ruleset being late-bound, so instead update on read as necessary.
                mods = null;
            }
        }

        // Used for database serialisation/deserialisation.
        [JsonIgnore]
        [Column("Mods")]
        public string ModsJson
        {
            get => JsonConvert.SerializeObject(apiMods);
            set => apiMods = JsonConvert.DeserializeObject<APIMod[]>(value);
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
        [Column("Beatmap")]
        public virtual BeatmapInfo BeatmapInfo { get; set; }

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

        /// <summary>
        /// Whether this <see cref="ScoreInfo"/> represents a legacy (osu!stable) score.
        /// </summary>
        [JsonIgnore]
        [NotMapped]
        public bool IsLegacyScore => Mods.OfType<ModClassic>().Any();

        public IEnumerable<HitResultDisplayStatistic> GetStatisticsForDisplay()
        {
            foreach (var r in Ruleset.CreateInstance().GetHitResults())
            {
                int value = Statistics.GetValueOrDefault(r.result);

                switch (r.result)
                {
                    case HitResult.SmallTickHit:
                    {
                        int total = value + Statistics.GetValueOrDefault(HitResult.SmallTickMiss);
                        if (total > 0)
                            yield return new HitResultDisplayStatistic(r.result, value, total, r.displayName);

                        break;
                    }

                    case HitResult.LargeTickHit:
                    {
                        int total = value + Statistics.GetValueOrDefault(HitResult.LargeTickMiss);
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

        public ScoreInfo DeepClone()
        {
            var clone = (ScoreInfo)MemberwiseClone();

            clone.Statistics = new Dictionary<HitResult, int>(clone.Statistics);

            return clone;
        }

        public override string ToString() => $"{User} playing {BeatmapInfo}";

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
