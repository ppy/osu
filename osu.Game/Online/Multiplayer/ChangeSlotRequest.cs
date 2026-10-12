// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using MessagePack;

namespace osu.Game.Online.Multiplayer
{
    /// <summary>
    /// User requests to change their slot in the room.
    /// </summary>
    [MessagePackObject]
    public class ChangeSlotRequest : MatchUserRequest
    {
        /// <summary>
        /// The zero-based ID of the desired slot.
        /// </summary>
        [Key(0)]
        public byte SlotID { get; set; }
    }
}
