// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

// Holds all Tournament information. Use this class to retrieve information to display.
// Instanced on joining a TournamentLounge

using System;
using System.Linq;
using System.Collections.Generic;
using osu.Framework.Logging;
using osu.Framework.Bindables;
using System.Text.Json.Serialization;
using osu.Game.Rulesets;
using osu.Game.Screens.OnlinePlay.Tournaments.Models;
using System.Drawing;

namespace osu.Game.Screens.OnlinePlay.Tournaments
{
    [Serializable]
    public class TournamentInfo
    {
        // todo : all mentions of this should be removed in final product
        public const bool IS_TESTING = true;

        public int TournamentID;
        public string TournamentName { get; set; } = string.Empty;

        public Action<TournamentsTabs, bool>? UpdateTabVisibility;

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

        [JsonIgnore]
        private uint tabsVisibility = 0u;

        [JsonIgnore]
        public BindableBool IsEditing = new BindableBool(false);

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
            if (IS_TESTING)
                testTournament1();
        }

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

        // Sets the visibility of the local clients footer tabs
        public void SetTabVisibility(TournamentsTabs tab, bool visibility)
        {
            bool visibilityChanged = visibility != GetTabVisibility(tab);
            Logger.Log("Visibility before " + tabsVisibility.ToString());
            tabsVisibility = visibility ? tabsVisibility | (1u << (int)tab) : tabsVisibility & ~(1u << (int)tab);
            Logger.Log("Visibility after " + tabsVisibility.ToString());
            // Only updates when visiiblity updates, might be redundant
            if (visibilityChanged) UpdateTabVisibility?.Invoke(tab, visibility);
        }

        private void testTournament1()
        {
            tabsVisibility = 17u;
            IsEditing.Value = true;

            List<TournamentUser> players = [];
            foreach (int id in Enumerable.Range(1, 8))
            {
                players.Add(new TournamentUser() { OnlineID = id });
                // PopulatePlayer(players.Last(), success: () => Console.WriteLine("Successfully populated player."), immediate: true);
            }

            foreach (int i in Enumerable.Range(0, 4))
            {
                Teams.Add(new TournamentTeam(players.GetRange(i, 2))
                {
                    FullName = { Value = "Team" + i.ToString() },
                    FlagName = { Value = "NO" },
                    Acronym = { Value = "T" + i.ToString() },
                    ID = i,
                });
            }

            foreach (int i in Enumerable.Range(0, 3))
            {
                Matches.Add(new TournamentMatch([Teams[i], Teams[i + 1]]));
                Matches.Last().Position.Value = new Point(i * 240, 0);
            }
        }
    }
}




