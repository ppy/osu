// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using Newtonsoft.Json;

namespace osu.Game.Beatmaps
{
    /// <summary>
    /// Beatmap set info retrieved for previewing locally without having the set downloaded.
    /// </summary>
    public class BeatmapSetOnlineInfo
    {
        /// <summary>
        /// The date this beatmap set was submitted to the online listing.
        /// </summary>
        public DateTimeOffset Submitted { get; set; }

        /// <summary>
        /// The date this beatmap set was ranked.
        /// </summary>
        public DateTimeOffset? Ranked { get; set; }

        /// <summary>
        /// The date this beatmap set was last updated.
        /// </summary>
        public DateTimeOffset? LastUpdated { get; set; }

        /// <summary>
        /// The status of this beatmap set.
        /// </summary>
        public BeatmapSetOnlineStatus Status { get; set; }

        /// <summary>
        /// Whether or not this beatmap set has a background video.
        /// </summary>
        public bool HasVideo { get; set; }

        /// <summary>
        /// Whether or not this beatmap set has a storyboard.
        /// </summary>
        public bool HasStoryboard { get; set; }

        /// <summary>
        /// The different sizes of cover art for this beatmap set.
        /// </summary>
        public BeatmapSetOnlineCovers Covers { get; set; }

        /// <summary>
        /// A small sample clip of this beatmap set's song.
        /// </summary>
        public string Preview { get; set; }

        /// <summary>
        /// The beats per minute of this beatmap set's song.
        /// </summary>
        public double BPM { get; set; }

        /// <summary>
        /// The amount of plays this beatmap set has.
        /// </summary>
        public int PlayCount { get; set; }

        /// <summary>
        /// The amount of people who have favourited this beatmap set.
        /// </summary>
        public int FavouriteCount { get; set; }

        /// <summary>
        /// Whether this beatmap set has been favourited by the current user.
        /// </summary>
        public bool HasFavourited { get; set; }

        /// <summary>
        /// The availability of this beatmap set.
        /// </summary>
        public BeatmapSetOnlineAvailability Availability { get; set; }

        /// <summary>
        /// The song genre of this beatmap set.
        /// </summary>
        public BeatmapSetOnlineGenre Genre { get; set; }

        /// <summary>
        /// The song language of this beatmap set.
        /// </summary>
        public BeatmapSetOnlineLanguage Language { get; set; }
    }

    public class BeatmapSetOnlineGenre
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class BeatmapSetOnlineLanguage
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class BeatmapSetOnlineCovers
    {
        public string CoverLowRes { get; set; }

        [JsonProperty(@"cover@2x")]
        public string Cover { get; set; }

        public string CardLowRes { get; set; }

        [JsonProperty(@"card@2x")]
        public string Card { get; set; }

        public string ListLowRes { get; set; }

        [JsonProperty(@"list@2x")]
        public string List { get; set; }
    }

    public class BeatmapSetOnlineAvailability
    {
        [JsonProperty(@"download_disabled")]
        public bool DownloadDisabled { get; set; }

        [JsonProperty(@"more_information")]
        public string ExternalLink { get; set; }
    }
}
