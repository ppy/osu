// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Users;
using osu.Game.Rulesets.Replays;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Online.API.Requests
{
    public class GetScoresRequest : APIRequest<GetScoresResponse>
    {
        private readonly BeatmapInfo beatmap;

        public GetScoresRequest(BeatmapInfo beatmap)
        {
            if (!beatmap.OnlineBeatmapID.HasValue)
                throw new InvalidOperationException($"Cannot lookup a beatmap's scores without having a populated {nameof(BeatmapInfo.OnlineBeatmapID)}.");

            this.beatmap = beatmap;

            Success += onSuccess;
        }

        private void onSuccess(GetScoresResponse r)
        {
            foreach (OnlineScore score in r.Scores)
                score.ApplyBeatmap(beatmap);
        }

        protected override string Target => $@"beatmaps/{beatmap.OnlineBeatmapID}/scores";
    }

    public class GetScoresResponse
    {
        [JsonProperty(@"scores")]
        public IEnumerable<OnlineScore> Scores;
    }

    public class OnlineScore : Score
    {
        [JsonProperty(@"score")]
        private double totalScore
        {
            set { TotalScore = value; }
        }

        [JsonProperty(@"max_combo")]
        private int maxCombo
        {
            set { MaxCombo = value; }
        }

        [JsonProperty(@"user")]
        private User user
        {
            set { User = value; }
        }

        [JsonProperty(@"replay_data")]
        private Replay replay
        {
            set { Replay = value; }
        }

        [JsonProperty(@"mode_int")]
        public int OnlineRulesetID { get; set; }

        [JsonProperty(@"score_id")]
        private long onlineScoreID
        {
            set { OnlineScoreID = value; }
        }

        [JsonProperty(@"created_at")]
        private DateTimeOffset date
        {
            set { Date = value; }
        }

        [JsonProperty(@"beatmap")]
        private BeatmapInfo beatmap
        {
            set { Beatmap = value; }
        }

        [JsonProperty(@"beatmapset")]
        private BeatmapMetadata metadata
        {
            set { Beatmap.Metadata = value; }
        }

        [JsonProperty(@"statistics")]
        private Dictionary<string, object> jsonStats
        {
            set
            {
                foreach (var kvp in value)
                {
                    string key = kvp.Key;
                    switch (key)
                    {
                        case @"count_300":
                            key = @"300";
                            break;
                        case @"count_100":
                            key = @"100";
                            break;
                        case @"count_50":
                            key = @"50";
                            break;
                        case @"count_miss":
                            key = @"x";
                            break;
                        default:
                            continue;
                    }

                    Statistics.Add(key, kvp.Value);
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
