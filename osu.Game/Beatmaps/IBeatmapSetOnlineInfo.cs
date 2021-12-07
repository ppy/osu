// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

#nullable enable

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
        DateTimeOffset Submitted { get; }

        /// <summary>
        /// The date this beatmap set was ranked.
        /// </summary>
        DateTimeOffset? Ranked { get; }

        /// <summary>
        /// The date this beatmap set was last updated.
        /// </summary>
        DateTimeOffset? LastUpdated { get; }

        /// <summary>
        /// The status of this beatmap set.
        /// </summary>
        BeatmapOnlineStatus Status { get; }

        /// <summary>
        /// Whether or not this beatmap set has explicit content.
        /// </summary>
        bool HasExplicitContent { get; }

        /// <summary>
        /// Whether or not this beatmap set has a background video.
        /// </summary>
        bool HasVideo { get; }

        /// <summary>
        /// Whether or not this beatmap set has a storyboard.
        /// </summary>
        bool HasStoryboard { get; }

        /// <summary>
        /// The different sizes of cover art for this beatmap set.
        /// </summary>
        BeatmapSetOnlineCovers Covers { get; }

        /// <summary>
        /// A small sample clip of this beatmap set's song.
        /// </summary>
        string Preview { get; }

        /// <summary>
        /// The beats per minute of this beatmap set's song.
        /// </summary>
        double BPM { get; }

        /// <summary>
        /// The amount of plays this beatmap set has.
        /// </summary>
        int PlayCount { get; }

        /// <summary>
        /// The amount of people who have favourited this beatmap set.
        /// </summary>
        int FavouriteCount { get; }

        /// <summary>
        /// Whether this beatmap set has been favourited by the current user.
        /// </summary>
        bool HasFavourited { get; }

        /// <summary>
        /// The availability of this beatmap set.
        /// </summary>
        BeatmapSetOnlineAvailability Availability { get; }

        /// <summary>
        /// The song genre of this beatmap set.
        /// </summary>
        BeatmapSetOnlineGenre Genre { get; }

        /// <summary>
        /// The song language of this beatmap set.
        /// </summary>
        BeatmapSetOnlineLanguage Language { get; }

        /// <summary>
        /// The track ID of this beatmap set.
        /// Non-null only if the track is linked to a featured artist track entry.
        /// </summary>
        int? TrackId { get; }

        /// <summary>
        /// Total vote counts of user ratings on a scale of 0..10 where 0 is unused (probably will be fixed at API?).
        /// </summary>
        int[]? Ratings { get; }

        /// <summary>
        /// Contains the current hype status of the beatmap set.
        /// Non-null only for <see cref="BeatmapOnlineStatus.WIP"/>, <see cref="BeatmapOnlineStatus.Pending"/>, and <see cref="BeatmapOnlineStatus.Qualified"/> sets.
        /// </summary>
        /// <remarks>
        /// See: https://github.com/ppy/osu-web/blob/93930cd02cfbd49724929912597c727c9fbadcd1/app/Models/Beatmapset.php#L155
        /// </remarks>
        BeatmapSetHypeStatus? HypeStatus { get; }

        /// <summary>
        /// Contains the current nomination status of the beatmap set.
        /// </summary>
        BeatmapSetNominationStatus? NominationStatus { get; }
    }
}
