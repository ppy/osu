// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

namespace osu.Game.Online.RealtimeMultiplayer
{
    /// <summary>
    /// The current overall state of a realtime multiplayer room.
    /// </summary>
    public enum MultiplayerRoomState
    {
        Open,
        WaitingForLoad,
        Playing,
        WaitingForResults,
        Closed
    }
}
