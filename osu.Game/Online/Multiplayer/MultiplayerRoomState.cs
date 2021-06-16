// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

namespace osu.Game.Online.Multiplayer
{
    /// <summary>
    /// The current overall state of a multiplayer room.
    /// </summary>
    public enum MultiplayerRoomState
    {
        /// <summary>
        /// The room is open and accepting new players.
        /// </summary>
        Open,

        /// <summary>
        /// A game start has been triggered but players have not finished loading.
        /// </summary>
        WaitingForLoad,

        /// <summary>
        /// A game is currently ongoing.
        /// </summary>
        Playing,

        /// <summary>
        /// The room has been disbanded and closed.
        /// </summary>
        Closed
    }
}
