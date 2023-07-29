// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading.Tasks;

namespace osu.Game.Online.Spectator
{
    /// <summary>
    /// An interface defining a spectator client instance.
    /// </summary>
    public interface ISpectatorClient
    {
        /// <summary>
        /// Signals that a user has begun a new play session.
        /// </summary>
        /// <param name="userId">The user.</param>
        /// <param name="state">The state of gameplay.</param>
        Task UserBeganPlaying(int userId, SpectatorState state);

        /// <summary>
        /// Signals that a user has finished a play session.
        /// </summary>
        /// <param name="userId">The user.</param>
        /// <param name="state">The state of gameplay.</param>
        Task UserFinishedPlaying(int userId, SpectatorState state);

        /// <summary>
        /// Called when new frames are available for a subscribed user's play session.
        /// </summary>
        /// <param name="userId">The user.</param>
        /// <param name="data">The frame data.</param>
        Task UserSentFrames(int userId, FrameDataBundle data);

        /// <summary>
        /// Signals that a user's submitted score was fully processed.
        /// </summary>
        /// <param name="userId">The ID of the user who achieved the score.</param>
        /// <param name="scoreId">The ID of the score.</param>
        Task UserScoreProcessed(int userId, long scoreId);
    }
}
