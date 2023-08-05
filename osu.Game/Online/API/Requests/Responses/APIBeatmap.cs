// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using Newtonsoft.Json;
using osu.Game.Beatmaps;
using osu.Game.Extensions;
using osu.Game.Rulesets;

namespace osu.Game.Online.API.Requests.Responses
{
    public class APIBeatmap : IBeatmapInfo, IBeatmapOnlineInfo
    {
        [JsonProperty(@"id")]
        public int OnlineID { get; set; }

        [JsonProperty(@"beatmapset_id")]
        public int OnlineBeatmapSetID { get; set; }

        [JsonProperty(@"status")]
        public BeatmapOnlineStatus Status { get; set; }

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
        public float DrainRate { get; set; }

        [JsonProperty(@"cs")]
        public float CircleSize { get; set; }

        [JsonProperty(@"ar")]
        public float ApproachRate { get; set; }

        [JsonProperty(@"accuracy")]
        public float OverallDifficulty { get; set; }

        [JsonIgnore]
        public double Length { get; set; }

        [JsonProperty(@"total_length")]
        private double lengthInSeconds
        {
            get => TimeSpan.FromMilliseconds(Length).TotalSeconds;
            set => Length = TimeSpan.FromSeconds(value).TotalMilliseconds;
        }

        [JsonIgnore]
        public double HitLength { get; set; }

        [JsonProperty(@"hit_length")]
        private double hitLengthInSeconds
        {
            get => TimeSpan.FromMilliseconds(HitLength).TotalSeconds;
            set => HitLength = TimeSpan.FromSeconds(value).TotalMilliseconds;
        }

        [JsonProperty(@"convert")]
        public bool Convert { get; set; }

        [JsonProperty(@"count_circles")]
        public int CircleCount { get; set; }

        [JsonProperty(@"count_sliders")]
        public int SliderCount { get; set; }

        [JsonProperty(@"count_spinners")]
        public int SpinnerCount { get; set; }

        [JsonProperty(@"version")]
        public string DifficultyName { get; set; } = string.Empty;

        [JsonProperty(@"failtimes")]
        public APIFailTimes? FailTimes { get; set; }

        [JsonProperty(@"max_combo")]
        public int? MaxCombo { get; set; }

        [JsonProperty(@"last_updated")]
        public DateTimeOffset LastUpdated { get; set; }

        public double BPM { get; set; }

        #region Implementation of IBeatmapInfo

        public IBeatmapMetadataInfo Metadata => (BeatmapSet as IBeatmapSetInfo)?.Metadata ?? new BeatmapMetadata();

        public IBeatmapDifficultyInfo Difficulty => new BeatmapDifficulty
        {
            DrainRate = DrainRate,
            CircleSize = CircleSize,
            ApproachRate = ApproachRate,
            OverallDifficulty = OverallDifficulty,
        };

        IBeatmapSetInfo? IBeatmapInfo.BeatmapSet => BeatmapSet;

        public string MD5Hash => Checksum;

        public IRulesetInfo Ruleset => new APIRuleset { OnlineID = RulesetID };

        [JsonIgnore]
        public string Hash => throw new NotImplementedException();

        #endregion

        public bool Equals(IBeatmapInfo? other) => other is APIBeatmap b && this.MatchesOnlineID(b);

        public class APIRuleset : IRulesetInfo
        {
            public int OnlineID { get; set; } = -1;

            public string Name => $@"{nameof(APIRuleset)} (ID: {OnlineID})";

            public string ShortName
            {
                get
                {
                    // TODO: this should really not exist.
                    switch (OnlineID)
                    {
                        case 0: return "osu";

                        case 1: return "taiko";

                        case 2: return "fruits";

                        case 3: return "mania";

                        default: throw new ArgumentOutOfRangeException();
                    }
                }
            }

            public string InstantiationInfo => string.Empty;

            public Ruleset CreateInstance() => throw new NotImplementedException();

            public bool Equals(IRulesetInfo? other) => other is APIRuleset r && this.MatchesOnlineID(r);

            public int CompareTo(IRulesetInfo? other)
            {
                if (!(other is APIRuleset ruleset))
                    throw new ArgumentException($@"Object is not of type {nameof(APIRuleset)}.", nameof(other));

                return OnlineID.CompareTo(ruleset.OnlineID);
            }

            // ReSharper disable once NonReadonlyMemberInGetHashCode
            public override int GetHashCode() => OnlineID;
        }
    }
}
