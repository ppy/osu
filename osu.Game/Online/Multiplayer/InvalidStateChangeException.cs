// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using Microsoft.AspNetCore.SignalR;

namespace osu.Game.Online.Multiplayer
{
    [Serializable]
    public class InvalidStateChangeException : HubException
    {
        public InvalidStateChangeException(MultiplayerUserState oldState, MultiplayerUserState newState)
            : base($"Cannot change from {oldState} to {newState}")
        {
        }
    }
}
