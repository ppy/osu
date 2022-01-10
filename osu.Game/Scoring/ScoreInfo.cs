// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using JetBrains.Annotations;
using Newtonsoft.Json;
using osu.Framework.Localisation;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Models;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Users;
using osu.Game.Utils;
using Realms;

#nullable enable

namespace osu.Game.Scoring
{
    [ExcludeFromDynamicCompile]
    [MapTo("Score")]
    public class ScoreInfo : RealmObject, IHasGuidPrimaryKey, IHasRealmFiles, ISoftDelete, IEquatable<ScoreInfo>, IScoreInfo
    {
        [PrimaryKey]
        public Guid ID { get; set; } = Guid.NewGuid();

        public IList<RealmNamedFileUsage> Files { get; } = null!;

        public string Hash { get; set; } = string.Empty;

        public bool DeletePending { get; set; }

        public bool Equals(ScoreInfo other) => other.ID == ID;

        [Indexed]
        public long OnlineID { get; set; } = -1;

        [MapTo("User")]
        public RealmUser RealmUser { get; set; } = new RealmUser();

        public ScoreInfo(BeatmapInfo beatmap, RulesetInfo ruleset, RealmUser realmUser)
        {
            Ruleset = ruleset;
            Beatmap = beatmap;
            RealmUser = realmUser;
        }

        [UsedImplicitly]
        public ScoreInfo() // TODO: consider removing this and migrating all usages to ctor with parameters.
        {
        }

        // TODO: this is a bit temporary to account for the fact that this class is used to ferry API user data to certain UI components.
        // Eventually we should either persist enough information to realm to not require the API lookups, or perform the API lookups locally.
        private APIUser? user;

        [IgnoreMap]
        public APIUser User
        {
            get => user ??= new APIUser
            {
                Username = RealmUser.Username,
                Id = RealmUser.OnlineID,
            };
            set
            {
                user = value;

                RealmUser = new RealmUser
                {
                    OnlineID = user.OnlineID,
                    Username = user.Username
                };
            }
        }

        public long TotalScore { get; set; }

        public int MaxCombo { get; set; }

        public double Accuracy { get; set; }

        public bool HasReplay { get; set; }

        public DateTimeOffset Date { get; set; }

        public double? PP { get; set; }

        public BeatmapInfo Beatmap { get; set; } = null!;

        public RulesetInfo Ruleset { get; set; } = null!;

        private Dictionary<HitResult, int>? statistics;

        [Ignored]
        public Dictionary<HitResult, int> Statistics
        {
            get
            {
                if (statistics != null)
                    return statistics;

                if (!string.IsNullOrEmpty(StatisticsJson))
                    statistics = JsonConvert.DeserializeObject<Dictionary<HitResult, int>>(StatisticsJson);

                return statistics ??= new Dictionary<HitResult, int>();
            }
            set => statistics = value;
        }

        [MapTo("Statistics")]
        public string StatisticsJson { get; set; } = null!;

        public ScoreRank Rank
        {
            get => (ScoreRank)RankInt;
            set => RankInt = (int)value;
        }

        [MapTo(nameof(Rank))]
        public int RankInt { get; set; }

        IRulesetInfo IScoreInfo.Ruleset => Ruleset;
        IBeatmapInfo IScoreInfo.Beatmap => Beatmap;
        IUser IScoreInfo.User => User;
        IEnumerable<INamedFileUsage> IHasNamedFiles.Files => Files;

        #region Properties required to make things work with existing usages

        private APIMod[]? localAPIMods;

        private Mod[]? mods;

        public Guid BeatmapInfoID => Beatmap.ID;

        public BeatmapInfo BeatmapInfo
        {
            get => Beatmap;
            set => Beatmap = value;
        }

        public int UserID => RealmUser.OnlineID;

        public int RulesetID => Ruleset.OnlineID;

        [Ignored]
        public List<HitEvent> HitEvents { get; set; } = new List<HitEvent>();

        public ScoreInfo DeepClone()
        {
            var clone = (ScoreInfo)MemberwiseClone();

            clone.Statistics = new Dictionary<HitResult, int>(clone.Statistics);

            return clone;
        }

        [Ignored]
        public bool Passed { get; set; } = true;

        public int Combo { get; set; }

        /// <summary>
        /// The position of this score, starting at 1.
        /// </summary>
        [Ignored]
        public int? Position { get; set; } // TODO: remove after all calls to `CreateScoreInfo` are gone.

        [Ignored]
        public LocalisableString DisplayAccuracy => Accuracy.FormatAccuracy();

        /// <summary>
        /// Whether this <see cref="EFScoreInfo"/> represents a legacy (osu!stable) score.
        /// </summary>
        [Ignored]
        public bool IsLegacyScore => Mods.OfType<ModClassic>().Any();

        // Used for database serialisation/deserialisation.
        [MapTo("Mods")]
        public string ModsJson { get; set; } = string.Empty;

        [Ignored]
        public Mod[] Mods
        {
            get
            {
                var rulesetInstance = Ruleset.CreateInstance();

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
                ModsJson = JsonConvert.SerializeObject(APIMods);
            }
        }

        // Used for API serialisation/deserialisation.
        [Ignored]
        public APIMod[] APIMods
        {
            get
            {
                if (localAPIMods == null)
                {
                    // prioritise reading from realm backing
                    if (!string.IsNullOrEmpty(ModsJson))
                        localAPIMods = JsonConvert.DeserializeObject<APIMod[]>(ModsJson);

                    // then check mods set via Mods property.
                    if (mods != null)
                        localAPIMods = mods.Select(m => new APIMod(m)).ToArray();
                }

                return localAPIMods ?? Array.Empty<APIMod>();
            }
            set
            {
                localAPIMods = value;

                // We potentially can't update this yet due to Ruleset being late-bound, so instead update on read as necessary.
                mods = null;
                ModsJson = JsonConvert.SerializeObject(APIMods);
            }
        }

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

        #endregion
    }
}
