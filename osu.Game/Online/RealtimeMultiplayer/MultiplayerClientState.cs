// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

namespace osu.Game.Online.RealtimeMultiplayer
{
    public class MultiplayerClientState
    {
        public long CurrentRoomID { get; set; }

        public MultiplayerClientState(in long roomId)
        {
            CurrentRoomID = roomId;
        }
    }
}
