// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Newtonsoft.Json;
using osu.Framework.Localisation;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Models;
using osu.Game.Online.API;
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
        public RealmUser RealmUser { get; set; } = null!;

        public IUser User
        {
            get => RealmUser;
            set => RealmUser = new RealmUser
            {
                OnlineID = value.OnlineID,
                Username = value.Username
            };
        }

        public long TotalScore { get; set; }

        public int MaxCombo { get; set; }

        public double Accuracy { get; set; }

        public bool HasReplay { get; set; }

        public DateTimeOffset Date { get; set; }

        public double? PP { get; set; }

        public RealmBeatmap Beatmap { get; set; } = null!;

        public BeatmapInfo BeatmapInfo
        {
            get => new BeatmapInfo();
            // .. todo
            set => Beatmap = new RealmBeatmap(new RealmRuleset("osu", "osu!", "wangs", 0), new RealmBeatmapDifficulty(), new RealmBeatmapMetadata());
        }

        public RealmRuleset Ruleset { get; set; } = null!;

        [Ignored]
        public Dictionary<HitResult, int> Statistics
        {
            // TODO: this is dangerous. a get operation may then modify the dictionary, which would be a fresh copy that is not persisted with the model.
            // this is already the case in multiple locations.
            get
            {
                if (string.IsNullOrEmpty(StatisticsJson))
                    return new Dictionary<HitResult, int>();

                return JsonConvert.DeserializeObject<Dictionary<HitResult, int>>(StatisticsJson) ?? new Dictionary<HitResult, int>();
            }
            set => JsonConvert.SerializeObject(StatisticsJson);
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

        public int BeatmapInfoID => BeatmapInfo.ID;

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

        [Ignored]
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
            }
        }

        // Used for API serialisation/deserialisation.
        [Ignored]
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
            set => APIMods = JsonConvert.DeserializeObject<APIMod[]>(value) ?? Array.Empty<APIMod>();
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
