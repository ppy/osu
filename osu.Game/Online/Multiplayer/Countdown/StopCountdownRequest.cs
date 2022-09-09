// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using MessagePack;
using Newtonsoft.Json;

namespace osu.Game.Online.Multiplayer.Countdown
{
    /// <summary>
    /// Request to stop the current countdown.
    /// </summary>
    [MessagePackObject]
    public class StopCountdownRequest : MatchUserRequest
    {
        [Key(0)]
        public readonly int ID;

        [JsonConstructor]
        [SerializationConstructor]
        public StopCountdownRequest(int id)
        {
            ID = id;
        }
    }
}
