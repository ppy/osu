// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

namespace osu.Game.Online.RealtimeMultiplayer
{
    [Serializable]
    public class MultiplayerRoom
    {
        public long RoomID { get; set; }

        public MultiplayerRoomState State { get; set; }
    }
}
