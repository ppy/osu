// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Linq;
using SQLite.Net.Attributes;

namespace osu.Game.Database
{
    public class BeatmapMetadata
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }

        public int? OnlineBeatmapSetID { get; set; }

        public string Title { get; set; }
        public string TitleUnicode { get; set; }
        public string Artist { get; set; }
        public string ArtistUnicode { get; set; }
        public string Author { get; set; }
        public string Source { get; set; }
        public string Tags { get; set; }
        public int PreviewTime { get; set; }
        public string AudioFile { get; set; }
        public string BackgroundFile { get; set; }

        public string[] SearchableTerms => new[]
        {
            Author,
            Artist,
            ArtistUnicode,
            Title,
            TitleUnicode,
            Source,
            Tags
        }.Where(s => !string.IsNullOrEmpty(s)).ToArray();
    }
}