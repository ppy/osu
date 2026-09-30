// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using MessagePack;

namespace osu.Game.Online.Multiplayer
{
    /// <summary>
    /// Communicates the result of a <see cref="RollRequest"/>.
    /// </summary>
    [Serializable]
    [MessagePackObject]
    public class RollEvent : MatchServerEvent
    {
        /// <summary>
        /// The ID of the user who initiated the roll.
        /// </summary>
        [Key(0)]
        public int UserID { get; set; }

        /// <summary>
        /// Determines the maximum possible result of the roll.
        /// Bigger than 1.
        /// </summary>
        [Key(1)]
        public uint Max { get; set; }

        /// <summary>
        /// The actual result of the roll.
        /// In the range [1, <see cref="Max"/>], inclusive both ends.
        /// </summary>
        [Key(2)]
        public uint Result { get; set; }
    }
}
