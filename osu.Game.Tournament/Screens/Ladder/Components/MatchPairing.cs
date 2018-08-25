// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Configuration;
using osu.Game.Tournament.Screens.Drawings.Components;

namespace osu.Game.Tournament.Screens.Ladder.Components
{
    /// <summary>
    /// A collection of two teams competing in a head-to-head match.
    /// </summary>
    public class MatchPairing
    {
        public Bindable<TournamentTeam> Team1 = new Bindable<TournamentTeam>();
        public Bindable<int?> Team1Score = new Bindable<int?>();

        public Bindable<TournamentTeam> Team2 = new Bindable<TournamentTeam>();
        public Bindable<int?> Team2Score = new Bindable<int?>();

        public Bindable<bool> Completed = new Bindable<bool>();

        public MatchPairing(TournamentTeam team1 = null, TournamentTeam team2 = null)
        {
            Team1.Value = team1;
            Team2.Value = team2;

            Team1Score.ValueChanged += _ => Completed.Value = false;
            Team2Score.ValueChanged += _ => Completed.Value = false;
        }
    }
}
