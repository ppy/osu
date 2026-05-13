// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using MessagePack;

namespace osu.Game.Online.Multiplayer
{
    [MessagePackObject]
    public class StandardMatchRoomState : MatchRoomState
    {
        /// <summary>
        /// Whether the room is currently locked.
        /// When locked, changes to slots (and teams, in team versus) cannot be performed by anyone but room referees.
        /// </summary>
        [Key(1)]
        public bool Locked { get; set; }

        /// <summary>
        /// The state of slots in the room.
        /// Linked to <see cref="MultiplayerRoomSettings.MaxParticipants"/>.
        /// <list type="bullet">
        /// <item>When <see cref="MultiplayerRoomSettings.MaxParticipants"/> is <see langword="null"/>, this property is also <see langword="null"/>.</item>
        /// <item>
        /// When <see cref="MultiplayerRoomSettings.MaxParticipants"/> is not <see langword="null"/>, this property is an array of that length.
        /// The items of that array represent either an empty slot (represented by <see langword="null"/>),
        /// or an user occupying that slot (represented by the ID of the relevant user).
        /// </item>
        /// </list>
        /// </summary>
        [Key(2)]
        public int?[]? Slots { get; set; }
    }
}
