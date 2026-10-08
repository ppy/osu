// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Threading.Tasks;

namespace osu.Game.Online.Spectator
{
    /// <summary>
    /// An interface defining the spectator server instance.
    /// </summary>
    public interface ISpectatorServer
    {
        #region V1 (obsoleted)

        /// <summary>
        /// Signal the start of a new play session.
        /// </summary>
        /// <param name="scoreToken">The score submission token.</param>
        /// <param name="state">The state of gameplay.</param>
        [Obsolete("New clients should use BeginPlaySessionV2.")] // can remove method 20270102
        Task BeginPlaySession(long? scoreToken, SpectatorState state);

        /// <summary>
        /// Send a bundle of frame data for the current play session.
        /// </summary>
        /// <param name="data">The frame data.</param>
        [Obsolete("New clients should use SendFrameDataV2.")] // can remove method 20270102
        Task SendFrameData(FrameDataBundle data);

        /// <summary>
        /// Signal the end of a play session.
        /// </summary>
        /// <param name="state">The state of gameplay.</param>
        [Obsolete("New clients should use EndPlaySessionV2.")] // can remove method 20270102
        Task EndPlaySession(SpectatorState state);

        #endregion

        #region V2

        /// <summary>
        /// Signal the start of a new play session.
        /// </summary>
        /// <param name="scoreToken">The score submission token.</param>
        /// <param name="state">The state of gameplay.</param>
        Task BeginPlaySessionV2(long? scoreToken, SpectatorState state);

        /// <summary>
        /// Send a bundle of frame data for the current play session.
        /// </summary>
        /// <param name="scoreToken">The score submission token.</param>
        /// <param name="data">The frame data.</param>
        Task SendFrameDataV2(long? scoreToken, FrameDataBundle data);

        /// <summary>
        /// Signal the end of a play session.
        /// </summary>
        /// <param name="scoreToken">The score submission token.</param>
        /// <param name="finalState">The final state of gameplay.</param>
        Task EndPlaySessionV2(long? scoreToken, SpectatedUserState finalState);

        #endregion

        /// <summary>
        /// Request spectating data for the specified user. May be called on multiple users and offline users.
        /// For offline users, a subscription will be created and data will begin streaming on next play.
        /// </summary>
        /// <param name="userId">The user to subscribe to.</param>
        Task StartWatchingUser(int userId);

        /// <summary>
        /// Stop requesting spectating data for the specified user. Unsubscribes from receiving further data.
        /// </summary>
        /// <param name="userId">The user to unsubscribe from.</param>
        Task EndWatchingUser(int userId);
    }
}
