// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using Newtonsoft.Json;
using osu.Game.Online.API.Requests;

namespace osu.Game.Online.Rooms
{
    /// <summary>
    /// An object which contains scores and related data for fetching next pages.
    /// </summary>
    public class MultiplayerScores : ResponseWithCursor
    {
        /// <summary>
        /// The scores.
        /// </summary>
        [JsonProperty("scores")]
        public List<MultiplayerScore> Scores { get; set; } = new List<MultiplayerScore>();

        /// <summary>
        /// The parameters to be used to fetch the next page.
        /// </summary>
        [JsonProperty("params")]
        public IndexScoresParams Params { get; set; } = new IndexScoresParams();
    }
}
