// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using osu.Framework.Bindables;
using osu.Game.Tournament.Screens.Ladder.Components;
using SixLabors.Primitives;

namespace osu.Game.Tournament.Models
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
                if (Team1Acronym != null) acronyms.Add(Team1Acronym);
                if (Team2Acronym != null) acronyms.Add(Team2Acronym);
                return acronyms;
            }
        }

        [JsonIgnore]
        public readonly Bindable<TournamentTeam> Team1 = new Bindable<TournamentTeam>();

        public string Team1Acronym;

        public readonly Bindable<int?> Team1Score = new Bindable<int?>();

        [JsonIgnore]
        public readonly Bindable<TournamentTeam> Team2 = new Bindable<TournamentTeam>();

        public string Team2Acronym;

        public readonly Bindable<int?> Team2Score = new Bindable<int?>();

        public readonly Bindable<bool> Completed = new Bindable<bool>();

        public readonly Bindable<bool> Losers = new Bindable<bool>();

        public readonly ObservableCollection<BeatmapChoice> PicksBans = new ObservableCollection<BeatmapChoice>();

        [JsonIgnore]
        public readonly Bindable<TournamentRound> Round = new Bindable<TournamentRound>();

        [JsonIgnore]
        public readonly Bindable<TournamentMatch> Progression = new Bindable<TournamentMatch>();

        [JsonIgnore]
        public readonly Bindable<TournamentMatch> LosersProgression = new Bindable<TournamentMatch>();

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
            Team1.BindValueChanged(t => Team1Acronym = t.NewValue?.Acronym.Value, true);
            Team2.BindValueChanged(t => Team2Acronym = t.NewValue?.Acronym.Value, true);
        }

        public TournamentMatch(TournamentTeam team1 = null, TournamentTeam team2 = null)
            : this()
        {
            Team1.Value = team1;
            Team2.Value = team2;
        }

        [JsonIgnore]
        public TournamentTeam Winner => !Completed.Value ? null : Team1Score.Value > Team2Score.Value ? Team1.Value : Team2.Value;

        [JsonIgnore]
        public TournamentTeam Loser => !Completed.Value ? null : Team1Score.Value > Team2Score.Value ? Team2.Value : Team1.Value;

        public int PointsToWin => Round.Value?.BestOf.Value / 2 + 1 ?? 0;

        /// <summary>
        /// Remove scores from the match, in case of a false click or false start.
        /// </summary>
        public void CancelMatchStart()
        {
            Team1Score.Value = null;
            Team2Score.Value = null;
        }

        /// <summary>
        /// Initialise this match with zeroed scores. Will be a noop if either team is not present.
        /// </summary>
        public void StartMatch()
        {
            if (Team1.Value == null || Team2.Value == null)
                return;

            Team1Score.Value = 0;
            Team2Score.Value = 0;
        }

        public void Reset()
        {
            CancelMatchStart();
            Team1.Value = null;
            Team2.Value = null;
            Completed.Value = false;
            PicksBans.Clear();
        }
    }
}
