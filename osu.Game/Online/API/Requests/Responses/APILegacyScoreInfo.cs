// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Scoring.Legacy;
using osu.Game.Users;

namespace osu.Game.Online.API.Requests.Responses
{
    public class APILegacyScoreInfo : LegacyScoreInfo
    {
        [JsonProperty(@"score")]
        private int totalScore
        {
            set => TotalScore = value;
        }

        [JsonProperty(@"max_combo")]
        private int maxCombo
        {
            set => MaxCombo = value;
        }

        [JsonProperty(@"user")]
        private User user
        {
            set => User = value;
        }

        [JsonProperty(@"id")]
        private long onlineScoreID
        {
            set => OnlineScoreID = value;
        }

        [JsonProperty(@"replay")]
        public bool Replay { get; set; }

        [JsonProperty(@"created_at")]
        private DateTimeOffset date
        {
            set => Date = value;
        }

        [JsonProperty(@"beatmap")]
        private BeatmapInfo beatmap
        {
            set => Beatmap = value;
        }

        [JsonProperty(@"beatmapset")]
        private BeatmapMetadata metadata
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
        private Dictionary<string, int> jsonStats
        {
            set
            {
                foreach (var kvp in value)
                {
                    switch (kvp.Key)
                    {
                        case @"count_geki":
                            CountGeki = kvp.Value;
                            break;

                        case @"count_300":
                            Count300 = kvp.Value;
                            break;

                        case @"count_katu":
                            CountKatu = kvp.Value;
                            break;

                        case @"count_100":
                            Count100 = kvp.Value;
                            break;

                        case @"count_50":
                            Count50 = kvp.Value;
                            break;

                        case @"count_miss":
                            CountMiss = kvp.Value;
                            break;

                        default:
                            continue;
                    }
                }
            }
        }

        [JsonProperty(@"mode_int")]
        public int OnlineRulesetID
        {
            get => RulesetID;
            set => RulesetID = value;
        }

        [JsonProperty(@"mods")]
        private string[] modStrings { get; set; }

        public override RulesetInfo Ruleset
        {
            get => base.Ruleset;
            set
            {
                base.Ruleset = value;

                if (modStrings != null)
                {
                    // Evaluate the mod string
                    Mods = Ruleset.CreateInstance().GetAllMods().Where(mod => modStrings.Contains(mod.Acronym)).ToArray();
                }
            }
        }
    }
}
