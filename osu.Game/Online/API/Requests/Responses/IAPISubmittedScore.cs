// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Online.API.Requests.Responses
{
    /// <summary>
    /// Common interface for API structures that are returned
    /// </summary>
    public interface IAPISubmittedScore
    {
        /// <summary>
        /// The <c>solo_scores</c>-schema ID of the score.
        /// </summary>
        ulong? SoloScoreID { get; }

        /// <summary>
        /// The position of the score on the leaderboard.
        /// </summary>
        /// <remarks>
        /// Only present in multiplayer.
        /// </remarks>
        int? Position { get; }
    }
}
