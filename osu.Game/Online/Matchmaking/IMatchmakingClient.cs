// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading.Tasks;

namespace osu.Game.Online.Matchmaking
{
    public interface IMatchmakingClient : IStatefulUserHubClient
    {
        /// <summary>
        /// Signals that the local user was placed in the matchmaking queue.
        /// </summary>
        Task MatchmakingQueueJoined();

        /// <summary>
        /// Signals that the local user was removed from the matchmaking queue.
        /// </summary>
        Task MatchmakingQueueLeft();

        /// <summary>
        /// Signals that a match has been found and the local user is invited to it.
        /// The invitation may be <see cref="IMatchmakingServer.MatchmakingAcceptInvitation">accepted</see>,
        /// <see cref="IMatchmakingServer.MatchmakingDeclineInvitation">declined</see>,
        /// or ignored - in which case it will automatically be declined after a short timeout period.
        /// </summary>
        Task MatchmakingRoomInvited();

        /// <summary>
        /// Signals that the matchmaking room is ready to be opened.
        /// </summary>
        Task MatchmakingRoomReady(long roomId, string password);

        /// <summary>
        /// The matchmaking lobby status has changed.
        /// </summary>
        Task MatchmakingLobbyStatusChanged(MatchmakingLobbyStatus status);

        /// <summary>
        /// The matchmaking status of the current user has changed.
        /// </summary>
        Task MatchmakingQueueStatusChanged(MatchmakingQueueStatus status);

        /// <summary>
        /// The user has raised a candidate playlist item to be played.
        /// </summary>
        Task MatchmakingItemSelected(int userId, long playlistItemId);

        /// <summary>
        /// The user has removed a candidate playlist item.
        /// </summary>
        Task MatchmakingItemDeselected(int userId, long playlistItemId);
    }
}
