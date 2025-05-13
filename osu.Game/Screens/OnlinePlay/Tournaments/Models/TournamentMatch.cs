// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using Newtonsoft.Json;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;

namespace osu.Game.Screens.OnlinePlay.Tournaments.Models
{
    /// <summary>
    /// A collection of two teams competing in a head-to-head match.
    /// </summary>
    [Serializable]
    public class TournamentMatch
    {
        public int ID;

        public List<string> Acronyms
        {
            get
            {
                List<string> acronyms = new List<string>();

                foreach (var team in Teams)
                {
                    if (team?.Acronym != null) acronyms.Add(team.Acronym.Value);
                }

                return acronyms;
            }
        }

        /// <summary>
        ///
        /// </summary>
        [JsonIgnore]
        public readonly BindableList<TournamentTeam?> Teams = [null, null];

        /// <summary>
        ///
        /// </summary>
        public readonly List<Bindable<int?>> TeamScores = new List<Bindable<int?>>();

        /// <summary>
        /// Index in Teams where the the team and following teams lost the match.
        /// LoserTeams = 1 would mean Teams[1] until end of list lost the match.
        /// </summary>
        public readonly Bindable<int?> LoserTeams = new Bindable<int?>();

        public readonly Bindable<bool> Completed = new Bindable<bool>();

        public readonly Bindable<bool> Losers = new Bindable<bool>();

        public readonly ObservableCollection<BeatmapChoice> PicksBans = new ObservableCollection<BeatmapChoice>();

        [JsonIgnore]
        public readonly Bindable<TournamentRound?> Round = new Bindable<TournamentRound?>();

        [JsonIgnore]
        public readonly Bindable<TournamentMatch?> Progression = new Bindable<TournamentMatch?>();

        [JsonIgnore]
        public readonly Bindable<TournamentMatch?> LosersProgression = new Bindable<TournamentMatch?>();

        /// <summary>
        /// Should not be set directly. Use LadderInfo.CurrentMatch.Value = this instead.
        /// </summary>
        public readonly Bindable<bool> Current = new Bindable<bool>();

        public readonly Bindable<DateTimeOffset> Date = new Bindable<DateTimeOffset>(DateTimeOffset.Now);

        [JsonProperty]
        public readonly BindableList<ConditionalTournamentMatch> ConditionalMatches = new BindableList<ConditionalTournamentMatch>();

        public readonly Bindable<Point> Position = new Bindable<Point>();

        public TournamentMatch()
        {
            Teams.BindCollectionChanged((temp1, temp2) => createTeamScores(), true);
        }

        public TournamentMatch(ICollection<TournamentTeam> teams)
        {
            Teams.Clear();
            Teams.AddRange(teams);
            if (Teams.Count < 2)
                Teams.AddRange(Enumerable.Repeat<TournamentTeam?>(null, 2 - Teams.Count));
            Teams.BindCollectionChanged((temp1, temp2) => createTeamScores(), true);
        }

        private void createTeamScores()
        {
            TeamScores.ForEach(score => score.UnbindAll());
            TeamScores.Clear();
            Teams.ForEach(_ => TeamScores.Add(new Bindable<int?>(null)));
        }

        public List<TournamentTeam> GetWinners()
        {
            List<TournamentTeam> winners = [];

            for (int i = 0; i < (LoserTeams.Value ?? 0); i++)
            {
                TournamentTeam? element; // Variable used to get around possible null warning

                if ((element = Teams.ElementAt(i)) != null)
                {
                    winners.Add(element);
                }
            }

            return winners;
        }

        public List<TournamentTeam> GetLosers()
        {
            List<TournamentTeam> losers = [];

            for (int i = LoserTeams.Value ?? Teams.Count; i < Teams.Count; i++)
            {
                TournamentTeam? element; // Variable used to get around possible null warning

                if ((element = Teams.ElementAt(i)) != null)
                {
                    losers.Add(element);
                }
            }

            return losers;
        }

        // todo how should team colour work with multiple teams per match, potentially dozens
        public TeamColour WinnerColour => TeamColour.Red;

        // todo : Should be replaced with a function that can support multiple formats and number of teams
        public int PointsToWin => Round.Value?.BestOf.Value / 2 + 1 ?? 0;

        /// <summary>
        /// Remove scores from the match, in case of a false click or false start.
        /// </summary>
        public void CancelMatchStart()
        {
            foreach (var score in TeamScores)
            {
                score.Value = null;
            }
        }

        /// <summary>
        /// Initialise this match with zeroed scores. Will be a noop if either team is not present or if either of the scores are non-zero.
        /// </summary>
        public void StartMatch()
        {
            if (Teams.Any(team => team != null))
                return;

            if (TeamScores.Any(score => score.Value != null))
                return;

            foreach (var score in TeamScores)
            {
                score.Value = 0;
            }
        }

        public void Reset()
        {
            CancelMatchStart();
            Teams.ReplaceRange(0, Teams.Count, Enumerable.Repeat<TournamentTeam?>(null, Teams.Count));
            Completed.Value = false;
            PicksBans.Clear();
        }
    }

    // todo : Should also add Walkover match
    /// <summary>
    /// A match that may not necessarily occur.
    /// </summary>
    public class ConditionalTournamentMatch : TournamentMatch
    {
    }
}
