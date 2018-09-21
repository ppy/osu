// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

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

        public readonly Bindable<TournamentTeam> Team1 = new Bindable<TournamentTeam>();

        public readonly Bindable<int?> Team1Score = new Bindable<int?>();

        public readonly Bindable<TournamentTeam> Team2 = new Bindable<TournamentTeam>();

        public readonly Bindable<int?> Team2Score = new Bindable<int?>();

        public readonly Bindable<bool> Completed = new Bindable<bool>();

        [JsonIgnore]
        public readonly Bindable<MatchPairing> Progression = new Bindable<MatchPairing>();

        [JsonIgnore]
        public readonly Bindable<MatchPairing> ProgressionSource = new Bindable<MatchPairing>();

        [JsonProperty]
        public Point Position;

        private MatchPairing lastProgression; // todo: fix if we ever get LastValue inside Bindable<>.

        public MatchPairing()
        {
            Progression.ValueChanged += progression =>
            {
                if (lastProgression != null)
                    lastProgression.ProgressionSource.Value = null;

                if (progression != null)
                    progression.ProgressionSource.Value = this;

                lastProgression = progression;
            };
        }

        public MatchPairing(TournamentTeam team1 = null, TournamentTeam team2 = null)
        {
            Team1.Value = team1;
            Team2.Value = team2;
        }

        [JsonIgnore]
        public TournamentTeam Winner => !Completed.Value ? null : Team1Score.Value > Team2Score.Value ? Team1.Value : Team2.Value;

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
