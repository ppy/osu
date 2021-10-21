using System;

namespace osu.Game.Beatmaps
{
    /// <summary>
    /// Beatmap set info retrieved for previewing locally without having the set downloaded.
    /// </summary>
    public interface IBeatmapSetOnlineInfo
    {
        /// <summary>
        /// The date this beatmap set was submitted to the online listing.
        /// </summary>
        DateTimeOffset Submitted { get; set; }

        /// <summary>
        /// The date this beatmap set was ranked.
        /// </summary>
        DateTimeOffset? Ranked { get; set; }

        /// <summary>
        /// The date this beatmap set was last updated.
        /// </summary>
        DateTimeOffset? LastUpdated { get; set; }

        /// <summary>
        /// The status of this beatmap set.
        /// </summary>
        BeatmapSetOnlineStatus Status { get; set; }

        /// <summary>
        /// Whether or not this beatmap set has explicit content.
        /// </summary>
        bool HasExplicitContent { get; set; }

        /// <summary>
        /// Whether or not this beatmap set has a background video.
        /// </summary>
        bool HasVideo { get; set; }

        /// <summary>
        /// Whether or not this beatmap set has a storyboard.
        /// </summary>
        bool HasStoryboard { get; set; }

        /// <summary>
        /// The different sizes of cover art for this beatmap set.
        /// </summary>
        BeatmapSetOnlineCovers Covers { get; set; }

        /// <summary>
        /// A small sample clip of this beatmap set's song.
        /// </summary>
        string Preview { get; set; }

        /// <summary>
        /// The beats per minute of this beatmap set's song.
        /// </summary>
        double BPM { get; set; }

        /// <summary>
        /// The amount of plays this beatmap set has.
        /// </summary>
        int PlayCount { get; set; }

        /// <summary>
        /// The amount of people who have favourited this beatmap set.
        /// </summary>
        int FavouriteCount { get; set; }

        /// <summary>
        /// Whether this beatmap set has been favourited by the current user.
        /// </summary>
        bool HasFavourited { get; set; }

        /// <summary>
        /// The availability of this beatmap set.
        /// </summary>
        BeatmapSetOnlineAvailability Availability { get; set; }

        /// <summary>
        /// The song genre of this beatmap set.
        /// </summary>
        BeatmapSetOnlineGenre Genre { get; set; }

        /// <summary>
        /// The song language of this beatmap set.
        /// </summary>
        BeatmapSetOnlineLanguage Language { get; set; }

        /// <summary>
        /// The track ID of this beatmap set.
        /// Non-null only if the track is linked to a featured artist track entry.
        /// </summary>
        int? TrackId { get; set; }
    }
}
