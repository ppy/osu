// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Runtime.Serialization;
using Microsoft.AspNetCore.SignalR;

namespace osu.Game.Online.Multiplayer
{
    [Serializable]
    public class NotJoinedRoomException : HubException
    {
        public NotJoinedRoomException()
            : base("This user has not yet joined a multiplayer room.")
        {
        }

        protected NotJoinedRoomException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
