// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using JetBrains.Annotations;
using Newtonsoft.Json;

namespace osu.Game.Online.Rooms
{
    /// <summary>
    /// A <see cref="MultiplayerScores"/> object returned via a <see cref="IndexPlaylistScoresRequest"/>.
    /// </summary>
    public class IndexedMultiplayerScores : MultiplayerScores
    {
        /// <summary>
        /// The total scores in the playlist item.
        /// </summary>
        [JsonProperty("total")]
        public long? TotalScores { get; set; }

        /// <summary>
        /// The user's score, if any.
        /// </summary>
        [JsonProperty("user_score")]
        [CanBeNull]
        public MultiplayerScore UserScore { get; set; }
    }
}
