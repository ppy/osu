// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Runtime.Serialization;
using Microsoft.AspNetCore.SignalR;

namespace osu.Game.Online.Multiplayer
{
    [Serializable]
    public class InvalidStateException : HubException
    {
        public InvalidStateException(string message)
            : base(message)
        {
        }

        protected InvalidStateException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
