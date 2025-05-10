// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Game.Tournament.Models;
using osu.Game.Tournament.Screens.Editors;

namespace osu.Game.Tournament.Tests.Screens
{
    public partial class TestSceneSeedingEditorScreen : TournamentScreenTestScene
    {
        [Cached]
        private readonly LadderInfo ladder = new LadderInfo();

        [BackgroundDependencyLoader]
        private void load()
        {
            var match = CreateSampleMatch();

            Add(new SeedingEditorScreen(match.Team1.Value.AsNonNull(), new TeamEditorScreen())
            {
                Width = 0.85f // create room for control panel
            });
        }
    }
}
