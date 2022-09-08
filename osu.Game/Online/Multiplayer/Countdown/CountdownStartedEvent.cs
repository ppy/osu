// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using MessagePack;
using Newtonsoft.Json;

namespace osu.Game.Online.Multiplayer.Countdown
{
    /// <summary>
    /// Indicates that a countdown started in the <see cref="MultiplayerRoom"/>.
    /// </summary>
    [MessagePackObject]
    public class CountdownStartedEvent : MatchServerEvent
    {
        /// <summary>
        /// The countdown that was started.
        /// </summary>
        [Key(0)]
        public readonly MultiplayerCountdown Countdown;

        [JsonConstructor]
        [SerializationConstructor]
        public CountdownStartedEvent(MultiplayerCountdown countdown)
        {
            Countdown = countdown;
        }
    }
}
