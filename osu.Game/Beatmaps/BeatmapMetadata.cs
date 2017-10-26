// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Newtonsoft.Json;
using osu.Game.Users;

namespace osu.Game.Beatmaps
{
    public class BeatmapMetadata
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        private int? onlineBeatmapSetID;

        [NotMapped]
        [JsonProperty(@"id")]
        public int? OnlineBeatmapSetID
        {
            get { return onlineBeatmapSetID; }
            set { onlineBeatmapSetID = value > 0 ? value : null; }
        }

        public string Title { get; set; }
        public string TitleUnicode { get; set; }
        public string Artist { get; set; }
        public string ArtistUnicode { get; set; }

        public List<BeatmapInfo> Beatmaps { get; set; }
        public List<BeatmapSetInfo> BeatmapSets { get; set; }

        /// <summary>
        /// Helper property to deserialize a username to <see cref="User"/>.
        /// </summary>
        [JsonProperty(@"creator")]
        [Column("Author")]
        public string AuthorString
        {
            get { return Author?.Username; }
            set { Author = new User { Username = value }; }
        }

        /// <summary>
        /// The author of the beatmaps in this set.
        /// </summary>
        public User Author;

        public string Source { get; set; }

        [JsonProperty(@"tags")]
        public string Tags { get; set; }
        public int PreviewTime { get; set; }
        public string AudioFile { get; set; }
        public string BackgroundFile { get; set; }

        public string[] SearchableTerms => new[]
        {
            Author?.Username,
            Artist,
            ArtistUnicode,
            Title,
            TitleUnicode,
            Source,
            Tags
        }.Where(s => !string.IsNullOrEmpty(s)).ToArray();
    }
}
