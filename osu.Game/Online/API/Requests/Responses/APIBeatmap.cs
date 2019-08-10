// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using Newtonsoft.Json;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;

namespace osu.Game.Online.API.Requests.Responses
{
    public class APIBeatmap : BeatmapMetadata
    {
        [JsonProperty(@"id")]
        public int OnlineBeatmapID { get; set; }

        [JsonProperty(@"beatmapset_id")]
        public int OnlineBeatmapSetID { get; set; }

        [JsonProperty(@"status")]
        public BeatmapSetOnlineStatus Status { get; set; }

        [JsonProperty(@"beatmapset")]
        public APIBeatmapSet BeatmapSet { get; set; }

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

        [JsonProperty(@"failtimes")]
        private BeatmapMetrics metrics { get; set; }

        public BeatmapInfo ToBeatmap(RulesetStore rulesets)
        {
            var set = BeatmapSet?.ToBeatmapSet(rulesets);

            return new BeatmapInfo
            {
                Metadata = set?.Metadata ?? this,
                Ruleset = rulesets.GetRuleset(ruleset),
                StarDifficulty = starDifficulty,
                OnlineBeatmapID = OnlineBeatmapID,
                Version = version,
                Length = TimeSpan.FromSeconds(length).TotalMilliseconds,
                Status = Status,
                BeatmapSet = set,
                Metrics = metrics,
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
                    CircleCount = circleCount,
                    SliderCount = sliderCount,
                },
            };
        }
    }
}
