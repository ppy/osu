// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

namespace osu.Game.Online.RealtimeMultiplayer
{
    public class InvalidStateException : Exception
    {
        public InvalidStateException(string message)
            : base(message)
        {
        }
    }
}
