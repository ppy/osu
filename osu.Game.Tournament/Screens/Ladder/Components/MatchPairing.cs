// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using osu.Framework.Configuration;
using osu.Game.Tournament.Components;
using SixLabors.Primitives;

namespace osu.Game.Tournament.Screens.Ladder.Components
{
    /// <summary>
    /// A collection of two teams competing in a head-to-head match.
    /// </summary>
    public class MatchPairing
    {
        public int ID;

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
        public readonly Bindable<TournamentGrouping> Grouping = new Bindable<TournamentGrouping>();

        [JsonIgnore]
        public readonly Bindable<MatchPairing> Progression = new Bindable<MatchPairing>();

        [JsonIgnore]
        public readonly Bindable<MatchPairing> LosersProgression = new Bindable<MatchPairing>();

        /// <summary>
        /// Should not be set directly. Use LadderInfo.CurrentMatch.Value = this instead.
        /// </summary>
        public readonly Bindable<bool> Current = new Bindable<bool>();

        public readonly Bindable<DateTimeOffset> Date = new Bindable<DateTimeOffset>();

        public Point Position;

        public MatchPairing()
        {
            Team1.BindValueChanged(t => Team1Acronym = t?.Acronym, true);
            Team2.BindValueChanged(t => Team2Acronym = t?.Acronym, true);
        }

        public MatchPairing(TournamentTeam team1 = null, TournamentTeam team2 = null)
            : this()
        {
            Team1.Value = team1;
            Team2.Value = team2;
        }

        [JsonIgnore]
        public TournamentTeam Winner => !Completed.Value ? null : Team1Score.Value > Team2Score.Value ? Team1.Value : Team2.Value;

        [JsonIgnore]
        public TournamentTeam Loser => !Completed.Value ? null : Team1Score.Value > Team2Score.Value ? Team2.Value : Team1.Value;

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
    }
}
