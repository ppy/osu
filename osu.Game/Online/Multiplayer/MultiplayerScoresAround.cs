// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Newtonsoft.Json;

namespace osu.Game.Online.Multiplayer
{
    /// <summary>
    /// An object which stores scores higher and lower than the user's score.
    /// </summary>
    public class MultiplayerScoresAround
    {
        /// <summary>
        /// Scores sorted "higher" than the user's score, depending on the sorting order.
        /// </summary>
        [JsonProperty("higher")]
        public MultiplayerScores Higher { get; set; }

        /// <summary>
        /// Scores sorted "lower" than the user's score, depending on the sorting order.
        /// </summary>
        [JsonProperty("lower")]
        public MultiplayerScores Lower { get; set; }
    }
}
