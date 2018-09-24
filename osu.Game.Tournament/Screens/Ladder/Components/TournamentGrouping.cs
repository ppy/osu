// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;

namespace osu.Game.Tournament.Screens.Ladder.Components
{
    public class TournamentGrouping
    {
        public string Name;
        public string Description;

        public int BestOf;

        public List<int> Pairings = new List<int>();
    }
}
