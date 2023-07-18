// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;

namespace osu.Game.Online.Rooms
{
    public enum RoomAvailability
    {
        Public,

        [Description(@"Friends Only")]
        FriendsOnly,

        [Description(@"Invite Only")]
        InviteOnly,
    }
}
