// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Game.Tournament.Components;

namespace osu.Game.Tournament.Screens.Ladder.Components
{
    public class LadderInfo
    {
        public List<MatchPairing> Pairings = new List<MatchPairing>();
        public List<TournamentProgression> Progressions = new List<TournamentProgression>();
        public List<TournamentGrouping> Groupings = new List<TournamentGrouping>();
        public List<TournamentTeam> Teams = new List<TournamentTeam>();
    }
}
