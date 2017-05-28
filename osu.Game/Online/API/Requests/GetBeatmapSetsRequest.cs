// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using osu.Game.Database;

namespace osu.Game.Online.API.Requests
{
    public class GetBeatmapSetsRequest : APIRequest<IEnumerable<GetBeatmapSetsResponse>>
    {
        private readonly string query;
        private readonly RankStatus rankStatus;

        public GetBeatmapSetsRequest(string query, RankStatus rankStatus = RankStatus.Any)
        {
            this.query = System.Uri.EscapeDataString(query);
            this.rankStatus = rankStatus;
        }

        protected override string Target => $@"beatmapsets/search?q={query}&s={(int)rankStatus}";
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

        [JsonProperty(@"beatmaps")]
        private IEnumerable<GetBeatmapSetsBeatmapResponse> beatmaps { get; set; }

        public BeatmapSetInfo ToSetInfo(RulesetDatabase rulesets)
        {
            return new BeatmapSetInfo
            {
                Metadata = this,
                OnlineInfo = new BeatmapSetOnlineInfo
                {
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

            public BeatmapInfo ToBeatmap(RulesetDatabase rulesets)
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
