// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Newtonsoft.Json;

namespace osu.Game.Beatmaps
{
    public class BeatmapMetadata
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int? BeatmapSetOnlineInfoId { get; set; }

        public string Title { get; set; }
        public string TitleUnicode { get; set; }
        public string Artist { get; set; }
        public string ArtistUnicode { get; set; }

        [JsonProperty(@"creator")]
        public string Author { get; set; }

        public string Source { get; set; }

        [JsonProperty(@"tags")]
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
