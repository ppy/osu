// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

// Holds all Tournament information. Use this class to retrieve information to display.
// Instanced on joining a TournamentLounge

using System;
using System.Linq;
using System.Collections.Generic;

namespace osu.Game.Screens.OnlinePlay.Tournaments
{
    public class TournamentInfo
    {
        public int TournamentID;
        public string TournamentName { get; set; } = string.Empty;

        public Action<TournamentsTabs, bool>? UpdateTabVisibility;

        // TODO SHOULD BE ZERO, USING FOR TESTING.
        private uint tabsVisibility = 17u;

        public bool GetTabVisibility(TournamentsTabs tab)
        {
            return ((tabsVisibility & (1u << (int)tab)) >> (int)tab) == 1;
        }

        public IEnumerable<TournamentsTabs> GetTabsVisibility()
        {
            return
                from TournamentsTabs tab in Enum.GetValues(typeof(TournamentsTabs))
                where GetTabVisibility(tab)
                select tab;
        }

        public void SetTabVisibility(TournamentsTabs tab, bool visibility)
        {
            bool visibilityChanged = visibility != GetTabVisibility(tab);
            tabsVisibility = visibility ? tabsVisibility | (1u << (int)tab) : tabsVisibility & ~(1u << (int)tab);
            // Only updates when visiiblity updates, might be redundant
            if (visibilityChanged) UpdateTabVisibility?.Invoke(tab, visibility);
        }
    }
}




