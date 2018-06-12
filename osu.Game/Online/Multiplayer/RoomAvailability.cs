// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.ComponentModel;

namespace osu.Game.Online.Multiplayer
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
