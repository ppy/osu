// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;

namespace osu.Game.Online.API.Requests.Responses
{
    [Serializable]
    public class SoloScoreInfo : IHasOnlineID<long>
    {
        [JsonProperty("replay")]
        public bool HasReplay { get; set; }

        [JsonProperty("beatmap_id")]
        public int BeatmapID { get; set; }

        [JsonProperty("ruleset_id")]
        public int RulesetID { get; set; }

        [JsonProperty("build_id")]
        public int? BuildID { get; set; }

        [JsonProperty("passed")]
        public bool Passed { get; set; }

        [JsonProperty("total_score")]
        public int TotalScore { get; set; }

        [JsonProperty("accuracy")]
        public double Accuracy { get; set; }

        [JsonProperty("user_id")]
        public int UserID { get; set; }

        // TODO: probably want to update this column to match user stats (short)?
        [JsonProperty("max_combo")]
        public int MaxCombo { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("rank")]
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

        #region osu-web API additions (not stored to database).

        [JsonProperty("id")]
        public long? ID { get; set; }

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

        #endregion

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

            var scoreInfo = ToScoreInfo(mods);

            scoreInfo.Ruleset = ruleset;
            if (beatmap != null) scoreInfo.BeatmapInfo = beatmap;

            return scoreInfo;
        }

        /// <summary>
        /// Create a <see cref="ScoreInfo"/> from an API score instance.
        /// </summary>
        /// <param name="mods">The mod instances, resolved from a ruleset.</param>
        /// <returns></returns>
        public ScoreInfo ToScoreInfo(Mod[] mods) => new ScoreInfo
        {
            OnlineID = OnlineID,
            User = User ?? new APIUser { Id = UserID },
            BeatmapInfo = new BeatmapInfo { OnlineID = BeatmapID },
            Ruleset = new RulesetInfo { OnlineID = RulesetID },
            Passed = Passed,
            TotalScore = TotalScore,
            Accuracy = Accuracy,
            MaxCombo = MaxCombo,
            Rank = Rank,
            Statistics = Statistics,
            Date = EndedAt,
            Hash = HasReplay ? "online" : string.Empty, // TODO: temporary?
            Mods = mods,
            PP = PP,
        };

        public long OnlineID => ID ?? -1;
    }
}
