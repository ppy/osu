// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using MessagePack;

namespace osu.Game.Online.Multiplayer
{
    /// <summary>
    /// A countdown that indicates the current multiplayer server is shutting down.
    /// </summary>
    [MessagePackObject]
    public class ServerShuttingDownCountdown : MultiplayerCountdown
    {
        /// <summary>
        /// If this is the final notification, no more <see cref="ServerShuttingDownCountdown"/> events will be sent after this.
        /// </summary>
        [Key(2)]
        public bool FinalNotification { get; set; }
    }
}
