// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using MessagePack;
using Newtonsoft.Json;

namespace osu.Game.Online.Multiplayer.Countdown
{
    /// <summary>
    /// Indicates that a countdown was stopped in the <see cref="MultiplayerRoom"/>.
    /// </summary>
    [MessagePackObject]
    public class CountdownStoppedEvent : MatchServerEvent
    {
        /// <summary>
        /// The identifier of the countdown that was stopped.
        /// </summary>
        [Key(0)]
        public readonly int ID;

        [JsonConstructor]
        [SerializationConstructor]
        public CountdownStoppedEvent(int id)
        {
            ID = id;
        }
    }
}
