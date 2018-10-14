// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Linq;
using osu.Game.Tournament.Screens.TeamIntro;

namespace osu.Game.Tournament.Tests
{
    public class TestCaseTeamIntro : LadderTestCase
    {
        public TestCaseTeamIntro()
        {
            var team1 = Ladder.Teams.First(t => t.Acronym == "USA");
            var team2 = Ladder.Teams.First(t => t.Acronym == "JPN");

            var round = Ladder.Groupings.First(g => g.Name == "Finals");

            Add(new TeamIntroScreen(team1, team2, round));
        }
    }
}
