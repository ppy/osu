// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

namespace osu.Game.IPC
{
    public class IPCTimeoutException : TimeoutException
    {
        public IPCTimeoutException(Type channelType)
            : base($@"IPC took too long to send message via channel {channelType}")
        {
        }
    }
}
