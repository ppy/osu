// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Online.Multiplayer
{
    public enum MultiplayerUserState
    {
        /// <summary>
        /// The user is idle and waiting for something to happen (or watching the match but not participating).
        /// </summary>
        Idle,

        /// <summary>
        /// The user has marked themselves as ready to participate and should be considered for the next game start.
        /// </summary>
        /// <remarks>
        /// Clients in this state will receive gameplay channel messages.
        /// As a client the only thing to look for in this state is a <see cref="IMultiplayerClient.LoadRequested"/> call.
        /// </remarks>
        Ready,

        /// <summary>
        /// The server is waiting for this user to finish loading. This is a reserved state, and is set by the server.
        /// </summary>
        /// <remarks>
        /// All users in <see cref="Ready"/> state when the game start will be transitioned to this state.
        /// All users in this state need to transition to <see cref="Loaded"/> before the game can start.
        /// </remarks>
        WaitingForLoad,

        /// <summary>
        /// The user has marked itself as loaded, but may still be adjusting settings prior to being ready for gameplay.
        /// Players remaining in this state for an extended period of time will be automatically transitioned to the <see cref="Playing"/> state by the server.
        /// </summary>
        Loaded,

        /// <summary>
        /// The user has finished adjusting settings and is ready to start gameplay.
        /// </summary>
        ReadyForGameplay,

        /// <summary>
        /// The user is currently playing in a game. This is a reserved state, and is set by the server.
        /// </summary>
        /// <remarks>
        /// Once there are no remaining <see cref="WaitingForLoad"/> users, all users in <see cref="Loaded"/> state will be transitioned to this state.
        /// At this point the game will start for all users.
        /// </remarks>
        Playing,

        /// <summary>
        /// The user has finished playing and is ready to view results.
        /// </summary>
        /// <remarks>
        /// Once all users transition from <see cref="Playing"/> to this state, the game will end and results will be distributed.
        /// All users will be transitioned to the <see cref="Results"/> state.
        /// </remarks>
        FinishedPlay,

        /// <summary>
        /// The user is currently viewing results. This is a reserved state, and is set by the server.
        /// </summary>
        Results,

        /// <summary>
        /// The user is currently spectating this room.
        /// </summary>
        Spectating
    }
}
