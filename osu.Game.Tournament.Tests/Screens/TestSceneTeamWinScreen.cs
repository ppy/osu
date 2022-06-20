// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Tournament.Screens.TeamWin;

namespace osu.Game.Tournament.Tests.Screens
{
    public class TestSceneTeamWinScreen : TournamentTestScene
    {
        [Test]
        public void TestBasic()
        {
            AddStep("set up match", () =>
            {
                var match = Ladder.CurrentMatch.Value;

                match.Round.Value = Ladder.Rounds.FirstOrDefault(g => g.Name.Value == "Finals");
                match.Completed.Value = true;
            });

            AddStep("create screen", () => Add(new TeamWinScreen
            {
                FillMode = FillMode.Fit,
                FillAspectRatio = 16 / 9f
            }));
        }
    }
}
