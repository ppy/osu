﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using Newtonsoft.Json;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;

#nullable enable

namespace osu.Game.Online.API.Requests.Responses
{
    public class APIBeatmap : IBeatmapInfo, IBeatmapOnlineInfo
    {
        [JsonProperty(@"id")]
        public int OnlineID { get; set; }

        [JsonProperty(@"beatmapset_id")]
        public int OnlineBeatmapSetID { get; set; }

        [JsonProperty(@"status")]
        public BeatmapSetOnlineStatus Status { get; set; }

        [JsonProperty("checksum")]
        public string Checksum { get; set; } = string.Empty;

        [JsonProperty(@"user_id")]
        public int AuthorID { get; set; }

        [JsonProperty(@"beatmapset")]
        public APIBeatmapSet? BeatmapSet { get; set; }

        [JsonProperty(@"playcount")]
        public int PlayCount { get; set; }

        [JsonProperty(@"passcount")]
        public int PassCount { get; set; }

        [JsonProperty(@"mode_int")]
        public int RulesetID { get; set; }

        [JsonProperty(@"difficulty_rating")]
        public double StarRating { get; set; }

        [JsonProperty(@"drain")]
        private float drainRate { get; set; }

        [JsonProperty(@"cs")]
        private float circleSize { get; set; }

        [JsonProperty(@"ar")]
        private float approachRate { get; set; }

        [JsonProperty(@"accuracy")]
        private float overallDifficulty { get; set; }

        public double Length => TimeSpan.FromSeconds(lengthInSeconds).TotalMilliseconds;

        [JsonProperty(@"total_length")]
        private double lengthInSeconds { get; set; }

        [JsonProperty(@"count_circles")]
        public int CircleCount { get; set; }

        [JsonProperty(@"count_sliders")]
        public int SliderCount { get; set; }

        [JsonProperty(@"version")]
        public string DifficultyName { get; set; } = string.Empty;

        [JsonProperty(@"failtimes")]
        public APIFailTimes? FailTimes { get; set; }

        [JsonProperty(@"max_combo")]
        public int? MaxCombo { get; set; }

        public double BPM { get; set; }

        public virtual BeatmapInfo ToBeatmapInfo(RulesetStore rulesets)
        {
            var set = BeatmapSet?.ToBeatmapSet(rulesets);

            return new BeatmapInfo
            {
                Metadata = set?.Metadata ?? new BeatmapMetadata(),
                Ruleset = rulesets.GetRuleset(RulesetID),
                StarDifficulty = StarRating,
                OnlineBeatmapID = OnlineID,
                Version = DifficultyName,
                // this is actually an incorrect mapping (Length is calculated as drain length in lazer's import process, see BeatmapManager.calculateLength).
                Length = Length,
                Status = Status,
                MD5Hash = Checksum,
                BeatmapSet = set,
                MaxCombo = MaxCombo,
                BaseDifficulty = new BeatmapDifficulty
                {
                    DrainRate = drainRate,
                    CircleSize = circleSize,
                    ApproachRate = approachRate,
                    OverallDifficulty = overallDifficulty,
                },
                OnlineInfo = this,
            };
        }

        #region Implementation of IBeatmapInfo

        public IBeatmapMetadataInfo Metadata => (BeatmapSet as IBeatmapSetInfo)?.Metadata ?? new BeatmapMetadata();

        public IBeatmapDifficultyInfo Difficulty => new BeatmapDifficulty
        {
            DrainRate = drainRate,
            CircleSize = circleSize,
            ApproachRate = approachRate,
            OverallDifficulty = overallDifficulty,
        };

        IBeatmapSetInfo? IBeatmapInfo.BeatmapSet => BeatmapSet;

        public string MD5Hash => Checksum;

        public IRulesetInfo Ruleset => new RulesetInfo { ID = RulesetID };

        [JsonIgnore]
        public string Hash => throw new NotImplementedException();

        #endregion
    }
}
