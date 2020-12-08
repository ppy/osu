// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

namespace osu.Game.Online.RealtimeMultiplayer
{
    public class NotHostException : Exception
    {
        public NotHostException()
            : base("User is attempting to perform a host level operation while not the host")
        {
        }
    }
}
