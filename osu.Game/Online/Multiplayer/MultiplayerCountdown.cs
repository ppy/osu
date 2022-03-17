// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using System;
using MessagePack;

namespace osu.Game.Online.Multiplayer
{
    /// <summary>
    /// Describes the current countdown in a <see cref="MultiplayerRoom"/>.
    /// </summary>
    [MessagePackObject]
    [Union(0, typeof(MatchStartCountdown))] // IMPORTANT: Add rules to SignalRUnionWorkaroundResolver for new derived types.
    public abstract class MultiplayerCountdown
    {
        /// <summary>
        /// The time at which the countdown will end.
        /// </summary>
        [Key(0)]
        public DateTimeOffset EndTime { get; set; }
    }
}
