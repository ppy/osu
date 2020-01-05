// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;

namespace osu.Game.Online.Multiplayer
{
    public enum RoomAvailability
    {
        [Description(@"公开")]
        Public,

        [Description(@"仅限好友")]
        FriendsOnly,

        [Description(@"仅限邀请")]
        InviteOnly,
    }
}
