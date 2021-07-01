// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Tournament.Screens.Setup;

namespace osu.Game.Tournament.Tests.Screens
{
    public class TestSceneStablePathSelectScreen : TournamentTestScene
    {
        public TestSceneStablePathSelectScreen()
        {
            AddStep("Add screen", () => Add(new StablePathSelectTestScreen()));
        }

        private class StablePathSelectTestScreen : StablePathSelectScreen
        {
            protected override void ChangePath()
            {
                Expire();
            }

            protected override void AutoDetect()
            {
                Expire();
            }
        }
    }
}
