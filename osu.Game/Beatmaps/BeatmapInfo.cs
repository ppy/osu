// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Newtonsoft.Json;
using osu.Framework.Testing;
using osu.Game.Database;
using osu.Game.IO.Serialization;
using osu.Game.Rulesets;
using osu.Game.Scoring;

namespace osu.Game.Beatmaps
{
    [ExcludeFromDynamicCompile]
    [Serializable]
    public class BeatmapInfo : IEquatable<BeatmapInfo>, IJsonSerializable, IHasPrimaryKey
    {
        public int ID { get; set; }

        public int BeatmapVersion;

        private int? onlineBeatmapID;

        [JsonProperty("id")]
        public int? OnlineBeatmapID
        {
            get => onlineBeatmapID;
            set => onlineBeatmapID = value > 0 ? value : null;
        }

        [JsonIgnore]
        public int BeatmapSetInfoID { get; set; }

        public BeatmapSetOnlineStatus Status { get; set; } = BeatmapSetOnlineStatus.None;

        [Required]
        public BeatmapSetInfo BeatmapSet { get; set; }

        public BeatmapMetadata Metadata { get; set; }

        [JsonIgnore]
        public int BaseDifficultyID { get; set; }

        public BeatmapDifficulty BaseDifficulty { get; set; }

        [NotMapped]
        public BeatmapMetrics Metrics { get; set; }

        [NotMapped]
        public BeatmapOnlineInfo OnlineInfo { get; set; }

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
        public bool Countdown { get; set; } = true;
        public float StackLeniency { get; set; } = 0.7f;
        public bool SpecialStyle { get; set; }

        public int RulesetID { get; set; }

        public RulesetInfo Ruleset { get; set; }

        public bool LetterboxInBreaks { get; set; }
        public bool WidescreenStoryboard { get; set; }
        public bool EpilepsyWarning { get; set; }

        // Editor
        // This bookmarks stuff is necessary because DB doesn't know how to store int[]
        [JsonIgnore]
        public string StoredBookmarks
        {
            get => string.Join(',', Bookmarks);
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    Bookmarks = Array.Empty<int>();
                    return;
                }

                Bookmarks = value.Split(',').Select(v =>
                {
                    bool result = int.TryParse(v, out int val);
                    return new { result, val };
                }).Where(p => p.result).Select(p => p.val).ToArray();
            }
        }

        [NotMapped]
        public int[] Bookmarks { get; set; } = Array.Empty<int>();

        public double DistanceSpacing { get; set; }
        public int BeatDivisor { get; set; }
        public int GridSize { get; set; }
        public double TimelineZoom { get; set; }

        // Metadata
        public string Version { get; set; }

        [JsonProperty("difficulty_rating")]
        public double StarDifficulty { get; set; }

        /// <summary>
        /// Currently only populated for beatmap deletion. Use <see cref="ScoreManager"/> to query scores.
        /// </summary>
        public List<ScoreInfo> Scores { get; set; }

        [JsonIgnore]
        public DifficultyRating DifficultyRating => BeatmapDifficultyCache.GetDifficultyRating(StarDifficulty);

        public string[] SearchableTerms => new[]
        {
            Version
        }.Concat(Metadata?.SearchableTerms ?? Enumerable.Empty<string>()).Where(s => !string.IsNullOrEmpty(s)).ToArray();

        public override string ToString()
        {
            string version = string.IsNullOrEmpty(Version) ? string.Empty : $"[{Version}]";

            return $"{Metadata} {version}".Trim();
        }

        public bool Equals(BeatmapInfo other)
        {
            if (ID == 0 || other?.ID == 0)
                // one of the two BeatmapInfos we are comparing isn't sourced from a database.
                // fall back to reference equality.
                return ReferenceEquals(this, other);

            return ID == other?.ID;
        }

        public bool AudioEquals(BeatmapInfo other) => other != null && BeatmapSet != null && other.BeatmapSet != null &&
                                                      BeatmapSet.Hash == other.BeatmapSet.Hash &&
                                                      (Metadata ?? BeatmapSet.Metadata).AudioFile == (other.Metadata ?? other.BeatmapSet.Metadata).AudioFile;

        public bool BackgroundEquals(BeatmapInfo other) => other != null && BeatmapSet != null && other.BeatmapSet != null &&
                                                           BeatmapSet.Hash == other.BeatmapSet.Hash &&
                                                           (Metadata ?? BeatmapSet.Metadata).BackgroundFile == (other.Metadata ?? other.BeatmapSet.Metadata).BackgroundFile;

        /// <summary>
        /// Returns a shallow-clone of this <see cref="BeatmapInfo"/>.
        /// </summary>
        public BeatmapInfo Clone() => (BeatmapInfo)MemberwiseClone();
    }
}
