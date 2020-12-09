// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

namespace osu.Game.Online.RealtimeMultiplayer
{
    public class NotJoinedRoomException : Exception
    {
        public NotJoinedRoomException()
            : base("This user has not yet joined a multiplayer room.")
        {
        }
    }
}
