// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using MessagePack;
using osu.Game.Online.Multiplayer.Countdown;

namespace osu.Game.Online.Multiplayer
{
    /// <summary>
    /// Describes the current countdown in a <see cref="MultiplayerRoom"/>.
    /// </summary>
    [MessagePackObject]
    [Union(0, typeof(MatchStartCountdown))] // IMPORTANT: Add rules to SignalRUnionWorkaroundResolver for new derived types.
    [Union(1, typeof(ForceGameplayStartCountdown))]
    public abstract class MultiplayerCountdown
    {
        /// <summary>
        /// The amount of time remaining in the countdown.
        /// </summary>
        /// <remarks>
        /// This is only sent once from the server upon initial retrieval of the <see cref="MultiplayerRoom"/> or via a <see cref="CountdownChangedEvent"/>.
        /// </remarks>
        [Key(0)]
        public TimeSpan TimeRemaining { get; set; }
    }
}
