using System;
using System.Linq;
using osu.Game.Beatmaps.Samples;
using osu.Game.GameModes.Play;
using SQLite.Net.Attributes;
using SQLiteNetExtensions.Attributes;

namespace osu.Game.Database
{
    public class BeatmapInfo
    {
        [PrimaryKey]
        public int BeatmapID { get; set; }

        [ForeignKey(typeof(BeatmapSetInfo))]
        public int BeatmapSetID { get; set; }

        [ManyToOne]
        public BeatmapSetInfo BeatmapSet { get; set; }
        
        [ForeignKey(typeof(BeatmapMetadata))]
        public int BeatmapMetadataID { get; set; }

        [OneToOne(CascadeOperations = CascadeOperation.All)]
        public BeatmapMetadata Metadata { get; set; }

        [ForeignKey(typeof(BaseDifficulty)), NotNull]
        public int BaseDifficultyID { get; set; }

        [OneToOne(CascadeOperations = CascadeOperation.All)]
        public BaseDifficulty BaseDifficulty { get; set; }
        
        public string Path { get; set; }
        
        // General
        public int AudioLeadIn { get; set; }
        public bool Countdown { get; set; }
        public SampleSet SampleSet { get; set; }
        public float StackLeniency { get; set; }
        public bool SpecialStyle { get; set; }
        public PlayMode Mode { get; set; }
        public bool LetterboxInBreaks { get; set; }
        public bool WidescreenStoryboard { get; set; }
        
        // Editor
        // This bookmarks stuff is necessary because DB doesn't know how to store int[]
        public string StoredBookmarks { get; internal set; }
        [Ignore]
        public int[] Bookmarks
        {
            get
            {
                return StoredBookmarks.Split(',').Select(b => int.Parse(b)).ToArray();
            }
            set
            {
                StoredBookmarks = string.Join(",", value);
            }
        }
        public double DistanceSpacing { get; set; }
        public int BeatDivisor { get; set; }
        public int GridSize { get; set; }
        public double TimelineZoom { get; set; }
        
        // Metadata
        public string Version { get; set; }
    }
}