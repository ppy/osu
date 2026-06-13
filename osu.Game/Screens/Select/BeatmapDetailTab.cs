// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

namespace osu.Game.Screens.Select
{
    public enum BeatmapDetailTab
    {
        /// <summary>
        /// Beatmap details.
        /// </summary>
        Details,

        /// <summary>
        /// Ranking.
        /// </summary>
        Ranking,

        /// <summary>
        /// Local leaderboards.
        /// </summary>
        /// <remarks>
        /// Provided for compatibility with older clients - can be removed 20261113.
        /// </remarks>
        [Obsolete("Use BeatmapLeaderboardScope instead")]
        Local,

        /// <summary>
        /// Country leaderboards.
        /// </summary>
        /// <remarks>
        /// Provided for compatibility with older clients - can be removed 20261113.
        /// </remarks>
        [Obsolete("Use BeatmapLeaderboardScope instead")]
        Country,

        /// <summary>
        /// Global leaderboards.
        /// </summary>
        /// <remarks>
        /// For compatibility with older clients - can be removed 20261113.
        /// </remarks>
        [Obsolete("Use BeatmapLeaderboardScope instead")]
        Global,

        /// <summary>
        /// Friend leaderboards.
        /// </summary>
        /// <remarks>
        /// For compatibility with older clients - can be removed 20261113.
        /// </remarks>
        [Obsolete("Use BeatmapLeaderboardScope instead")]
        Friends,

        /// <summary>
        /// Team leaderboards.
        /// </summary>
        /// <remarks>
        /// For compatibility with older clients - can be removed 20261113.
        /// </remarks>
        [Obsolete("Use BeatmapLeaderboardScope instead")]
        Team
    }
}
