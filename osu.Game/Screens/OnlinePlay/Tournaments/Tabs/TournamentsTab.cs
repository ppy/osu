// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.


using System;
using System.Diagnostics;

namespace osu.Game.Screens.OnlinePlay.Tournaments.Tabs
{
    public static class Extensions
    {
        public static TournamentsTab GetLast(this TournamentsTabs flags)
        {
            // Gets the most significant bit.
            int bit = 0;
            int num = (int)flags;
            while ((num >>= 1) != 0)
                bit++;
            return (TournamentsTab)(1 << bit);
        }

        public static TournamentsTabs AsFlag(this TournamentsTab tab)
        {
            Debug.Assert(Enum.IsDefined(tab), @"Tried to cast value to an enum that doesn't exist.");
            return (TournamentsTabs)(int)tab;
        }

        public static TournamentsTab AsTab(this TournamentsTabs tab)
        {
            Debug.Assert(Enum.IsDefined((TournamentsTab)(int)tab), @"Tried to cast value to an enum that doesn't exist.");
            return (TournamentsTab)(int)(tab & ~(tab + 1)); // Still gives a valid enum.
        }
    }

    public enum TournamentsTab
    {
        Info = 1,
        Players = 2,
        Qualifiers = 4,
        Mappools = 8,
        Results = 16,
        Schedule = 32,
        Settings = 64,
    }

    [Flags]
    public enum TournamentsTabs
    {
        None = 0,
        Info = 1,
        Players = 2,
        Qualifiers = 4,
        Mappools = 8,
        Results = 16,
        Schedule = 32,
        Settings = 64,
        All = 127,
    }
}
