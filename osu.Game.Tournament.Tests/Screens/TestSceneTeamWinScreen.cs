// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Tournament.Models;
using osu.Game.Tournament.Screens.TeamWin;

namespace osu.Game.Tournament.Tests.Screens
{
    public class TestSceneTeamWinScreen : LadderTestScene
    {
        [Cached]
        private readonly Bindable<TournamentMatch> currentMatch = new Bindable<TournamentMatch>();

        [BackgroundDependencyLoader]
        private void load()
        {
            var match = new TournamentMatch();
            match.Team1.Value = Ladder.Teams.FirstOrDefault(t => t.Acronym.Value == "USA");
            match.Team2.Value = Ladder.Teams.FirstOrDefault(t => t.Acronym.Value == "JPN");
            match.Round.Value = Ladder.Rounds.FirstOrDefault(g => g.Name.Value == "Finals");
            currentMatch.Value = match;

            Add(new TeamWinScreen
            {
                FillMode = FillMode.Fit,
                FillAspectRatio = 16 / 9f
            });
        }
    }
}
