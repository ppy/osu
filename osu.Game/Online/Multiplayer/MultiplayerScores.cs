// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using Newtonsoft.Json;
using osu.Game.Online.API.Requests;

namespace osu.Game.Online.Multiplayer
{
    /// <summary>
    /// An object which contains scores and related data for fetching next pages.
    /// </summary>
    public class MultiplayerScores
    {
        /// <summary>
        /// To be used for fetching the next page.
        /// </summary>
        [JsonProperty("cursor")]
        public Cursor Cursor { get; set; }

        /// <summary>
        /// The scores.
        /// </summary>
        [JsonProperty("scores")]
        public List<MultiplayerScore> Scores { get; set; }

        /// <summary>
        /// The total scores in the playlist item. Only provided via <see cref="IndexPlaylistScoresRequest"/>.
        /// </summary>
        [JsonProperty("total")]
        public int? TotalScores { get; set; }

        /// <summary>
        /// The user's score, if any. Only provided via <see cref="IndexPlaylistScoresRequest"/>.
        /// </summary>
        [JsonProperty("user_score")]
        public MultiplayerScore UserScore { get; set; }
    }
}
