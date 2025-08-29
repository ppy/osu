// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using MessagePack;
using osu.Game.Online.Matchmaking;
using osu.Game.Online.Multiplayer.Countdown;

namespace osu.Game.Online.Multiplayer
{
    /// <summary>
    /// Describes the current countdown in a <see cref="MultiplayerRoom"/>.
    /// </summary>
    [MessagePackObject]
    [Union(0, typeof(MatchStartCountdown))] // IMPORTANT: Add rules to SignalRUnionWorkaroundResolver for new derived types.
    [Union(1, typeof(ForceGameplayStartCountdown))]
    [Union(2, typeof(ServerShuttingDownCountdown))]
    [Union(3, typeof(MatchmakingStageCountdown))]
    public abstract class MultiplayerCountdown
    {
        /// <summary>
        /// A unique identifier for this countdown.
        /// </summary>
        [Key(0)]
        public int ID { get; set; }

        /// <summary>
        /// The amount of time remaining in the countdown.
        /// </summary>
        /// <remarks>
        /// This is only sent once from the server upon initial retrieval of the <see cref="MultiplayerRoom"/> or via a <see cref="CountdownStartedEvent"/>.
        /// </remarks>
        [Key(1)]
        public TimeSpan TimeRemaining { get; set; }

        /// <summary>
        /// Whether only a single instance of this <see cref="MultiplayerCountdown"/> type may be active at any one time.
        /// </summary>
        [IgnoreMember]
        public virtual bool IsExclusive => true;
    }
}
