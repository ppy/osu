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

            var round = Ladder.Groupings.First(g => g.Name == "Quarter Finals");

            Add(new TeamIntroScreen(team1, team2, round));
        }
    }
}
