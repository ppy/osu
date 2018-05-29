// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Online.Multiplayer;
using osu.Game.Screens.Multi.Screens.Match;

namespace osu.Game.Tests.Visual
{
    public class TestCaseMatch : OsuTestCase
    {
        public TestCaseMatch()
        {
            Room room = new Room
            {
            };

            Match match = new Match(room);

            AddStep(@"show", () => Add(match));
            AddStep(@"exit", match.Exit);
        }
    }
}
