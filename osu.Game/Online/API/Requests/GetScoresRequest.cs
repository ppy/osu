// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
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
using osu.Game.Screens.Select.Leaderboards;
using osu.Framework.IO.Network;

namespace osu.Game.Online.API.Requests
{
    public class GetScoresRequest : APIRequest<GetScoresResponse>
    {
        private readonly BeatmapInfo beatmap;
        private readonly LeaderboardScope scope;
        private readonly RulesetInfo ruleset;

        public GetScoresRequest(BeatmapInfo beatmap, RulesetInfo ruleset, LeaderboardScope scope = LeaderboardScope.Global)
        {
            if (!beatmap.OnlineBeatmapID.HasValue)
                throw new InvalidOperationException($"Cannot lookup a beatmap's scores without having a populated {nameof(BeatmapInfo.OnlineBeatmapID)}.");

            if (scope == LeaderboardScope.Local)
                throw new InvalidOperationException("Should not attempt to request online scores for a local scoped leaderboard");

            this.beatmap = beatmap;
            this.scope = scope;
            this.ruleset = ruleset ?? throw new ArgumentNullException(nameof(ruleset));

            Success += onSuccess;
        }

        private void onSuccess(GetScoresResponse r)
        {
            foreach (OnlineScore score in r.Scores)
            {
                // Process scores after retrieving data from server, ensuring the ruleset is assigned
                foreach (var kvp in score.JsonStats)
                {
                    HitResult newKey;
                    switch (kvp.Key)
                    {
                        case @"count_geki":
                            newKey = ruleset.ID == 3 ? HitResult.Perfect : HitResult.Great; // Currently only accounting for osu!mania; will need evaluation for osu!catch
                            break;
                        case @"count_300":
                            newKey = HitResult.Great;
                            break;
                        case @"count_katu":
                            newKey = HitResult.Good;
                            break;
                        case @"count_100":
                            newKey = ruleset.ID == 3 ? HitResult.Ok : HitResult.Good;
                            break;
                        case @"count_50":
                            newKey = ruleset.ID != 1 ? HitResult.Meh : HitResult.Miss; // Do not create a new key if score is about osu!taiko
                            break;
                        case @"count_miss":
                            newKey = HitResult.Miss;
                            break;
                        default:
                            continue;
                    }

                    if (!score.Statistics.ContainsKey(newKey))
                        score.Statistics.Add(newKey, kvp.Value);
                    else
                        score.Statistics[newKey] = (long)score.Statistics[newKey] + (long)kvp.Value;
                }
                score.ApplyBeatmap(beatmap);
            }
        }

        protected override WebRequest CreateWebRequest()
        {
            var req = base.CreateWebRequest();

            req.Timeout = 30000;
            req.AddParameter(@"type", scope.ToString().ToLowerInvariant());
            req.AddParameter(@"mode", ruleset.ShortName);

            return req;
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
        public Dictionary<string, object> JsonStats { get; set; }

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
