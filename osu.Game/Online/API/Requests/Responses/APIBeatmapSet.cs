// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;

namespace osu.Game.Online.API.Requests.Responses
{
    public class APIBeatmapSet : BeatmapMetadata // todo: this is a bit wrong...
    {
        [JsonProperty(@"covers")]
        private BeatmapSetOnlineCovers covers { get; set; }

        private int? onlineBeatmapSetID;

        [JsonProperty(@"id")]
        public int? OnlineBeatmapSetID
        {
            get => onlineBeatmapSetID;
            set => onlineBeatmapSetID = value > 0 ? value : null;
        }

        [JsonProperty(@"status")]
        public BeatmapSetOnlineStatus Status { get; set; }

        [JsonProperty(@"preview_url")]
        private string preview { get; set; }

        [JsonProperty(@"has_favourited")]
        private bool hasFavourited { get; set; }

        [JsonProperty(@"play_count")]
        private int playCount { get; set; }

        [JsonProperty(@"favourite_count")]
        private int favouriteCount { get; set; }

        [JsonProperty(@"bpm")]
        private double bpm { get; set; }

        [JsonProperty(@"video")]
        private bool hasVideo { get; set; }

        [JsonProperty(@"storyboard")]
        private bool hasStoryboard { get; set; }

        [JsonProperty(@"submitted_date")]
        private DateTimeOffset submitted { get; set; }

        [JsonProperty(@"ranked_date")]
        private DateTimeOffset? ranked { get; set; }

        [JsonProperty(@"last_updated")]
        private DateTimeOffset lastUpdated { get; set; }

        [JsonProperty(@"ratings")]
        private int[] ratings { get; set; }

        [JsonProperty(@"user_id")]
        private long creatorId
        {
            set => Author.Id = value;
        }

        [JsonProperty(@"availability")]
        private BeatmapSetOnlineAvailability availability { get; set; }

        [JsonProperty(@"genre")]
        private BeatmapSetOnlineGenre genre { get; set; }

        [JsonProperty(@"language")]
        private BeatmapSetOnlineLanguage language { get; set; }

        [JsonProperty(@"beatmaps")]
        private IEnumerable<APIBeatmap> beatmaps { get; set; }

        public BeatmapSetInfo ToBeatmapSet(RulesetStore rulesets)
        {
            return new BeatmapSetInfo
            {
                OnlineBeatmapSetID = OnlineBeatmapSetID,
                Metadata = this,
                Status = Status,
                Metrics = ratings == null ? null : new BeatmapSetMetrics { Ratings = ratings },
                OnlineInfo = new BeatmapSetOnlineInfo
                {
                    Covers = covers,
                    Preview = preview,
                    PlayCount = playCount,
                    FavouriteCount = favouriteCount,
                    BPM = bpm,
                    Status = Status,
                    HasVideo = hasVideo,
                    HasStoryboard = hasStoryboard,
                    Submitted = submitted,
                    Ranked = ranked,
                    LastUpdated = lastUpdated,
                    Availability = availability,
                    HasFavourited = hasFavourited,
                    Genre = genre,
                    Language = language
                },
                Beatmaps = beatmaps?.Select(b => b.ToBeatmap(rulesets)).ToList(),
            };
        }
    }
}
