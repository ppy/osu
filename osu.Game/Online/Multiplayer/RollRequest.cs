// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using MessagePack;

namespace osu.Game.Online.Multiplayer
{
    /// <summary>
    /// Requests a random roll of a number from 1 to <see cref="Max"/> inclusive.
    /// </summary>
    [Serializable]
    [MessagePackObject]
    public class RollRequest : MatchUserRequest
    {
        /// <summary>
        /// Determines the maximum possible result of the roll.
        /// Must be bigger than 1.
        /// Defaults to 100 if not provided.
        /// </summary>
        [Key(0)]
        public uint? Max { get; set; }
    }
}
