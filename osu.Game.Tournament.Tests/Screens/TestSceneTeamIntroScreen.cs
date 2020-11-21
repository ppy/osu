// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Tournament.Models;
using osu.Game.Tournament.Screens.TeamIntro;

namespace osu.Game.Tournament.Tests.Screens
{
    public class TestSceneTeamIntroScreen : TournamentTestScene
    {
        [Cached]
        private readonly LadderInfo ladder = new LadderInfo();

        [BackgroundDependencyLoader]
        private void load()
        {
            var match = new TournamentMatch();
            match.Team1.Value = Ladder.Teams.FirstOrDefault(t => t.Acronym.Value == "USA");
            match.Team2.Value = Ladder.Teams.FirstOrDefault(t => t.Acronym.Value == "JPN");
            match.Round.Value = Ladder.Rounds.FirstOrDefault(g => g.Name.Value == "Finals");
            ladder.CurrentMatch.Value = match;

            Add(new TeamIntroScreen
            {
                FillMode = FillMode.Fit,
                FillAspectRatio = 16 / 9f
            });
        }
    }
}
