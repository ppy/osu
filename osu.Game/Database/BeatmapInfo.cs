// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using Newtonsoft.Json;
using osu.Game.IO.Serialization;
using SQLite.Net.Attributes;
using SQLiteNetExtensions.Attributes;
using System;
using System.Linq;

namespace osu.Game.Database
{
    public class BeatmapInfo : IEquatable<BeatmapInfo>, IJsonSerializable
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }

        //TODO: should be in database
        public int BeatmapVersion;

        public int? OnlineBeatmapID { get; set; }

        public int? OnlineBeatmapSetID { get; set; }

        [ForeignKey(typeof(BeatmapSetInfo))]
        public int BeatmapSetInfoID { get; set; }

        [ManyToOne]
        public BeatmapSetInfo BeatmapSet { get; set; }

        [ForeignKey(typeof(BeatmapMetadata))]
        public int BeatmapMetadataID { get; set; }

        [OneToOne(CascadeOperations = CascadeOperation.All)]
        public BeatmapMetadata Metadata { get; set; }

        [ForeignKey(typeof(BeatmapDifficulty)), NotNull]
        public int BaseDifficultyID { get; set; }

        [OneToOne(CascadeOperations = CascadeOperation.All)]
        public BeatmapDifficulty Difficulty { get; set; }

        [Ignore]
        public BeatmapMetrics Metrics { get; set; }

        public string Path { get; set; }

        [JsonProperty("file_md5")]
        public string Hash { get; set; }

        // General
        public int AudioLeadIn { get; set; }
        public bool Countdown { get; set; }
        public float StackLeniency { get; set; }
        public bool SpecialStyle { get; set; }

        [ForeignKey(typeof(RulesetInfo))]
        public int RulesetID { get; set; }

        [OneToOne(CascadeOperations = CascadeOperation.All)]
        public RulesetInfo Ruleset { get; set; }

        public bool LetterboxInBreaks { get; set; }
        public bool WidescreenStoryboard { get; set; }

        // Editor
        // This bookmarks stuff is necessary because DB doesn't know how to store int[]
        public string StoredBookmarks { get; internal set; }

        [Ignore]
        [JsonIgnore]
        public int[] Bookmarks
        {
            get { return StoredBookmarks.Split(',').Select(int.Parse).ToArray(); }
            set { StoredBookmarks = string.Join(",", value); }
        }

        public double DistanceSpacing { get; set; }
        public int BeatDivisor { get; set; }
        public int GridSize { get; set; }
        public double TimelineZoom { get; set; }

        // Metadata
        public string Version { get; set; }

        public double StarDifficulty { get; set; }

        public bool Equals(BeatmapInfo other)
        {
            return ID == other?.ID;
        }

        public bool AudioEquals(BeatmapInfo other) => other != null && BeatmapSet != null && other.BeatmapSet != null &&
                                                      BeatmapSet.Path == other.BeatmapSet.Path &&
                                                      (Metadata ?? BeatmapSet.Metadata).AudioFile == (other.Metadata ?? other.BeatmapSet.Metadata).AudioFile;
    }
}
