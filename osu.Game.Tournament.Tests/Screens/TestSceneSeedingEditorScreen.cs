// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Game.Tournament.Models;
using osu.Game.Tournament.Screens.Editors;

namespace osu.Game.Tournament.Tests.Screens
{
    public class TestSceneSeedingEditorScreen : TournamentTestScene
    {
        [Cached]
        private readonly LadderInfo ladder = new LadderInfo();

        public TestSceneSeedingEditorScreen()
        {
            var match = CreateSampleMatch();

            Add(new SeedingEditorScreen(match.Team1.Value, new TeamEditorScreen())
            {
                Width = 0.85f // create room for control panel
            });
        }
    }
}
