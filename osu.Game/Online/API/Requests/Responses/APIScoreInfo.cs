// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Scoring;
using osu.Game.Scoring.Legacy;

namespace osu.Game.Online.API.Requests.Responses
{
    public class APIScoreInfo : IScoreInfo
    {
        [JsonProperty(@"score")]
        public long TotalScore { get; set; }

        [JsonProperty(@"max_combo")]
        public int MaxCombo { get; set; }

        [JsonProperty(@"user")]
        public APIUser User { get; set; }

        [JsonProperty(@"id")]
        public long OnlineID { get; set; }

        [JsonProperty(@"replay")]
        public bool HasReplay { get; set; }

        [JsonProperty(@"created_at")]
        public DateTimeOffset Date { get; set; }

        [JsonProperty(@"beatmap")]
        [CanBeNull]
        public APIBeatmap Beatmap { get; set; }

        [JsonProperty("accuracy")]
        public double Accuracy { get; set; }

        [JsonProperty(@"pp")]
        public double? PP { get; set; }

        [JsonProperty(@"beatmapset")]
        [CanBeNull]
        public APIBeatmapSet BeatmapSet
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

        [JsonProperty("statistics")]
        public Dictionary<string, int> Statistics { get; set; }

        [JsonProperty(@"mode_int")]
        public int RulesetID { get; set; }

        [JsonProperty(@"mods")]
        private string[] mods { set => Mods = value.Select(acronym => new APIMod { Acronym = acronym }); }

        [NotNull]
        public IEnumerable<APIMod> Mods { get; set; } = Array.Empty<APIMod>();

        [JsonProperty("rank")]
        [JsonConverter(typeof(StringEnumConverter))]
        public ScoreRank Rank { get; set; }

        /// <summary>
        /// Create a <see cref="ScoreInfo"/> from an API score instance.
        /// </summary>
        /// <param name="rulesets">A ruleset store, used to populate a ruleset instance in the returned score.</param>
        /// <param name="beatmap">An optional beatmap, copied into the returned score (for cases where the API does not populate the beatmap).</param>
        /// <returns></returns>
        public ScoreInfo CreateScoreInfo(RulesetStore rulesets, BeatmapInfo beatmap = null)
        {
            var ruleset = rulesets.GetRuleset(RulesetID);

            var rulesetInstance = ruleset.CreateInstance();

            var modInstances = Mods.Select(apiMod => rulesetInstance.CreateModFromAcronym(apiMod.Acronym)).Where(m => m != null).ToArray();

            // all API scores provided by this class are considered to be legacy.
            modInstances = modInstances.Append(rulesetInstance.CreateMod<ModClassic>()).ToArray();

            var scoreInfo = new ScoreInfo
            {
                TotalScore = TotalScore,
                MaxCombo = MaxCombo,
                BeatmapInfo = beatmap,
                User = User,
                Accuracy = Accuracy,
                OnlineScoreID = OnlineID,
                Date = Date,
                PP = PP,
                RulesetID = RulesetID,
                Hash = HasReplay ? "online" : string.Empty, // todo: temporary?
                Rank = Rank,
                Ruleset = ruleset,
                Mods = modInstances,
            };

            if (Statistics != null)
            {
                foreach (var kvp in Statistics)
                {
                    switch (kvp.Key)
                    {
                        case @"count_geki":
                            scoreInfo.SetCountGeki(kvp.Value);
                            break;

                        case @"count_300":
                            scoreInfo.SetCount300(kvp.Value);
                            break;

                        case @"count_katu":
                            scoreInfo.SetCountKatu(kvp.Value);
                            break;

                        case @"count_100":
                            scoreInfo.SetCount100(kvp.Value);
                            break;

                        case @"count_50":
                            scoreInfo.SetCount50(kvp.Value);
                            break;

                        case @"count_miss":
                            scoreInfo.SetCountMiss(kvp.Value);
                            break;
                    }
                }
            }

            return scoreInfo;
        }

        public IRulesetInfo Ruleset => new RulesetInfo { ID = RulesetID };

        IBeatmapInfo IScoreInfo.Beatmap => Beatmap;
    }
}
