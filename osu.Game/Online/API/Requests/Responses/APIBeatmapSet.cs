// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Rulesets;

#nullable enable

namespace osu.Game.Online.API.Requests.Responses
{
    public class APIBeatmapSet : BeatmapMetadata, IBeatmapSetOnlineInfo, IBeatmapSetInfo
    {
        [JsonProperty(@"covers")]
        public BeatmapSetOnlineCovers Covers { get; set; }

        [JsonProperty(@"id")]
        public int OnlineID { get; set; }

        [JsonProperty(@"status")]
        public BeatmapSetOnlineStatus Status { get; set; }

        [JsonProperty(@"preview_url")]
        public string Preview { get; set; } = string.Empty;

        [JsonProperty(@"has_favourited")]
        public bool HasFavourited { get; set; }

        [JsonProperty(@"play_count")]
        public int PlayCount { get; set; }

        [JsonProperty(@"favourite_count")]
        public int FavouriteCount { get; set; }

        [JsonProperty(@"bpm")]
        public double BPM { get; set; }

        [JsonProperty(@"nsfw")]
        public bool HasExplicitContent { get; set; }

        [JsonProperty(@"video")]
        public bool HasVideo { get; set; }

        [JsonProperty(@"storyboard")]
        public bool HasStoryboard { get; set; }

        [JsonProperty(@"submitted_date")]
        public DateTimeOffset Submitted { get; set; }

        [JsonProperty(@"ranked_date")]
        public DateTimeOffset? Ranked { get; set; }

        [JsonProperty(@"last_updated")]
        public DateTimeOffset? LastUpdated { get; set; }

        [JsonProperty(@"ratings")]
        private int[] ratings { get; set; } = Array.Empty<int>();

        [JsonProperty(@"track_id")]
        public int? TrackId { get; set; }

        [JsonProperty(@"user_id")]
        private int creatorId
        {
            set => Author.Id = value;
        }

        [JsonProperty(@"availability")]
        public BeatmapSetOnlineAvailability Availability { get; set; }

        [JsonProperty(@"genre")]
        public BeatmapSetOnlineGenre Genre { get; set; }

        [JsonProperty(@"language")]
        public BeatmapSetOnlineLanguage Language { get; set; }

        [JsonProperty(@"beatmaps")]
        private IEnumerable<APIBeatmap> beatmaps { get; set; } = Array.Empty<APIBeatmap>();

        public virtual BeatmapSetInfo ToBeatmapSet(RulesetStore rulesets)
        {
            var beatmapSet = new BeatmapSetInfo
            {
                OnlineBeatmapSetID = OnlineID,
                Metadata = this,
                Status = Status,
                Metrics = new BeatmapSetMetrics { Ratings = ratings },
                OnlineInfo = this
            };

            beatmapSet.Beatmaps = beatmaps.Select(b =>
            {
                var beatmap = b.ToBeatmapInfo(rulesets);
                beatmap.BeatmapSet = beatmapSet;
                beatmap.Metadata = beatmapSet.Metadata;
                return beatmap;
            }).ToList();

            return beatmapSet;
        }

        #region Implementation of IBeatmapSetInfo

        IEnumerable<IBeatmapInfo> IBeatmapSetInfo.Beatmaps => beatmaps;

        IBeatmapMetadataInfo IBeatmapSetInfo.Metadata => this;

        DateTimeOffset IBeatmapSetInfo.DateAdded => throw new NotImplementedException();
        IEnumerable<INamedFileUsage> IBeatmapSetInfo.Files => throw new NotImplementedException();
        double IBeatmapSetInfo.MaxStarDifficulty => throw new NotImplementedException();
        double IBeatmapSetInfo.MaxLength => throw new NotImplementedException();
        double IBeatmapSetInfo.MaxBPM => throw new NotImplementedException();

        #endregion
    }
}
