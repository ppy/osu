// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using osu.Framework.Testing;
using osu.Game.Database;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Rulesets;
using osu.Game.Scoring;

namespace osu.Game.Beatmaps
{
    [ExcludeFromDynamicCompile]
    [Serializable]
    [Table(@"BeatmapInfo")]
    public class EFBeatmapInfo : IEquatable<EFBeatmapInfo>, IHasPrimaryKey, IBeatmapInfo
    {
        public int ID { get; set; }

        public bool IsManaged => ID > 0;

        public int BeatmapVersion;

        private int? onlineID;

        [JsonProperty("id")]
        [Column("OnlineBeatmapID")]
        public int? OnlineID
        {
            get => onlineID;
            set => onlineID = value > 0 ? value : null;
        }

        [JsonIgnore]
        public int BeatmapSetInfoID { get; set; }

        public BeatmapOnlineStatus Status { get; set; } = BeatmapOnlineStatus.None;

        [Required]
        public EFBeatmapSetInfo BeatmapSetInfo { get; set; }

        public EFBeatmapMetadata Metadata { get; set; }

        [JsonIgnore]
        public int BaseDifficultyID { get; set; }

        public EFBeatmapDifficulty BaseDifficulty { get; set; }

        [NotMapped]
        public APIBeatmap OnlineInfo { get; set; }

        [NotMapped]
        public int? MaxCombo { get; set; }

        /// <summary>
        /// The playable length in milliseconds of this beatmap.
        /// </summary>
        public double Length { get; set; }

        /// <summary>
        /// The most common BPM of this beatmap.
        /// </summary>
        public double BPM { get; set; }

        public string Path { get; set; }

        [JsonProperty("file_sha2")]
        public string Hash { get; set; }

        [JsonIgnore]
        public bool Hidden { get; set; }

        /// <summary>
        /// MD5 is kept for legacy support (matching against replays, osu-web-10 etc.).
        /// </summary>
        [JsonProperty("file_md5")]
        public string MD5Hash { get; set; }

        // General
        public double AudioLeadIn { get; set; }
        public float StackLeniency { get; set; } = 0.7f;
        public bool SpecialStyle { get; set; }

        [Column("RulesetID")]
        public int RulesetInfoID { get; set; }

        public EFRulesetInfo RulesetInfo { get; set; }

        public bool LetterboxInBreaks { get; set; }
        public bool WidescreenStoryboard { get; set; }
        public bool EpilepsyWarning { get; set; }

        /// <summary>
        /// Whether or not sound samples should change rate when playing with speed-changing mods.
        /// TODO: only read/write supported for now, requires implementation in gameplay.
        /// </summary>
        public bool SamplesMatchPlaybackRate { get; set; }

        public CountdownType Countdown { get; set; } = CountdownType.Normal;

        /// <summary>
        /// The number of beats to move the countdown backwards (compared to its default location).
        /// </summary>
        public int CountdownOffset { get; set; }

        [NotMapped]
        public int[] Bookmarks { get; set; } = Array.Empty<int>();

        public double DistanceSpacing { get; set; }
        public int BeatDivisor { get; set; }
        public int GridSize { get; set; }
        public double TimelineZoom { get; set; }

        // Metadata
        [Column("Version")]
        public string DifficultyName { get; set; }

        [JsonProperty("difficulty_rating")]
        [Column("StarDifficulty")]
        public double StarRating { get; set; }

        /// <summary>
        /// Currently only populated for beatmap deletion. Use <see cref="ScoreManager"/> to query scores.
        /// </summary>
        public List<EFScoreInfo> Scores { get; set; }

        [JsonIgnore]
        public DifficultyRating DifficultyRating => BeatmapDifficultyCache.GetDifficultyRating(StarRating);

        public override string ToString() => this.GetDisplayTitle();

        public bool Equals(EFBeatmapInfo other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;

            if (ID != 0 && other.ID != 0)
                return ID == other.ID;

            return false;
        }

        public bool Equals(IBeatmapInfo other) => other is EFBeatmapInfo b && Equals(b);

        public bool AudioEquals(EFBeatmapInfo other) => other != null && BeatmapSetInfo != null && other.BeatmapSetInfo != null &&
                                                        BeatmapSetInfo.Hash == other.BeatmapSetInfo.Hash &&
                                                        (Metadata ?? BeatmapSetInfo.Metadata).AudioFile == (other.Metadata ?? other.BeatmapSetInfo.Metadata).AudioFile;

        public bool BackgroundEquals(EFBeatmapInfo other) => other != null && BeatmapSetInfo != null && other.BeatmapSetInfo != null &&
                                                             BeatmapSetInfo.Hash == other.BeatmapSetInfo.Hash &&
                                                             (Metadata ?? BeatmapSetInfo.Metadata).BackgroundFile == (other.Metadata ?? other.BeatmapSetInfo.Metadata).BackgroundFile;

        /// <summary>
        /// Returns a shallow-clone of this <see cref="EFBeatmapInfo"/>.
        /// </summary>
        public EFBeatmapInfo Clone() => (EFBeatmapInfo)MemberwiseClone();

        #region Implementation of IHasOnlineID

        int IHasOnlineID<int>.OnlineID => OnlineID ?? -1;

        #endregion

        #region Implementation of IBeatmapInfo

        [JsonIgnore]
        IBeatmapMetadataInfo IBeatmapInfo.Metadata => Metadata ?? BeatmapSetInfo?.Metadata ?? new EFBeatmapMetadata();

        [JsonIgnore]
        IBeatmapDifficultyInfo IBeatmapInfo.Difficulty => BaseDifficulty;

        [JsonIgnore]
        IBeatmapSetInfo IBeatmapInfo.BeatmapSet => BeatmapSetInfo;

        [JsonIgnore]
        IRulesetInfo IBeatmapInfo.Ruleset => RulesetInfo;

        #endregion
    }
}
