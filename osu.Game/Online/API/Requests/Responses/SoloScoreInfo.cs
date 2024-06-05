// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Users;

namespace osu.Game.Online.API.Requests.Responses
{
    [Serializable]
    public class SoloScoreInfo : IScoreInfo
    {
        [JsonProperty("beatmap_id")]
        public int BeatmapID { get; set; }

        [JsonProperty("ruleset_id")]
        public int RulesetID { get; set; }

        [JsonProperty("build_id")]
        public int? BuildID { get; set; }

        [JsonProperty("passed")]
        public bool Passed { get; set; }

        [JsonProperty("total_score")]
        public long TotalScore { get; set; }

        [JsonProperty("total_score_without_mods")]
        public long TotalScoreWithoutMods { get; set; }

        [JsonProperty("accuracy")]
        public double Accuracy { get; set; }

        [JsonProperty("user_id")]
        public int UserID { get; set; }

        // TODO: probably want to update this column to match user stats (short)?
        [JsonProperty("max_combo")]
        public int MaxCombo { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        // ScoreRank is aligned to make 0 equal D. We still want to serialise this (even when DefaultValueHandling.Ignore is used).
        [JsonProperty("rank", DefaultValueHandling = DefaultValueHandling.Include)]
        public ScoreRank Rank { get; set; }

        [JsonProperty("started_at")]
        public DateTimeOffset? StartedAt { get; set; }

        [JsonProperty("ended_at")]
        public DateTimeOffset EndedAt { get; set; }

        [JsonProperty("mods")]
        public APIMod[] Mods { get; set; } = Array.Empty<APIMod>();

        [JsonIgnore]
        [JsonProperty("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonIgnore]
        [JsonProperty("updated_at")]
        public DateTimeOffset UpdatedAt { get; set; }

        [JsonIgnore]
        [JsonProperty("deleted_at")]
        public DateTimeOffset? DeletedAt { get; set; }

        [JsonProperty("statistics")]
        public Dictionary<HitResult, int> Statistics { get; set; } = new Dictionary<HitResult, int>();

        [JsonProperty("maximum_statistics")]
        public Dictionary<HitResult, int> MaximumStatistics { get; set; } = new Dictionary<HitResult, int>();

        /// <summary>
        /// Used to preserve the total score for legacy scores.
        /// </summary>
        [JsonProperty("legacy_total_score")]
        public int? LegacyTotalScore { get; set; }

        [JsonProperty("legacy_score_id")]
        public ulong? LegacyScoreId { get; set; }

        #region osu-web API additions (not stored to database).

        [JsonProperty("id")]
        public ulong? ID { get; set; }

        [JsonProperty("user")]
        public APIUser? User { get; set; }

        [JsonProperty("beatmap")]
        public APIBeatmap? Beatmap { get; set; }

        [JsonProperty("beatmapset")]
        public APIBeatmapSet? BeatmapSet
        {
            set
            {
                // in the deserialisation case we need to ferry this data across.
                // the order of properties returned by the API guarantees that the beatmap is populated by this point.
                if (!(Beatmap is APIBeatmap apiBeatmap))
                    throw new InvalidOperationException("Beatmap set metadata arrived before beatmap metadata in response");

                apiBeatmap.BeatmapSet = value;
            }
        }

        [JsonProperty("pp")]
        public double? PP { get; set; }

        [JsonProperty("has_replay")]
        public bool HasReplay { get; set; }

        [JsonProperty("ranked")]
        public bool Ranked { get; set; }

        // These properties are calculated or not relevant to any external usage.
        public bool ShouldSerializeID() => false;
        public bool ShouldSerializeUser() => false;
        public bool ShouldSerializeBeatmap() => false;
        public bool ShouldSerializeBeatmapSet() => false;
        public bool ShouldSerializePP() => false;
        public bool ShouldSerializeOnlineID() => false;
        public bool ShouldSerializeHasReplay() => false;

        // These fields only need to be serialised if they hold values.
        // Generally this is required because this model may be used by server-side components, but
        // we don't want to bother sending these fields in score submission requests, for instance.
        public bool ShouldSerializeEndedAt() => EndedAt != default;
        public bool ShouldSerializeStartedAt() => StartedAt != default;
        public bool ShouldSerializeLegacyScoreId() => LegacyScoreId != null;
        public bool ShouldSerializeLegacyTotalScore() => LegacyTotalScore != null;
        public bool ShouldSerializeMods() => Mods.Length > 0;
        public bool ShouldSerializeUserID() => UserID > 0;
        public bool ShouldSerializeBeatmapID() => BeatmapID > 0;
        public bool ShouldSerializeBuildID() => BuildID != null;

        #endregion

        #region IScoreInfo

        public long OnlineID => (long?)ID ?? -1;

        IUser IScoreInfo.User => User!;
        DateTimeOffset IScoreInfo.Date => EndedAt;
        long IScoreInfo.LegacyOnlineID => (long?)LegacyScoreId ?? -1;
        IBeatmapInfo IScoreInfo.Beatmap => Beatmap!;
        IRulesetInfo IScoreInfo.Ruleset => Beatmap!.Ruleset;

        #endregion

        /// <summary>
        /// Whether this <see cref="ScoreInfo"/> represents a legacy (osu!stable) score.
        /// </summary>
        [JsonIgnore]
        public bool IsLegacyScore => LegacyScoreId != null;

        public override string ToString() => $"score_id: {ID} user_id: {UserID}";

        /// <summary>
        /// Create a <see cref="ScoreInfo"/> from an API score instance.
        /// </summary>
        /// <param name="rulesets">A ruleset store, used to populate a ruleset instance in the returned score.</param>
        /// <param name="beatmap">An optional beatmap, copied into the returned score (for cases where the API does not populate the beatmap).</param>
        /// <returns></returns>
        public ScoreInfo ToScoreInfo(RulesetStore rulesets, BeatmapInfo? beatmap = null)
        {
            var ruleset = rulesets.GetRuleset(RulesetID) ?? throw new InvalidOperationException($"Ruleset with ID of {RulesetID} not found locally");

            var rulesetInstance = ruleset.CreateInstance();

            var mods = Mods.Select(apiMod => apiMod.ToMod(rulesetInstance)).ToArray();

            var scoreInfo = ToScoreInfo(mods, beatmap);
            scoreInfo.Ruleset = ruleset;

            return scoreInfo;
        }

        /// <summary>
        /// Create a <see cref="ScoreInfo"/> from an API score instance.
        /// </summary>
        /// <param name="mods">The mod instances, resolved from a ruleset.</param>
        /// <param name="beatmap">The object to populate the scores' beatmap with.
        ///<list type="bullet">
        /// <item>If this is a <see cref="BeatmapInfo"/> type, then the score will be fully populated with the given object.</item>
        /// <item>Otherwise, if this is an <see cref="IBeatmapInfo"/> type (e.g. <see cref="APIBeatmap"/>), then only the beatmap ruleset will be populated.</item>
        /// <item>Otherwise, if this is <c>null</c>, then the beatmap ruleset will not be populated.</item>
        /// <item>The online beatmap ID is populated in all cases.</item>
        /// </list>
        /// </param>
        /// <returns>The populated <see cref="ScoreInfo"/>.</returns>
        public ScoreInfo ToScoreInfo(Mod[] mods, IBeatmapInfo? beatmap = null)
        {
            var score = new ScoreInfo
            {
                OnlineID = OnlineID,
                LegacyOnlineID = (long?)LegacyScoreId ?? -1,
                IsLegacyScore = IsLegacyScore,
                User = User ?? new APIUser { Id = UserID },
                BeatmapInfo = new BeatmapInfo { OnlineID = BeatmapID },
                Ruleset = new RulesetInfo { OnlineID = RulesetID },
                Passed = Passed,
                TotalScore = TotalScore,
                TotalScoreWithoutMods = TotalScoreWithoutMods,
                LegacyTotalScore = LegacyTotalScore,
                Accuracy = Accuracy,
                MaxCombo = MaxCombo,
                Rank = Rank,
                Statistics = Statistics,
                MaximumStatistics = MaximumStatistics,
                Date = EndedAt,
                HasOnlineReplay = HasReplay,
                Mods = mods,
                PP = PP,
                Ranked = Ranked,
            };

            if (beatmap is BeatmapInfo realmBeatmap)
                score.BeatmapInfo = realmBeatmap;
            else if (beatmap != null)
            {
                score.BeatmapInfo.Ruleset.OnlineID = beatmap.Ruleset.OnlineID;
                score.BeatmapInfo.Ruleset.Name = beatmap.Ruleset.Name;
                score.BeatmapInfo.Ruleset.ShortName = beatmap.Ruleset.ShortName;
            }

            return score;
        }

        /// <summary>
        /// Creates a <see cref="SoloScoreInfo"/> from a local score for score submission.
        /// </summary>
        /// <param name="score">The local score.</param>
        public static SoloScoreInfo ForSubmission(ScoreInfo score) => new SoloScoreInfo
        {
            Rank = score.Rank,
            TotalScore = score.TotalScore,
            TotalScoreWithoutMods = score.TotalScoreWithoutMods,
            Accuracy = score.Accuracy,
            PP = score.PP,
            MaxCombo = score.MaxCombo,
            RulesetID = score.RulesetID,
            Passed = score.Passed,
            Mods = score.APIMods,
            Statistics = score.Statistics.Where(kvp => kvp.Value != 0).ToDictionary(),
            MaximumStatistics = score.MaximumStatistics.Where(kvp => kvp.Value != 0).ToDictionary(),
        };
    }
}
