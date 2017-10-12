// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using osu.Game.Beatmaps;
using osu.Game.Overlays;
using osu.Game.Overlays.Direct;
using osu.Game.Rulesets;
using osu.Game.Users;

namespace osu.Game.Online.API.Requests
{
    public class GetBeatmapSetsRequest : APIRequest<IEnumerable<GetBeatmapSetsResponse>>
    {
        private readonly string query;
        private readonly RulesetInfo ruleset;
        private readonly RankStatus rankStatus;
        private readonly DirectSortCriteria sortCriteria;
        private readonly SortDirection direction;
        private string directionString => direction == SortDirection.Descending ? @"desc" : @"asc";

        public GetBeatmapSetsRequest(string query, RulesetInfo ruleset, RankStatus rankStatus = RankStatus.Any, DirectSortCriteria sortCriteria = DirectSortCriteria.Ranked, SortDirection direction = SortDirection.Descending)
        {
            this.query = System.Uri.EscapeDataString(query);
            this.ruleset = ruleset;
            this.rankStatus = rankStatus;
            this.sortCriteria = sortCriteria;
            this.direction = direction;
        }

        protected override string Target => $@"beatmapsets/search?q={query}&m={ruleset.ID ?? 0}&s={(int)rankStatus}&sort={sortCriteria.ToString().ToLower()}_{directionString}";
    }

    public class GetBeatmapSetsResponse : BeatmapMetadata
    {
        [JsonProperty(@"covers")]
        private BeatmapSetOnlineCovers covers { get; set; }

        [JsonProperty(@"previewUrl")]
        private string preview { get; set; }

        [JsonProperty(@"play_count")]
        private int playCount { get; set; }

        [JsonProperty(@"favourite_count")]
        private int favouriteCount { get; set; }

        [JsonProperty(@"id")]
        private int onlineId { get; set; }

        [JsonProperty(@"creator")]
        private string creatorUsername { get; set; }

        [JsonProperty(@"user_id")]
        private long creatorId = 1;

        [JsonProperty(@"beatmaps")]
        private IEnumerable<GetBeatmapSetsBeatmapResponse> beatmaps { get; set; }

        public BeatmapSetInfo ToBeatmapSet(RulesetStore rulesets)
        {
            return new BeatmapSetInfo
            {
                OnlineBeatmapSetID = onlineId,
                Metadata = this,
                OnlineInfo = new BeatmapSetOnlineInfo
                {
                    Author = new User
                    {
                        Id = creatorId,
                        Username = creatorUsername,
                    },
                    Covers = covers,
                    Preview = preview,
                    PlayCount = playCount,
                    FavouriteCount = favouriteCount,
                },
                Beatmaps = beatmaps.Select(b => b.ToBeatmap(rulesets)).ToList(),
            };
        }

        private class GetBeatmapSetsBeatmapResponse : BeatmapMetadata
        {
            [JsonProperty(@"playcount")]
            private int playCount { get; set; }

            [JsonProperty(@"passcount")]
            private int passCount { get; set; }

            [JsonProperty(@"mode_int")]
            private int ruleset { get; set; }

            [JsonProperty(@"difficulty_rating")]
            private double starDifficulty { get; set; }

            public BeatmapInfo ToBeatmap(RulesetStore rulesets)
            {
                return new BeatmapInfo
                {
                    Metadata = this,
                    Ruleset = rulesets.GetRuleset(ruleset),
                    StarDifficulty = starDifficulty,
                    OnlineInfo = new BeatmapOnlineInfo
                    {
                        PlayCount = playCount,
                        PassCount = passCount,
                    },
                };
            }
        }
    }
}
