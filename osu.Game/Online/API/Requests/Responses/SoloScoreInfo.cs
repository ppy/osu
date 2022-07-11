// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [Serializable]
    public class SoloScoreInfo : IHasOnlineID<long>
    {
        [JsonIgnore]
        public long id { get; set; }

        public int user_id { get; set; }

        public int beatmap_id { get; set; }

        public int ruleset_id { get; set; }

        public int? build_id { get; set; }

        public bool passed { get; set; }

        public int total_score { get; set; }

        public double accuracy { get; set; }

        public APIUser user { get; set; }

        // TODO: probably want to update this column to match user stats (short)?
        public int max_combo { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public ScoreRank rank { get; set; }

        public DateTimeOffset? started_at { get; set; }

        public DateTimeOffset? ended_at { get; set; }

        public List<APIMod> mods { get; set; } = new List<APIMod>();

        [JsonProperty("statistics")]
        public Dictionary<HitResult, int> Statistics { get; set; } = new Dictionary<HitResult, int>();

        public override string ToString() => $"score_id: {id} user_id: {user_id}";

        [JsonIgnore]
        public DateTimeOffset created_at { get; set; }

        [JsonIgnore]
        public DateTimeOffset updated_at { get; set; }

        [JsonIgnore]
        public DateTimeOffset? deleted_at { get; set; }

        /// <summary>
        /// Create a <see cref="ScoreInfo"/> from an API score instance.
        /// </summary>
        /// <param name="rulesets">A ruleset store, used to populate a ruleset instance in the returned score.</param>
        /// <param name="beatmap">An optional beatmap, copied into the returned score (for cases where the API does not populate the beatmap).</param>
        /// <returns></returns>
        public ScoreInfo CreateScoreInfo(RulesetStore rulesets, BeatmapInfo beatmap = null)
        {
            var ruleset = rulesets.GetRuleset(ruleset_id) ?? throw new InvalidOperationException($"Ruleset with ID of {ruleset_id} not found locally");

            var rulesetInstance = ruleset.CreateInstance();

            var modInstances = mods.Select(apiMod => rulesetInstance.CreateModFromAcronym(apiMod.Acronym)).Where(m => m != null).ToArray();

            // all API scores provided by this class are considered to be legacy.
            modInstances = modInstances.Append(rulesetInstance.CreateMod<ModClassic>()).ToArray();

            var scoreInfo = new ScoreInfo
            {
                User = user ?? new APIUser { Id = user_id },
                BeatmapInfo = beatmap ?? new BeatmapInfo { OnlineID = beatmap_id },
                Passed = passed,
                TotalScore = total_score,
                Accuracy = accuracy,
                MaxCombo = max_combo,
                Rank = rank,
                Statistics = Statistics,
                OnlineID = OnlineID,
                Date = ended_at ?? DateTimeOffset.Now,
                // PP =
                Hash = "online", // TODO: temporary?
                Ruleset = ruleset,
                Mods = modInstances,
            };

            return scoreInfo;
        }

        public ScoreInfo CreateScoreInfo(Mod[] mods) => new ScoreInfo
        {
            OnlineID = id,
            User = new APIUser { Id = user_id },
            BeatmapInfo = new BeatmapInfo { OnlineID = beatmap_id },
            Ruleset = new RulesetInfo { OnlineID = ruleset_id },
            Passed = passed,
            TotalScore = total_score,
            Accuracy = accuracy,
            MaxCombo = max_combo,
            Rank = rank,
            Mods = mods,
            Statistics = Statistics
        };

        public long OnlineID => id;
    }
}
