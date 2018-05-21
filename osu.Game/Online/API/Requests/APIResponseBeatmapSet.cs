// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using System;

namespace osu.Game.Online.API.Requests
{
    public class APIResponseBeatmapSet : BeatmapMetadata // todo: this is a bit wrong...
    {
        [JsonProperty(@"covers")]
        private BeatmapSetOnlineCovers covers { get; set; }

        [JsonProperty(@"preview_url")]
        private string preview { get; set; }

        [JsonProperty(@"play_count")]
        private int playCount { get; set; }

        [JsonProperty(@"favourite_count")]
        private int favouriteCount { get; set; }

        [JsonProperty(@"bpm")]
        private double bpm { get; set; }

        [JsonProperty(@"video")]
        private bool hasVideo { get; set; }

        [JsonProperty(@"status")]
        private BeatmapSetOnlineStatus status { get; set; }

        [JsonProperty(@"submitted_date")]
        private DateTimeOffset submitted { get; set; }

        [JsonProperty(@"ranked_date")]
        private DateTimeOffset ranked { get; set; }

        [JsonProperty(@"last_updated")]
        private DateTimeOffset lastUpdated { get; set; }

        [JsonProperty(@"user_id")]
        private long creatorId {
            set { Author.Id = value; }
        }

        [JsonProperty(@"beatmaps")]
        private IEnumerable<APIResponseBeatmap> beatmaps { get; set; }

        public BeatmapSetInfo ToBeatmapSet(RulesetStore rulesets)
        {
            return new BeatmapSetInfo
            {
                OnlineBeatmapSetID = OnlineBeatmapSetID,
                Metadata = this,
                OnlineInfo = new BeatmapSetOnlineInfo
                {
                    Covers = covers,
                    Preview = preview,
                    PlayCount = playCount,
                    FavouriteCount = favouriteCount,
                    BPM = bpm,
                    Status = status,
                    HasVideo = hasVideo,
                    Submitted = submitted,
                    Ranked = ranked,
                    LastUpdated = lastUpdated,
                },
                Beatmaps = beatmaps?.Select(b => b.ToBeatmap(rulesets)).ToList(),
            };
        }

        private class APIResponseBeatmap : BeatmapMetadata
        {
            [JsonProperty(@"id")]
            private int onlineBeatmapID { get; set; }

            [JsonProperty(@"playcount")]
            private int playCount { get; set; }

            [JsonProperty(@"passcount")]
            private int passCount { get; set; }

            [JsonProperty(@"mode_int")]
            private int ruleset { get; set; }

            [JsonProperty(@"difficulty_rating")]
            private double starDifficulty { get; set; }

            [JsonProperty(@"drain")]
            private float drainRate { get; set; }

            [JsonProperty(@"cs")]
            private float circleSize { get; set; }

            [JsonProperty(@"ar")]
            private float approachRate { get; set; }

            [JsonProperty(@"accuracy")]
            private float overallDifficulty { get; set; }

            [JsonProperty(@"total_length")]
            private double length { get; set; }

            [JsonProperty(@"count_circles")]
            private int circleCount { get; set; }

            [JsonProperty(@"count_sliders")]
            private int sliderCount { get; set; }

            [JsonProperty(@"version")]
            private string version { get; set; }

            public BeatmapInfo ToBeatmap(RulesetStore rulesets)
            {
                return new BeatmapInfo
                {
                    Metadata = this,
                    Ruleset = rulesets.GetRuleset(ruleset),
                    StarDifficulty = starDifficulty,
                    OnlineBeatmapID = onlineBeatmapID,
                    Version = version,
                    BaseDifficulty = new BeatmapDifficulty
                    {
                        DrainRate = drainRate,
                        CircleSize = circleSize,
                        ApproachRate = approachRate,
                        OverallDifficulty = overallDifficulty,
                    },
                    OnlineInfo = new BeatmapOnlineInfo
                    {
                        PlayCount = playCount,
                        PassCount = passCount,
                        Length = length,
                        CircleCount = circleCount,
                        SliderCount = sliderCount,
                    },
                };
            }
        }
    }
}
