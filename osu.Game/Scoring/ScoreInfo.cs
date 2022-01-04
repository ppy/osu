// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Newtonsoft.Json;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Users;
using osu.Game.Utils;

namespace osu.Game.Scoring
{
    public class ScoreInfo : IScoreInfo, IHasFiles<ScoreFileInfo>, IHasPrimaryKey, ISoftDelete, IEquatable<ScoreInfo>, IDeepCloneable<ScoreInfo>
    {
        public int ID { get; set; }

        public bool IsManaged => ID > 0;

        public ScoreRank Rank { get; set; }

        public long TotalScore { get; set; }

        [Column(TypeName = "DECIMAL(1,4)")] // TODO: This data type is wrong (should contain more precision). But at the same time, we probably don't need to be storing this in the database.
        public double Accuracy { get; set; }

        public LocalisableString DisplayAccuracy => Accuracy.FormatAccuracy();

        public double? PP { get; set; }

        public int MaxCombo { get; set; }

        public int Combo { get; set; } // Todo: Shouldn't exist in here

        public int RulesetID { get; set; }

        [NotMapped]
        public bool Passed { get; set; } = true;

        public RulesetInfo Ruleset { get; set; }

        private APIMod[] localAPIMods;

        private Mod[] mods;

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
                    scoreMods = APIMods.Select(m => m.ToMod(rulesetInstance)).ToArray();

                return scoreMods;
            }
            set
            {
                localAPIMods = null;
                mods = value;
            }
        }

        // Used for API serialisation/deserialisation.
        [NotMapped]
        public APIMod[] APIMods
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
        [Column("Mods")]
        public string ModsJson
        {
            get => JsonConvert.SerializeObject(APIMods);
            set => APIMods = JsonConvert.DeserializeObject<APIMod[]>(value);
        }

        [NotMapped]
        public APIUser User { get; set; }

        [Column("User")]
        public string UserString
        {
            get => User?.Username;
            set
            {
                User ??= new APIUser();
                User.Username = value;
            }
        }

        [Column("UserID")]
        public int? UserID
        {
            get => User?.Id ?? 1;
            set
            {
                User ??= new APIUser();
                User.Id = value ?? 1;
            }
        }

        public int BeatmapInfoID { get; set; }

        [Column("Beatmap")]
        public BeatmapInfo BeatmapInfo { get; set; }

        private long? onlineID;

        [Column("OnlineScoreID")]
        public long? OnlineID
        {
            get => onlineID;
            set => onlineID = value > 0 ? value : null;
        }

        public DateTimeOffset Date { get; set; }

        [NotMapped]
        public Dictionary<HitResult, int> Statistics { get; set; } = new Dictionary<HitResult, int>();

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
        public List<HitEvent> HitEvents { get; set; }

        public List<ScoreFileInfo> Files { get; } = new List<ScoreFileInfo>();

        public string Hash { get; set; }

        public bool DeletePending { get; set; }

        /// <summary>
        /// The position of this score, starting at 1.
        /// </summary>
        [NotMapped]
        public int? Position { get; set; } // TODO: remove after all calls to `CreateScoreInfo` are gone.

        /// <summary>
        /// Whether this <see cref="ScoreInfo"/> represents a legacy (osu!stable) score.
        /// </summary>
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

        public override string ToString() => this.GetDisplayTitle();

        public bool Equals(ScoreInfo other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;

            if (ID != 0 && other.ID != 0)
                return ID == other.ID;

            return false;
        }

        #region Implementation of IHasOnlineID

        long IHasOnlineID<long>.OnlineID => OnlineID ?? -1;

        #endregion

        #region Implementation of IScoreInfo

        IBeatmapInfo IScoreInfo.Beatmap => BeatmapInfo;
        IRulesetInfo IScoreInfo.Ruleset => Ruleset;
        IUser IScoreInfo.User => User;
        bool IScoreInfo.HasReplay => Files.Any();

        #endregion

        IEnumerable<INamedFileUsage> IHasNamedFiles.Files => Files;
    }
}
