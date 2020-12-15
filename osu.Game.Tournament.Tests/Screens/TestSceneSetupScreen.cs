// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Game.Tournament.Screens;

namespace osu.Game.Tournament.Tests.Screens
{
    public class TestSceneSetupScreen : TournamentTestScene
    {
        [BackgroundDependencyLoader]
        private void load()
        {
            Add(new SetupScreen());
        }
    }
}
