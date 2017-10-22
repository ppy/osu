// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;

namespace osu.Game.Online.API.Requests
{
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

        [JsonProperty(@"user_id")]
        private long creatorId {
            set { Author.Id = value; }
        }

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
