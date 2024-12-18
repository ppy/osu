// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using Microsoft.AspNetCore.SignalR;

namespace osu.Game.Online.Multiplayer
{
    [Serializable]
    public class UserBlocksPMsException : HubException
    {
        public const string MESSAGE = "Cannot perform action because user has disabled non-friend communications.";

        public UserBlocksPMsException()
            : base(MESSAGE)
        {
        }
    }
}
