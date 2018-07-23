// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Replays;
using osu.Game.Rulesets.Scoring;
using osu.Game.Users;

namespace osu.Game.Online.API.Requests.Responses
{
    public class APIScore : Score
    {
        [JsonProperty(@"score")]
        private double totalScore
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

        [JsonProperty(@"replay_data")]
        private Replay replay
        {
            set => Replay = value;
        }

        [JsonProperty(@"mode_int")]
        public int OnlineRulesetID { get; set; }

        [JsonProperty(@"score_id")]
        private long onlineScoreID
        {
            set => OnlineScoreID = value;
        }

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
        private Dictionary<string, object> jsonStats
        {
            set
            {
                foreach (var kvp in value)
                {
                    HitResult newKey;
                    switch (kvp.Key)
                    {
                        case @"count_300":
                            newKey = HitResult.Great;
                            break;
                        case @"count_100":
                            newKey = HitResult.Good;
                            break;
                        case @"count_50":
                            newKey = HitResult.Meh;
                            break;
                        case @"count_miss":
                            newKey = HitResult.Miss;
                            break;
                        default:
                            continue;
                    }

                    Statistics.Add(newKey, kvp.Value);
                }
            }
        }

        [JsonProperty(@"mods")]
        private string[] modStrings { get; set; }

        public void ApplyBeatmap(BeatmapInfo beatmap)
        {
            Beatmap = beatmap;
            ApplyRuleset(beatmap.Ruleset);
        }

        public void ApplyRuleset(RulesetInfo ruleset)
        {
            Ruleset = ruleset;

            // Evaluate the mod string
            Mods = Ruleset.CreateInstance().GetAllMods().Where(mod => modStrings.Contains(mod.ShortenedName)).ToArray();
        }
    }
}
