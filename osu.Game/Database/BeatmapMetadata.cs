using System;
using osu.Game.GameModes.Play;
using SQLite;

namespace osu.Game.Database
{
    public class BeatmapMetadata
    {
        [PrimaryKey]
        public int ID { get; set; }
        public string Title { get; set; }
        public string TitleUnicode { get; set; }
        public string Artist { get; set; }
        public string ArtistUnicode { get; set; }
        public string Author { get; set; }
        public string Source { get; set; }
        public string Tags { get; set; }
        public PlayMode Mode { get; set; }
        public int PreviewTime { get; set; }
        public string AudioFile { get; set; }
        public string BackgroundFile { get; set; }
    }
}