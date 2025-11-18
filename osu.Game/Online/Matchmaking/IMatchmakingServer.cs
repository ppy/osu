// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading.Tasks;

namespace osu.Game.Online.Matchmaking
{
    public interface IMatchmakingServer
    {
        /// <summary>
        /// Retrieves all active matchmaking pools.
        /// </summary>
        Task<MatchmakingPool[]> GetMatchmakingPools();

        /// <summary>
        /// Joins the matchmaking lobby, allowing the local user to receive status updates.
        /// </summary>
        Task MatchmakingJoinLobby();

        /// <summary>
        /// Leaves the matchmaking lobby.
        /// </summary>
        Task MatchmakingLeaveLobby();

        /// <summary>
        /// Joins the matchmaking queue, allowing the local user to get matched up with others.
        /// </summary>
        Task MatchmakingJoinQueue(int poolId);

        /// <summary>
        /// Leaves the matchmaking queue.
        /// </summary>
        Task MatchmakingLeaveQueue();

        /// <summary>
        /// Accepts a matchmaking room invitation.
        /// </summary>
        Task MatchmakingAcceptInvitation();

        /// <summary>
        /// Declines a matchmaking room invitation.
        /// </summary>
        Task MatchmakingDeclineInvitation();

        /// <summary>
        /// Raise a candidate playlist item to be played in the current round.
        /// </summary>
        /// <param name="playlistItemId">The playlist item, or -1 to indicate a random selection.</param>
        Task MatchmakingToggleSelection(long playlistItemId);

        /// <summary>
        /// Debug only - skips to the next stage of the matchmaking room.
        /// </summary>
        Task MatchmakingSkipToNextStage();
    }
}
