// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using osu.Framework.Testing;
using osu.Game.Database;
using osu.Game.Users;

namespace osu.Game.Beatmaps
{
    [ExcludeFromDynamicCompile]
    [Serializable]
    public class BeatmapMetadata : IEquatable<BeatmapMetadata>, IHasPrimaryKey, IBeatmapMetadataInfo
    {
        public int ID { get; set; }

        public string Title { get; set; }

        [JsonProperty("title_unicode")]
        public string TitleUnicode { get; set; }

        public string Artist { get; set; }

        [JsonProperty("artist_unicode")]
        public string ArtistUnicode { get; set; }

        [JsonIgnore]
        public List<BeatmapInfo> Beatmaps { get; set; }

        [JsonIgnore]
        public List<BeatmapSetInfo> BeatmapSets { get; set; }

        /// <summary>
        /// Helper property to deserialize a username to <see cref="User"/>.
        /// </summary>
        [JsonProperty(@"user_id")]
        [Column("AuthorID")]
        public int AuthorID
        {
            get => Author?.Id ?? 1;
            set
            {
                Author ??= new User();
                Author.Id = value;
            }
        }

        /// <summary>
        /// Helper property to deserialize a username to <see cref="User"/>.
        /// </summary>
        [JsonProperty(@"creator")]
        [Column("Author")]
        public string AuthorString
        {
            get => Author?.Username;
            set
            {
                Author ??= new User();
                Author.Username = value;
            }
        }

        /// <summary>
        /// The author of the beatmaps in this set.
        /// </summary>
        [JsonIgnore]
        public User Author;

        public string Source { get; set; }

        [JsonProperty(@"tags")]
        public string Tags { get; set; }

        /// <summary>
        /// The time in milliseconds to begin playing the track for preview purposes.
        /// If -1, the track should begin playing at 40% of its length.
        /// </summary>
        public int PreviewTime { get; set; }

        public string AudioFile { get; set; }

        public string BackgroundFile { get; set; }

        public bool Equals(BeatmapMetadata other) => ((IBeatmapMetadataInfo)this).Equals(other);

        public override string ToString() => this.GetDisplayTitle();

        string IBeatmapMetadataInfo.Author => AuthorString;
    }
}
