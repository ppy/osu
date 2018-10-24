// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Tournament.Screens.TeamIntro;

namespace osu.Game.Tournament.Tests
{
    public class TestCaseTeamIntro : LadderTestCase
    {
        [BackgroundDependencyLoader]
        private void load()
        {
            var team1 = Ladder.Teams.FirstOrDefault(t => t.Acronym == "USA");
            var team2 = Ladder.Teams.FirstOrDefault(t => t.Acronym == "JPN");

            var round = Ladder.Groupings.FirstOrDefault(g => g.Name == "Finals");

            Add(new TeamIntroScreen(team1, team2, round)
            {
                FillMode = FillMode.Fit,
                FillAspectRatio = 16 / 9f
            });
        }
    }
}
