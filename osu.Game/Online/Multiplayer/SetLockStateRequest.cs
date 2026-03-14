// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using MessagePack;

namespace osu.Game.Online.Multiplayer
{
    [MessagePackObject]
    public class SetLockStateRequest : MatchUserRequest
    {
        /// <summary>
        /// <para>
        /// If <see langword="true"/>, <see cref="MultiplayerRoomUserRole.Player"/>s will not be able to change teams by themselves in the room,
        /// only <see cref="MultiplayerRoomUserRole.Referee"/>s will be able to change teams for the <see cref="MultiplayerRoomUserRole.Player"/>s.
        /// </para>
        /// <para>
        /// If <see langword="false"/>, any user can change their team in the room.
        /// </para>
        /// </summary>
        // TODO: mention slots as well when slots are reimplemented
        [Key(0)]
        public bool Locked { get; set; }
    }
}
