// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

namespace osu.Game.Tournament.Screens.Ladder.Components
{
    public class TournamentProgression
    {
        public int Item1;
        public int Item2;

        public bool Losers;

        public TournamentProgression(int item1, int item2, bool losers = false)
        {
            Item1 = item1;
            Item2 = item2;
            Losers = losers;
        }
    }
}
