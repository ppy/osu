// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using MessagePack;

namespace osu.Game.Online.Multiplayer.Countdown
{
    /// <summary>
    /// Indicates a change to the <see cref="MultiplayerRoom"/>'s countdown.
    /// </summary>
    [MessagePackObject]
    public class CountdownChangedEvent : MatchServerEvent
    {
        /// <summary>
        /// The new countdown.
        /// </summary>
        [Key(0)]
        public MultiplayerCountdown? Countdown { get; set; }
    }
}
