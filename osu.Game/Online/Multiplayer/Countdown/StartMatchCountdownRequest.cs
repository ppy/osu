// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using MessagePack;

namespace osu.Game.Online.Multiplayer.Countdown
{
    /// <summary>
    /// A request for a countdown to start the match.
    /// </summary>
    [MessagePackObject]
    public class StartMatchCountdownRequest : MatchUserRequest
    {
        /// <summary>
        /// How long the countdown should last.
        /// </summary>
        [Key(0)]
        public TimeSpan Duration { get; set; }
    }
}
