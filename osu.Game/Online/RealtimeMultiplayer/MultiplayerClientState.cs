// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

#nullable enable

namespace osu.Game.Online.RealtimeMultiplayer
{
    [Serializable]
    public class MultiplayerClientState
    {
        public readonly long CurrentRoomID;

        public MultiplayerClientState(in long roomId)
        {
            CurrentRoomID = roomId;
        }
    }
}
