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
using osu.Game.Scoring.Legacy;
using osu.Game.Users;

namespace osu.Game.Online.API.Requests.Responses
{
    public class APIScoreInfo : IScoreInfo
    {
        [JsonProperty(@"score")]
        public long TotalScore { get; set; }

        [JsonProperty(@"max_combo")]
        public int MaxCombo { get; set; }

        [JsonProperty(@"user")]
        public User User { get; set; }

        public bool HasReplay { get; set; }

        [JsonProperty(@"id")]
        public long OnlineID { get; set; }

        [JsonProperty(@"replay")]
        public bool Replay { get; set; }

        [JsonProperty(@"created_at")]
        public DateTimeOffset Date { get; set; }

        [JsonProperty(@"beatmap")]
        public APIBeatmap Beatmap { get; set; }

        [JsonProperty("accuracy")]
        public double Accuracy { get; set; }

        [JsonProperty(@"pp")]
        public double? PP { get; set; }

        [JsonProperty(@"beatmapset")]
        public APIBeatmapSet BeatmapSet
        {
            set
            {
                // in the deserialisation case we need to ferry this data across.
                if (Beatmap is APIBeatmap apiBeatmap)
                    apiBeatmap.BeatmapSet = value;
            }
        }

        [JsonProperty("statistics")]
        public Dictionary<string, int> Statistics { get; set; }

        [JsonProperty(@"mode_int")]
        public int OnlineRulesetID { get; set; }

        [JsonProperty(@"mods")]
        public string[] Mods { get; set; }

        [JsonProperty("rank")]
        [JsonConverter(typeof(StringEnumConverter))]
        public ScoreRank Rank { get; set; }

        // TODO: This function will eventually be going away.
        public ScoreInfo CreateScoreInfo(RulesetStore rulesets, BeatmapInfo beatmap = null)
        {
            var ruleset = rulesets.GetRuleset(OnlineRulesetID);

            var rulesetInstance = ruleset.CreateInstance();

            var mods = Mods != null ? Mods.Select(acronym => rulesetInstance.CreateModFromAcronym(acronym)).Where(m => m != null).ToArray() : Array.Empty<Mod>();

            // all API scores provided by this class are considered to be legacy.
            mods = mods.Append(rulesetInstance.CreateMod<ModClassic>()).ToArray();

            var scoreInfo = new ScoreInfo
            {
                TotalScore = TotalScore,
                MaxCombo = MaxCombo,
                User = User,
                Accuracy = Accuracy,
                OnlineScoreID = OnlineID,
                Date = Date,
                PP = PP,
                RulesetID = OnlineRulesetID,
                Hash = Replay ? "online" : string.Empty, // todo: temporary?
                Rank = Rank,
                Ruleset = ruleset,
                Mods = mods,
            };

            if (beatmap != null)
                scoreInfo.BeatmapInfo = beatmap;

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

        public IRulesetInfo Ruleset => new RulesetInfo { ID = OnlineRulesetID };

        IBeatmapInfo IScoreInfo.Beatmap => Beatmap;

        Dictionary<HitResult, int> IScoreInfo.Statistics => new Dictionary<HitResult, int>(); // TODO: implement... maybe. hitresults have weird mappings per ruleset it would seem.
    }
}
