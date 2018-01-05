// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.ComponentModel;

namespace osu.Game.Beatmaps
{
    public enum RankStatus
    {
        Any = 7,
        [Description("Ranked & Approved")]
        RankedApproved = 0,
        Approved = 1,
        Loved = 8,
        Favourites = 2,
        [Description("Mod Requests")]
        ModRequests = 3,
        Pending = 4,
        Graveyard = 5,
        [Description("My Maps")]
        MyMaps = 6,
    }
}
