// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Tournament.Screens.TeamWin;

namespace osu.Game.Tournament.Tests.Screens
{
    public class TestSceneTeamWinScreen : TournamentTestScene
    {
        [BackgroundDependencyLoader]
        private void load()
        {
            var match = Ladder.CurrentMatch.Value;

            match.Round.Value = Ladder.Rounds.FirstOrDefault(g => g.Name.Value == "Finals");
            match.Completed.Value = true;

            Add(new TeamWinScreen
            {
                FillMode = FillMode.Fit,
                FillAspectRatio = 16 / 9f
            });
        }
    }
}
