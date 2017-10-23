﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Newtonsoft.Json;
using osu.Game.IO.Serialization;
using osu.Game.Rulesets;

namespace osu.Game.Beatmaps
{
    public class BeatmapInfo : IEquatable<BeatmapInfo>, IJsonSerializable
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        //TODO: should be in database
        public int BeatmapVersion;

        [JsonProperty("id")]
        public int? OnlineBeatmapID { get; set; }

        [JsonProperty("beatmapset_id")]
        [NotMapped]
        public int? OnlineBeatmapSetID { get; set; }

        public int BeatmapSetInfoID { get; set; }

        [Required]
        public BeatmapSetInfo BeatmapSet { get; set; }

        public BeatmapMetadata Metadata { get; set; }

        public int BaseDifficultyID { get; set; }

        public BeatmapDifficulty BaseDifficulty { get; set; }

        [NotMapped]
        public BeatmapMetrics Metrics { get; set; }

        [NotMapped]
        public BeatmapOnlineInfo OnlineInfo { get; set; }

        public string Path { get; set; }

        [JsonProperty("file_sha2")]
        public string Hash { get; set; }

        public bool Hidden { get; set; }

        /// <summary>
        /// MD5 is kept for legacy support (matching against replays, osu-web-10 etc.).
        /// </summary>
        [JsonProperty("file_md5")]
        public string MD5Hash { get; set; }

        // General
        public int AudioLeadIn { get; set; }
        public bool Countdown { get; set; }
        public float StackLeniency { get; set; }
        public bool SpecialStyle { get; set; }

        public int RulesetID { get; set; }

        public RulesetInfo Ruleset { get; set; }

        public bool LetterboxInBreaks { get; set; }
        public bool WidescreenStoryboard { get; set; }

        // Editor
        // This bookmarks stuff is necessary because DB doesn't know how to store int[]
        [JsonIgnore]
        public string StoredBookmarks
        {
            get { return string.Join(",", Bookmarks); }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    Bookmarks = new int[0];
                    return;
                }

                Bookmarks = value.Split(',').Select(v =>
                {
                    int val;
                    bool result = int.TryParse(v, out val);
                    return new { result, val };
                }).Where(p => p.result).Select(p => p.val).ToArray();
            }
        }

        [NotMapped]
        public int[] Bookmarks { get; set; } = new int[0];

        public double DistanceSpacing { get; set; }
        public int BeatDivisor { get; set; }
        public int GridSize { get; set; }
        public double TimelineZoom { get; set; }

        // Metadata
        public string Version { get; set; }

        public double StarDifficulty { get; set; }

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
    }
}
