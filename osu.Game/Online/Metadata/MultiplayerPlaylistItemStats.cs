// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using MessagePack;

namespace osu.Game.Online.Metadata
{
    [MessagePackObject]
    [Serializable]
    public class MultiplayerPlaylistItemStats
    {
        public const int TOTAL_SCORE_DISTRIBUTION_BINS = 13;

        /// <summary>
        /// The ID of the playlist item which these stats pertain to.
        /// </summary>
        [Key(0)]
        public long PlaylistItemID { get; set; }

        /// <summary>
        /// The count of scores with given total ranges in the room.
        /// The ranges are bracketed into <see cref="TOTAL_SCORE_DISTRIBUTION_BINS"/> bins, each of 100,000 score width.
        /// The last bin will contain count of all scores with total of 1,200,000 or larger.
        /// </summary>
        [Key(1)]
        public long[] TotalScoreDistribution { get; set; } = new long[TOTAL_SCORE_DISTRIBUTION_BINS];

        /// <summary>
        /// The cumulative total of all passing scores (across all users) for the playlist item so far.
        /// </summary>
        [Key(2)]
        public long CumulativeScore { get; set; }

        /// <summary>
        /// The last score to have been processed into provided statistics. Generally only for server-side accounting purposes.
        /// </summary>
        [Key(3)]
        public ulong LastProcessedScoreID { get; set; }
    }
}
