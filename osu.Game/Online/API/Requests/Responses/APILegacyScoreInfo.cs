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
using osu.Game.Scoring;
using osu.Game.Scoring.Legacy;
using osu.Game.Users;

namespace osu.Game.Online.API.Requests.Responses
{
    public class APILegacyScoreInfo
    {
        public ScoreInfo CreateScoreInfo(RulesetStore rulesets)
        {
            var ruleset = rulesets.GetRuleset(OnlineRulesetID);

            var mods = Mods != null ? ruleset.CreateInstance().GetAllMods().Where(mod => Mods.Contains(mod.Acronym)).ToArray() : Array.Empty<Mod>();

            var scoreInfo = new ScoreInfo
            {
                TotalScore = TotalScore,
                MaxCombo = MaxCombo,
                User = User,
                Accuracy = Accuracy,
                OnlineScoreID = OnlineScoreID,
                Date = Date,
                PP = PP,
                Beatmap = Beatmap,
                RulesetID = OnlineRulesetID,
                Hash = Replay ? "online" : string.Empty, // todo: temporary?
                Rank = Rank,
                Ruleset = ruleset,
                Mods = mods,
                IsLegacyScore = true
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

        [JsonProperty(@"score")]
        public int TotalScore { get; set; }

        [JsonProperty(@"max_combo")]
        public int MaxCombo { get; set; }

        [JsonProperty(@"user")]
        public User User { get; set; }

        [JsonProperty(@"id")]
        public long OnlineScoreID { get; set; }

        [JsonProperty(@"replay")]
        public bool Replay { get; set; }

        [JsonProperty(@"created_at")]
        public DateTimeOffset Date { get; set; }

        [JsonProperty(@"beatmap")]
        public BeatmapInfo Beatmap { get; set; }

        [JsonProperty("accuracy")]
        public double Accuracy { get; set; }

        [JsonProperty(@"pp")]
        public double? PP { get; set; }

        [JsonProperty(@"beatmapset")]
        public BeatmapMetadata Metadata
        {
            set
            {
                // extract the set ID to its correct place.
                Beatmap.BeatmapSet = new BeatmapSetInfo { OnlineBeatmapSetID = value.ID };
                value.ID = 0;

                Beatmap.Metadata = value;
            }
        }

        [JsonProperty(@"statistics")]
        public Dictionary<string, int> Statistics { get; set; }

        [JsonProperty(@"mode_int")]
        public int OnlineRulesetID { get; set; }

        [JsonProperty(@"mods")]
        public string[] Mods { get; set; }

        [JsonProperty("rank")]
        [JsonConverter(typeof(StringEnumConverter))]
        public ScoreRank Rank { get; set; }
    }
}
