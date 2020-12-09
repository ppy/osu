// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

namespace osu.Game.Online.RealtimeMultiplayer
{
    public class AlreadyInRoomException : Exception
    {
        public AlreadyInRoomException()
            : base("This user is already in a multiplayer room.")
        {
        }
    }
}
