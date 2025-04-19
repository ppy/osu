// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

// Holds all Tournament information. Use this class to retrieve information to display.
// Instanced on joining a TournamentLounge

using System;
using System.Diagnostics;
using System.Collections.Generic;
using osu.Framework.Bindables;
using System.Text.Json.Serialization;
using osu.Game.Rulesets;
using osu.Game.Screens.OnlinePlay.Tournaments.Models;
using osu.Game.Screens.OnlinePlay.Tournaments.Tabs;

namespace osu.Game.Screens.OnlinePlay.Tournaments
{
    [Serializable]
    public class TournamentInfo
    {
        public int TournamentID;
        public string TournamentName { get; set; } = string.Empty;

        [JsonIgnore]
        public readonly Bindable<TournamentsTab> CurrentTabType = new Bindable<TournamentsTab>(TournamentsTab.Info);

        public readonly Bindable<TournamentsTabs> VisibleTabs = new Bindable<TournamentsTabs>(TournamentsTabs.None);

        [JsonIgnore]
        public readonly BindableBool IsEditing = new BindableBool(false);

        /// <summary>
        /// The largest number of players allowed in a team
        /// </summary>
        public Bindable<int> MaxTeamSize = new BindableInt(8);

        /// <summary>
        /// The smallest number of players allowed in a team
        /// </summary>
        public Bindable<int> MinTeamSize = new BindableInt(1)
        {
            MinValue = 1
        };

        /// <summary>
        /// The maximum amount of players from a team playing in a match
        /// </summary>
        public Bindable<int> MaxTeamPlayers = new BindableInt(4)
        {
            MinValue = 1
        };

        // TAKEN FROM LadderInfo.cs

        public Bindable<RulesetInfo?> Ruleset = new Bindable<RulesetInfo?>();

        public BindableList<TournamentMatch> Matches = new BindableList<TournamentMatch>();
        public BindableList<TournamentRound> Rounds = new BindableList<TournamentRound>();
        public BindableList<TournamentTeam> Teams = new BindableList<TournamentTeam>();

        // only used for serialisation
        public List<TournamentProgression> Progressions = new List<TournamentProgression>();

        [JsonIgnore] // updated manually in TournamentGameBase
        public Bindable<TournamentMatch?> CurrentMatch = new Bindable<TournamentMatch?>();

        public Bindable<int> ChromaKeyWidth = new BindableInt(1024)
        {
            MinValue = 640,
            MaxValue = 1366,
        };

        public Bindable<int> PlayersPerTeam = new BindableInt(4)
        {
            MinValue = 3,
            MaxValue = 4,
        };

        public Bindable<bool> AutoProgressScreens = new BindableBool(true);

        public Bindable<bool> SplitMapPoolByMods = new BindableBool(true);

        public Bindable<bool> DisplayTeamSeeds = new BindableBool();

        // IMPORT END

        public TournamentInfo()
        {
            VisibleTabs.BindValueChanged(tab =>
            {
                // Don't know if hiding all tabs should be allowed. Just assert for now.
                Debug.Assert(VisibleTabs.Value != TournamentsTabs.None);
                Debug.Assert((uint)VisibleTabs.Value <= (uint)TournamentsTabs.All);

                // If the current tab was hidden.
                if ((~tab.NewValue & tab.OldValue).HasFlag(CurrentTabType.Value.AsFlag()))
                {
                    // If current tab was hidden, select its right tab as current if able, otherwise select its left.
                    uint rights = (uint)VisibleTabs.Value & ~((uint)CurrentTabType.Value - 1);
                    uint right = rights & (~rights + 1);
                    CurrentTabType.Value = right != 0 ? (TournamentsTab)right : VisibleTabs.Value.GetLast();
                }
            });
        }

        /// <summary>
        /// Sets the visibility of the local client's footer tabs
        /// </summary>
        /// <param name="tab"></param>
        /// <param name="visibility"></param>
        public void SetTabVisibility(TournamentsTab tab, bool visibility)
        {
            VisibleTabs.Value = visibility ? VisibleTabs.Value | tab.AsFlag() : VisibleTabs.Value & ~tab.AsFlag();
        }
    }
}
